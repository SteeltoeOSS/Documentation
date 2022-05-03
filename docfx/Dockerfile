FROM mono:6.12
RUN \
    apt-get update && \
    apt-get install -y \
        unzip \
        wget
COPY . /work
RUN \
    /work/install.sh && \
    rm -rf /work
