#!/usr/bin/env bash

# set -x
set -eo pipefail

die () {
    log $*
    exit 1
}

log () {
    echo "$(basename $0): $*"
}

docenv=$DOC_ENVIRONMENT
docenv_home=${DOC_ENVIRONMENT_HOME:-/etc/documentation-site}

case $(uname) in
    Darwin)
        sed=gsed ;;
    *)
        sed=sed ;;
esac

source $docenv_home/default.env

if [[ -z $docenv ]]; then
    log "using default configuration"
else
    log "using $docenv configuration"
    docenv_file=$docenv_home/$docenv.env
    if [[ ! -f $docenv_file ]]; then
        log "env file not found: $docenv_file"
        return
    fi
    source $docenv_file
fi

if [[ -n $mainsite_host ]]; then
    if [[ -z $default_mainsite_host ]]; then
        die "default_mainsite_host not defined"
    fi
    if [[ -z $html_dir ]]; then
        die "html_dir not defined"
    fi
    if [[ ! -d $html_dir ]]; then
        die "html_dir does not exist or is not a directory: $html_dir"
    fi
    log "updating documentation site to use $mainsite_host"
    find $html_dir -type f | xargs $sed -i 's_'$default_mainsite_host'_'$mainsite_host'_g'
fi
