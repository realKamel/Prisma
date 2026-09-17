FROM python:3.14
RUN apt-get update && \
    apt-get install -y ffmpeg && \
    rm -rf /var/lib/apt/lists/*

RUN pip install --no-cache-dir minio

WORKDIR /app
ENV PYTHONUNBUFFERED=1

COPY worker.py .

CMD ["python", "-u", "worker.py"]
