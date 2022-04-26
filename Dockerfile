FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
RUN \
    apt-get update && \
    apt-get install -y \
        git \
        gnupg \
        unzip \
        wget
RUN \
    apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF && \
    echo "deb https://download.mono-project.com/repo/debian stable-buster main" | tee /etc/apt/sources.list.d/mono-official-stable.list && \
    apt-get update && \
    apt-get install -y \
        mono-runtime \
        mono-devel \
        msbuild
RUN \
    wget https://github.com/dotnet/docfx/releases/download/v2.57.2/docfx.zip && \
    mkdir -p /usr/local/libexec && \
    unzip docfx.zip -d /usr/local/libexec/docfx && \
    rm docfx.zip && \
    echo "#!/bin/sh\nmono /usr/local/libexec/docfx/docfx.exe \$*" > /usr/local/bin/docfx && \
    chmod +x /usr/local/bin/docfx
WORKDIR /docs
COPY . .
RUN \
    ./build.sh \
        -3 3.1.3 \
        -2 2.5.4

FROM nginx:1.19
COPY --from=build /docs/build/_site /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
