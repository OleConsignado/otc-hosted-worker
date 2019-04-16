#!/bin/bash

set -e

CICD_COMMON_VERSION="v2.2"

export CLASS_LIBRARY_PROJ_DIR=Source/Otc.HostedWorker.Abstractions
#export TEST_PROJ_DIR=Source/Otc.HostedWorker.Tests

cd $TRAVIS_BUILD_DIR

wget -q https://raw.githubusercontent.com/OleConsignado/otc-cicd-common/$CICD_COMMON_VERSION/cicd-common.sh -O ./cicd-common.sh
chmod +x ./cicd-common.sh

./cicd-common.sh $@

export CLASS_LIBRARY_PROJ_DIR=Source/Otc.HostedWorker

./cicd-common.sh $@

export CLASS_LIBRARY_PROJ_DIR=Source/Otc.WebHostedWorker

./cicd-common.sh $@

export CLASS_LIBRARY_PROJ_DIR=Source/Otc.WebHostedWorkerAdapter.Abstractions

./cicd-common.sh $@

export CLASS_LIBRARY_PROJ_DIR=Source/Otc.WebHostedWorkerAdapter

./cicd-common.sh $@

