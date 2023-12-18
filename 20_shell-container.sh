#!/bin/sh
REGISTRY=ghcr.io/informatievlaanderen
IMAGE=kafka-topic-transfer
VERSION=`cat VERSION | tr -d " \t\n\r"`
PROJECT_PATH=./src/kafka.transfer.app
DOCKERFILE_PATH=./src/kafka.transfer.app/Dockerfile.local
APPSETTINGS_PATH=./src/kafka.transfer.app/appsettings.linux.json
ENTRYPOINT=/bin/bash
docker run -it --rm \
 -v "${APPSETTINGS_PATH}:/app/appsettings.json" \
 --entrypoint ${ENTRYPOINT} \
 ${REGISTRY}/${IMAGE}:${VERSION}