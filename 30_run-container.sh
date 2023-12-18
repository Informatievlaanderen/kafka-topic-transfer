#!/bin/sh
REGISTRY=ghcr.io/informatievlaanderen
IMAGE=kafka-topic-transfer
VERSION=`cat VERSION | tr -d " \t\n\r"`
APPSETTINGS_PATH=$1

docker run -it --rm \
 -v "${APPSETTINGS_PATH}:/app/appsettings.json" \
 ${REGISTRY}/${IMAGE}:${VERSION}
