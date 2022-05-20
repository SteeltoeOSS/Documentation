#!/usr/bin/env bash

set -e

# =========================================================================== #
# wrapper for ytt                                                             #
# =========================================================================== #

base_dir=$(dirname $0)
ytt=$base_dir/tools/ytt

if [ ! -x $ytt ]; then
  ytt_version=$(grep '^ytt_version=' $base_dir/tool.properties | cut -d= -f2)
  ytt_mirror=$(grep '^ytt_mirror=' $base_dir/tool.properties | cut -d= -f2)
  case $(uname -s) in
    Darwin) platform=darwin ;;
    Linux) platform=linux ;;
    *)
      echo "unsupported platform: $(uname -s)"
      exit 1
      ;;
  esac
  ytt_url=$ytt_mirror/$ytt_version/ytt-$platform-amd64
  mkdir -p $(dirname $ytt)
  wget $ytt_url -O $ytt
  chmod +x $ytt
fi

exec $ytt $*
