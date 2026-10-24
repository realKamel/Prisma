use anyhow::{anyhow, bail, Context, Result};
use aws_credential_types::Credentials;
use aws_sdk_s3::{
    config::Region,
    primitives::ByteStream,
    Client,
};
use std::{
    env,
    path::Path,
    process::Stdio,
    time::Duration,
};
use tempfile::tempdir;
use tokio::{
    fs::{self, File},
    io::copy,
    process::Command,
    time::sleep,
};
use walkdir::WalkDir;

#[derive(Clone)]
struct Config {
    endpoint: String,
    access_key: String,
    secret_key: String,
    bucket: String,
    poll_interval: u64,
}

impl Config {
    fn from_env() -> Self {
        let raw_endpoint = env::var("S3_ENDPOINT")
            .unwrap_or_else(|_| "object-storage:9000".to_string());

        let endpoint = raw_endpoint
            .trim()
            .trim_start_matches("https://")
            .trim_start_matches("http://")
            .trim_end_matches('/')
            .to_string();

        Self {
            endpoint,
            access_key: env::var("S3_ACCESS_KEY_ID")
                .unwrap_or_else(|_| "admin".to_string()),
            secret_key: env::var("S3_SECRET_ACCESS_KEY")
                .unwrap_or_else(|_| "admin123".to_string()),
            bucket: env::var("S3_BUCKET_NAME")
                .unwrap_or_else(|_| "prisma-bucket".to_string()),
            poll_interval: env::var("POLL_INTERVAL")
                .unwrap_or_else(|_| "5".to_string())
                .parse()
                .unwrap_or(5),
        }
    }

    fn s3_endpoint(&self) -> String {
        format!("http://{}", self.endpoint)
    }
}

struct Quality {
    name: &'static str,
    scale: &'static str,
    bitrate: &'static str,
    maxrate: &'static str,
    bufsize: &'static str,
    crf: &'static str,
}

const QUALITIES: [Quality; 3] = [
    Quality {
        name: "1080p",
        scale: "scale=-2:1080",
        bitrate: "5000k",
        maxrate: "5350k",
        bufsize: "7500k",
        crf: "20",
    },
    Quality {
        name: "720p",
        scale: "scale=-2:720",
        bitrate: "2800k",
        maxrate: "2996k",
        bufsize: "4200k",
        crf: "23",
    },
    Quality {
        name: "480p",
        scale: "scale=-2:480",
        bitrate: "1400k",
        maxrate: "1498k",
        bufsize: "2100k",
        crf: "25",
    },
];

fn create_s3_client(config: &Config) -> Client {
    let shared_config = aws_sdk_s3::Config::builder()
        .behavior_version(
            aws_sdk_s3::config::BehaviorVersion::latest()
        )
        .region(Region::new("us-east-1"))
        .credentials_provider(Credentials::new(
            config.access_key.clone(),
            config.secret_key.clone(),
            None,
            None,
            "minio-worker",
        ))
        .endpoint_url(config.s3_endpoint())
        .force_path_style(true)
        .build();

    Client::from_conf(shared_config)
}
async fn check_minio(config: &Config) -> Result<()> {
    let health_url = format!("{}/minio/health/live", config.s3_endpoint());

    let response = reqwest::Client::builder()
        .timeout(Duration::from_secs(3))
        .build()?
        .get(&health_url)
        .send()
        .await
        .with_context(|| format!("Cannot reach MinIO at {health_url}"))?;

    if !response.status().is_success() {
        bail!(
            "MinIO health check failed: HTTP {}",
            response.status()
        );
    }

    Ok(())
}

async fn run_ffmpeg(args: &[String], description: &str) -> Result<()> {
    let output = Command::new("ffmpeg")
        .args(args)
        .stdout(Stdio::null())
        .stderr(Stdio::piped())
        .output()
        .await
        .context("Failed to start FFmpeg. Is it installed?")?;

    if !output.status.success() {
        let stderr = String::from_utf8_lossy(&output.stderr);
        bail!("{description} failed:\n{stderr}");
    }

    Ok(())
}

async fn download_object(
    client: &Client,
    bucket: &str,
    object_name: &str,
    destination: &Path,
) -> Result<()> {
    let response = client
        .get_object()
        .bucket(bucket)
        .key(object_name)
        .send()
        .await
        .with_context(|| format!("Failed to download {object_name}"))?;

    // Stream the body to disk instead of loading the entire video
    // into memory.
    let mut reader = response.body.into_async_read();
    let mut file = File::create(destination).await?;

    copy(&mut reader, &mut file)
        .await
        .context("Failed while writing downloaded video")?;

    Ok(())
}

async fn transcode(
    client: &Client,
    config: &Config,
    object_name: &str,
    guid: &str,
) -> Result<()> {
    let work_dir = tempdir().context("Failed to create temporary directory")?;
    let input_path = work_dir.path().join("input.mp4");
    let hls_dir = work_dir.path().join("hls");

    fs::create_dir_all(&hls_dir).await?;

    println!("[{guid}] 1/6 Downloading {object_name} from MinIO...");
    download_object(client, &config.bucket, object_name, &input_path).await?;

    println!(
        "[{guid}] 2/6 Transcoding video to 1080p, 720p and 480p..."
    );

    for quality in &QUALITIES {
        let quality_dir = hls_dir.join(quality.name);
        fs::create_dir_all(&quality_dir).await?;

        let segment_pattern = quality_dir.join("segment_%03d.ts");
        let playlist = quality_dir.join("index.m3u8");

        let args = vec![
            "-y".into(),
            "-i".into(),
            input_path.to_string_lossy().into_owned(),
            "-vf".into(),
            quality.scale.into(),
            "-c:v".into(),
            "libx264".into(),
            "-preset".into(),
            "fast".into(),
            "-crf".into(),
            quality.crf.into(),
            "-b:v".into(),
            quality.bitrate.into(),
            "-maxrate".into(),
            quality.maxrate.into(),
            "-bufsize".into(),
            quality.bufsize.into(),
            "-c:a".into(),
            "aac".into(),
            "-b:a".into(),
            "128k".into(),
            "-hls_time".into(),
            "6".into(),
            "-hls_playlist_type".into(),
            "vod".into(),
            "-hls_segment_filename".into(),
            segment_pattern.to_string_lossy().into_owned(),
            playlist.to_string_lossy().into_owned(),
        ];

        run_ffmpeg(&args, &format!("FFmpeg {} transcode", quality.name))
            .await?;
    }

    println!("[{guid}] 3/6 Extracting standalone audio...");
    let audio_path = hls_dir.join("audio.m4a");

    let audio_args = vec![
        "-y".into(),
        "-i".into(),
        input_path.to_string_lossy().into_owned(),
        "-vn".into(),
        "-c:a".into(),
        "aac".into(),
        "-b:a".into(),
        "128k".into(),
        audio_path.to_string_lossy().into_owned(),
    ];

    run_ffmpeg(&audio_args, "FFmpeg audio extraction").await?;

    println!("[{guid}] 4/6 Creating master playlist...");
    let master_playlist = concat!(
        "#EXTM3U\n",
        "#EXT-X-VERSION:3\n",
        "#EXT-X-STREAM-INF:BANDWIDTH=5500000,RESOLUTION=1920x1080\n",
        "1080p/index.m3u8\n",
        "#EXT-X-STREAM-INF:BANDWIDTH=3100000,RESOLUTION=1280x720\n",
        "720p/index.m3u8\n",
        "#EXT-X-STREAM-INF:BANDWIDTH=1600000,RESOLUTION=854x480\n",
        "480p/index.m3u8\n",
    );

    fs::write(hls_dir.join("master.m3u8"), master_playlist).await?;

    println!("[{guid}] 5/6 Uploading output files...");

    for entry in WalkDir::new(&hls_dir).into_iter() {
        let entry = entry.context("Failed to enumerate HLS output files")?;

        if !entry.file_type().is_file() {
            continue;
        }

        let file_path = entry.path();
        let relative_path = file_path
            .strip_prefix(&hls_dir)?
            .to_string_lossy()
            .replace('\\', "/");

        let target = format!("videos/{guid}/{relative_path}");

        let content_type = match file_path.extension().and_then(|ext| ext.to_str()) {
            Some("m3u8") => "application/vnd.apple.mpegurl",
            Some("ts") => "video/mp2t",
            Some("m4a") => "audio/mp4",
            _ => "application/octet-stream",
        };

        let body = ByteStream::from_path(file_path)
            .await
            .with_context(|| format!("Failed to read {}", file_path.display()))?;

        client
            .put_object()
            .bucket(&config.bucket)
            .key(&target)
            .content_type(content_type)
            .body(body)
            .send()
            .await
            .with_context(|| format!("Failed to upload {target}"))?;

        println!("[{guid}] Uploaded {target}");
    }

    // Delete the raw upload only after every output has been uploaded.
    println!("[{guid}] 6/6 Deleting original upload...");
    client
        .delete_object()
        .bucket(&config.bucket)
        .key(object_name)
        .send()
        .await
        .with_context(|| format!("Failed to delete original {object_name}"))?;

    println!(
        "[{guid}] SUCCESS: videos/{guid}/master.m3u8 and \
         videos/{guid}/audio.m4a are available"
    );

    // tempdir is automatically removed when dropped.
    Ok(())
}

async fn poll_once(client: &Client, config: &Config) -> Result<()> {
    println!(
        "[+] Polling {}/uploads/ for MP4 files...",
        config.bucket
    );

    let mut continuation_token: Option<String> = None;
    let mut objects = Vec::new();

    // Fetch all pages; a single ListObjectsV2 request is limited.
    loop {
        let mut request = client
            .list_objects_v2()
            .bucket(&config.bucket)
            .prefix("uploads/");

        if let Some(token) = continuation_token.as_deref() {
            request = request.continuation_token(token);
        }

        let response = request
            .send()
            .await
            .context("Failed to list objects in MinIO")?;

        for object in response.contents() {
            if let Some(key) = object.key() {
                if key.ends_with(".mp4") {
                    objects.push(key.to_string());
                }
            }
        }

        if !response.is_truncated().unwrap_or(false) {
            break;
        }

        continuation_token = response.next_continuation_token().map(str::to_owned);

        if continuation_token.is_none() {
            break;
        }
    }

    for object_name in objects {
        let filename = object_name
            .rsplit('/')
            .next()
            .unwrap_or(&object_name);

        let Some(guid) = filename.strip_suffix(".mp4") else {
            continue;
        };

        println!("\n[+] New upload detected: {object_name}");

        if let Err(error) = transcode(client, config, &object_name, guid).await {
            eprintln!(
                "[{guid}] Processing failed: {error:#}. \
                 Original object will be retained if it still exists."
            );
        }
    }

    Ok(())
}

#[tokio::main]
async fn main() -> Result<()> {
    let config = Config::from_env();

    println!("FFmpeg ABR + Audio Worker started.");
    println!("Target Endpoint: {}", config.endpoint);
    println!("Target Bucket: {}", config.bucket);

    check_minio(&config).await?;

    println!("MinIO is reachable!");

    // Check that FFmpeg exists before entering the polling loop.
    Command::new("ffmpeg")
        .arg("-version")
        .stdout(Stdio::null())
        .stderr(Stdio::null())
        .status()
        .await
        .context("FFmpeg is not installed or not on PATH")?
        .success()
        .then_some(())
        .ok_or_else(|| anyhow!("FFmpeg version check failed"))?;

    let client = create_s3_client(&config);

    loop {
        if let Err(error) = poll_once(&client, &config).await {
            eprintln!(
                "Worker polling error: {error:#}. \
                 Check MinIO connectivity and credentials."
            );
        }

        println!(
            "[*] Sleeping for {} seconds...\n",
            config.poll_interval
        );

        sleep(Duration::from_secs(config.poll_interval)).await;
    }
}
