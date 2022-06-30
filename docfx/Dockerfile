FROM mono:6.12
WORKDIR /scratch
RUN \
    apt-get update && \
    apt-get install -y \
        git \
        unzip \
        vim \
        wget
RUN \
    wget https://packages.microsoft.com/config/debian/$(cat /etc/issue | awk '{print $3}')/packages-microsoft-prod.deb -O packages-microsoft-prod.deb && \
    dpkg -i packages-microsoft-prod.deb && \
    rm packages-microsoft-prod.deb && \
    apt-get update && \
    apt install -y \
    dotnet-sdk-6.0
COPY . .
RUN \
    wget https://github.com/dotnet/docfx/releases/download/$(cat docfx_version)/docfx.zip && \
    mkdir -p /usr/local/libexec && \
    unzip docfx.zip -d /usr/local/libexec/docfx && \
    install docfx /usr/local/bin && \
    rm -rf /scratch
WORKDIR /work
ENTRYPOINT ["docfx"]
