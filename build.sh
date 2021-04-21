#!/usr/bin/env bash

set -e

base_dir=$(dirname $0)

git_sources_url=https://github.com/SteeltoeOSS/Steeltoe
v2_sources=2.x
v3_sources=main
build_dir=build

set_color=tput
color_ok="setaf 2"
color_error="setaf 1"
color_reset="sgr0"

usage() {
    cat <<EOF
USAGE
     $(basename $0) [-h]

OPTIONS
     -h      print this message
     -d      build directory (default $build_dir)
     -2      V2 API source branch/tag (default $v2_sources)
     -3      V3 API source branch/tag (default $v3_sources)
EOF
}

msg() {
  [ $TERM == dumb  ] || $set_color $color_ok
  echo "--- "$*
  [ $TERM == dumb ] || $set_color $color_reset
}

err() {
  [ $TERM == dumb ] || $set_color $color_error
  echo "!!! "$* >&2
  [ $TERM == dumb ] || $set_color $color_reset
}

die() {
  err $*
  exit 1
}

get_sources() {
  local dest_dir=$1
  local label=$2
  msg "$(basename $dest_dir) sources"
  if [ -d $dest_dir ]; then
    (
      cd $dest_dir
      git checkout $label
    )
  else
    echo mkdir -p $(dirname $dest_dir)
    git clone $git_sources_url $dest_dir -b $label
  fi
}

while getopts ":d:h2:3:" opt ; do
  case $opt in
    h)
      usage
      exit
      ;;
    d)
      build_dir=$OPTARG
      ;;
    2)
      v2_sources=$OPTARG
      ;;
    3)
      v3_sources=$OPTARG
      ;;
    \?)
      die "invalid option -$OPTARG; run with -h for help"
      ;;
    :)
      die "option -$OPTARG requires an argument; run with -h for help"
      ;;
  esac
done
shift $(($OPTIND - 1))

cd $base_dir

get_sources sources/v2 $v2_sources
get_sources sources/v3 $v3_sources

msg "building v2 metadata"
docfx metadata api-v2.json
msg "building v3 metadata"
docfx metadata api-v3.json
msg "building all metadata"
docfx metadata api-all.json
msg "building site"
rm -rf $build_dir
docfx build -o $build_dir --globalMetadataFiles devhost.json
