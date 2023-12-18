#!/bin/sh
REGISTRY=ghcr.io/informatievlaanderen
IMAGE=kafka-topic-transfer
VERSION=`cat VERSION | tr -d " \t\n\r"`
PROJECT_PATH=./src/kafka.transfer.app
DOCKERFILE_PATH=./src/kafka.transfer.app/Dockerfile.local

#Release build
dotnet build -c Release

# Remove excisting container with same version
docker image rm -f ${REGISTRY}/${IMAGE}:${VERSION}

# Build container
docker build -t ${REGISTRY}/${IMAGE}:${VERSION} -f ${DOCKERFILE_PATH} ${PROJECT_PATH}
