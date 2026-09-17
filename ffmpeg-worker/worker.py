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
        print(f"[{guid}] 1/4 Downloading {object_name} from MinIO...")
        client.fget_object(BUCKET, object_name, input_path)

        print(f"[{guid}] 2/4 Transcoding to HLS (this may take a moment)...")
        # Added stderr=subprocess.PIPE to capture exact FFmpeg errors if it fails
        video_result = subprocess.run([
            "ffmpeg", "-y", "-i", input_path,
            "-c:v", "libx264", "-preset", "fast", "-crf", "23",
            "-c:a", "aac", "-b:a", "128k",
            "-hls_time", "6", "-hls_playlist_type", "vod",
            "-hls_segment_filename", os.path.join(hls_dir, "segment_%03d.ts"),
            os.path.join(hls_dir, "index.m3u8")
        ], capture_output=True, text=True)

        if video_result.returncode != 0:
            raise Exception(f"FFmpeg video transcode failed:\n{video_result.stderr}")

        print(f"[{guid}] 3/4 Extracting audio-only rendition...")
        audio_result = subprocess.run([
            "ffmpeg", "-y", "-i", input_path,
            "-vn", "-c:a", "aac", "-b:a", "128k",
            os.path.join(hls_dir, "audio.m4a")
        ], capture_output=True, text=True)

        if audio_result.returncode != 0:
            raise Exception(f"FFmpeg audio extract failed:\n{audio_result.stderr}")

        print(f"[{guid}] 4/4 Uploading HLS files to videos/{guid}/...")
        for filename in os.listdir(hls_dir):
            file_path = os.path.join(hls_dir, filename)
            target = f"videos/{guid}/{filename}"

            content_type = "application/vnd.apple.mpegurl" if filename.endswith(".m3u8") \
                else "video/mp2t" if filename.endswith(".ts") else "audio/mp4"

            client.fput_object(BUCKET, target, file_path, content_type=content_type)

        # Clean up the original raw upload to save space
        client.remove_object(BUCKET, object_name)
        print(f"[{guid}] ✅ SUCCESS: HLS available at videos/{guid}/index.m3u8")

    except S3Error as e:
        print(f"[{guid}] ❌ MinIO Error: {e}")
    except Exception as e:
        print(f"[{guid}] ❌ Processing Error: {e}")
    finally:
        # Always clean up local temp files, even if it failed
        shutil.rmtree(work_dir, ignore_errors=True)


def main():
    print(f"FFmpeg Worker started.", flush=True)
    print(f"Target Endpoint: {ENDPOINT}", flush=True)
    print(f"Target Bucket: {BUCKET}", flush=True)

    # --- CRITICAL CONNECTION TEST ---
    print("Testing connection to MinIO...", flush=True)
    try:
        # This will timeout in 3 seconds if the hostname is wrong or MinIO is down
        urllib.request.urlopen(f"http://{ENDPOINT}/minio/health/live", timeout=3)
        print("✅ MinIO is reachable!\n", flush=True)
    except Exception as e:
        print(f"❌ FATAL: Cannot reach MinIO at http://{ENDPOINT}", flush=True)
        print(f"Error: {e}", flush=True)
        print("\n💡 FIX:", flush=True)
        print("   - If running LOCALLY (outside Docker), run: set S3_ENDPOINT=127.0.0.1:9000", flush=True)
        print("   - If running IN DOCKER, ensure 'object-storage' matches your docker-compose service name.",
              flush=True)
        sys.exit(1)
    # --------------------------------

    while True:
        try:
            print(f"[+] Polling MinIO for new files in '{BUCKET}/uploads/'...", flush=True)

            objects = list(client.list_objects(BUCKET, prefix="uploads/", recursive=True))

            print(f"    -> Found {len(objects)} total object(s) in bucket.", flush=True)

            mp4_files = [obj for obj in objects if obj.object_name.endswith(".mp4")]
            print(f"    -> Found {len(mp4_files)} .mp4 file(s) ready to process.", flush=True)

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
