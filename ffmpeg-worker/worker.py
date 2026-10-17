import os
import time
import subprocess
import tempfile
import shutil
import urllib.request
import sys
from minio import Minio
from minio.error import S3Error

# Safely parse endpoint (removes http:// or https:// if present, as Minio SDK expects just host:port)
raw_endpoint = os.environ.get("S3_ENDPOINT", "object-storage:9000")
ENDPOINT = raw_endpoint.replace("http://", "").replace("https://", "")
ACCESS_KEY = os.environ.get("S3_ACCESS_KEY_ID", "admin")
SECRET_KEY = os.environ.get("S3_SECRET_ACCESS_KEY", "admin123")
BUCKET = os.environ.get("S3_BUCKET_NAME", "prisma-bucket")
POLL_INTERVAL = int(os.environ.get("POLL_INTERVAL", "5"))

# Initialize MinIO client (secure=False because we are using internal Docker HTTP)
client = Minio(ENDPOINT, access_key=ACCESS_KEY, secret_key=SECRET_KEY, secure=False)


def transcode(object_name: str, guid: str):
    work_dir = tempfile.mkdtemp()
    input_path = os.path.join(work_dir, "input.mp4")
    hls_dir = os.path.join(work_dir, "hls")
    os.makedirs(hls_dir, exist_ok=True)

    try:
        print(f"[{guid}] 1/6 Downloading {object_name} from MinIO...")
        client.fget_object(BUCKET, object_name, input_path)

        # Define qualities for ABR
        qualities = [
            {"name": "1080p", "scale": "scale=-2:1080", "b:v": "5000k", "maxrate": "5350k", "bufsize": "7500k",
             "crf": "20"},
            {"name": "720p", "scale": "scale=-2:720", "b:v": "2800k", "maxrate": "2996k", "bufsize": "4200k",
             "crf": "23"},
            {"name": "480p", "scale": "scale=-2:480", "b:v": "1400k", "maxrate": "1498k", "bufsize": "2100k",
             "crf": "25"}
        ]

        print(f"[{guid}] 2/6 Transcoding video to multiple qualities (1080p, 720p, 480p)...")
        for q in qualities:
            q_dir = os.path.join(hls_dir, q["name"])
            os.makedirs(q_dir, exist_ok=True)

            variant_result = subprocess.run([
                "ffmpeg", "-y", "-i", input_path,
                "-vf", q["scale"],
                "-c:v", "libx264", "-preset", "fast", "-crf", q["crf"],
                "-b:v", q["b:v"], "-maxrate", q["maxrate"], "-bufsize", q["bufsize"],
                "-c:a", "aac", "-b:a", "128k",
                "-hls_time", "6", "-hls_playlist_type", "vod",
                "-hls_segment_filename", os.path.join(q_dir, "segment_%03d.ts"),
                os.path.join(q_dir, "index.m3u8")
            ], capture_output=True, text=True)

            if variant_result.returncode != 0:
                raise Exception(f"FFmpeg transcode failed for {q['name']}:\n{variant_result.stderr}")

        print(f"[{guid}] 3/6 Extracting standalone audio-only rendition (audio.m4a)...")
        audio_result = subprocess.run([
            "ffmpeg", "-y", "-i", input_path,
            "-vn", "-c:a", "aac", "-b:a", "128k",
            os.path.join(hls_dir, "audio.m4a")
        ], capture_output=True, text=True)

        if audio_result.returncode != 0:
            raise Exception(f"FFmpeg audio extract failed:\n{audio_result.stderr}")

        print(f"[{guid}] 4/6 Creating Master Playlist (master.m3u8)...")
        master_playlist_path = os.path.join(hls_dir, "master.m3u8")
        with open(master_playlist_path, "w") as f:
            f.write("#EXTM3U\n")
            f.write("#EXT-X-VERSION:3\n")
            f.write('#EXT-X-STREAM-INF:BANDWIDTH=5500000,RESOLUTION=1920x1080\n1080p/index.m3u8\n')
            f.write('#EXT-X-STREAM-INF:BANDWIDTH=3100000,RESOLUTION=1280x720\n720p/index.m3u8\n')
            f.write('#EXT-X-STREAM-INF:BANDWIDTH=1600000,RESOLUTION=854x480\n480p/index.m3u8\n')

        print(f"[{guid}] 5/6 Uploading all files to videos/{guid}/...")
        # Walk through temp directory and upload maintaining nested/flat structure
        for root, dirs, files in os.walk(hls_dir):
            for file in files:
                file_path = os.path.join(root, file)
                rel_path = os.path.relpath(file_path, hls_dir)
                target = f"videos/{guid}/{rel_path.replace(os.sep, '/')}"

                # Assign appropriate MIME types
                if file.endswith(".m3u8"):
                    content_type = "application/vnd.apple.mpegurl"
                elif file.endswith(".ts"):
                    content_type = "video/mp2t"
                elif file.endswith(".m4a"):
                    content_type = "audio/mp4"
                else:
                    content_type = "application/octet-stream"

                client.fput_object(BUCKET, target, file_path, content_type=content_type)

        # Clean up the original raw upload to save space
        client.remove_object(BUCKET, object_name)
        print(
            f"[{guid}] ✅ SUCCESS: ABR HLS available at videos/{guid}/master.m3u8 and audio at videos/{guid}/audio.m4a")

    except S3Error as e:
        print(f"[{guid}] ❌ MinIO Error: {e}")
    except Exception as e:
        print(f"[{guid}] ❌ Processing Error: {e}")
    finally:
        # Always clean up local temp files, even if it failed
        shutil.rmtree(work_dir, ignore_errors=True)


def main():
    print(f"FFmpeg ABR + Audio Worker started.", flush=True)
    print(f"Target Endpoint: {ENDPOINT}", flush=True)
    print(f"Target Bucket: {BUCKET}", flush=True)

    # --- CRITICAL CONNECTION TEST ---
    print("Testing connection to MinIO...", flush=True)
    try:
        urllib.request.urlopen(f"http://{ENDPOINT}/minio/health/live", timeout=3)
        print("✅ MinIO is reachable!\n", flush=True)
    except Exception as e:
        print(f"❌ FATAL: Cannot reach MinIO at http://{ENDPOINT}", flush=True)
        print(f"Error: {e}", flush=True)
        sys.exit(1)
    # --------------------------------

    while True:
        try:
            print(f"[+] Polling MinIO for new files in '{BUCKET}/uploads/'...", flush=True)

            objects = list(client.list_objects(BUCKET, prefix="uploads/", recursive=True))
            mp4_files = [obj for obj in objects if obj.object_name.endswith(".mp4")]

            for obj in mp4_files:
                print(f"\n📥 New upload detected: {obj.object_name}", flush=True)
                guid = obj.object_name.split("/")[-1].replace(".mp4", "")
                transcode(obj.object_name, guid)

        except S3Error as e:
            print(f"❌ S3 Connection Error: {e}. Check your endpoint and credentials.", flush=True)
        except Exception as e:
            print(f"❌ Unexpected Worker Error: {e}", flush=True)

        print(f"[*] Sleeping for {POLL_INTERVAL} seconds...\n", flush=True)
        time.sleep(POLL_INTERVAL)


if __name__ == "__main__":
    main()
