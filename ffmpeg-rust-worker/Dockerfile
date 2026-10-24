# Stage 1: Build the Rust worker
FROM rust:1-slim-trixie AS builder

WORKDIR /app

COPY Cargo.toml Cargo.lock ./
COPY src ./src

RUN cargo build --release

# Stage 2: Runtime
FROM debian:trixie-slim

RUN apt-get update && \
    apt-get install -y --no-install-recommends ffmpeg ca-certificates && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /app

COPY --from=builder /app/target/release/ffmpeg-worker /usr/local/bin/ffmpeg-worker

ENV RUST_LOG=info

CMD ["ffmpeg-worker"]
