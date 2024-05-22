#!/usr/bin/env bash

set -e
set -o pipefail

base_dir=$(dirname $0)
cd $base_dir

git_sources_url=https://github.com/SteeltoeOSS/Steeltoe
v2_sources=$(cat metadata.conf | grep '^2:' | cut -d: -f2)
v3_sources=$(cat metadata.conf | grep '^3:' | cut -d: -f2)
v4_sources=$(cat metadata.conf | grep '^4:' | cut -d: -f2)
build_dir=build

get_sources() {
  local dest_dir=$1
  local branch=$2
  echo "$(basename $dest_dir) sources from $branch"
  [ -d $dest_dir ] && rm -rf $dest_dir
  git clone $git_sources_url $dest_dir -b $branch --depth 1
}

get_sources sources/v2 $v2_sources
get_sources sources/v3 $v3_sources
get_sources sources/v4 $v4_sources

echo "building v2 metadata"
docfx metadata api-v2.json

get_sources sources/v3 $v3_sources
echo "building v3 metadata"
docfx metadata api-v3.json
echo "building v4 metadata"
docfx metadata api-v4.json
echo "building all metadata"
docfx metadata api-all.json
