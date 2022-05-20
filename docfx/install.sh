#!/usr/bin/env bash

set -e

cd "$(dirname "${BASH_SOURCE[0]}")"

version=$(cat version)
wget https://github.com/dotnet/docfx/releases/download/${version}/docfx.zip
mkdir -p /usr/local/libexec
unzip docfx.zip -d /usr/local/libexec/docfx
rm docfx.zip

cat <<EOF >/usr/local/bin/docfx
#!/bin/sh
mono /usr/local/libexec/docfx/docfx.exe \$*
EOF

chmod +x /usr/local/bin/docfx
