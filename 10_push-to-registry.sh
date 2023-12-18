#!/bin/sh
REGISTRY=ghcr.io/informatievlaanderen
IMAGE=kafka-topic-transfer
VERSION=`cat VERSION | tr -d " \t\n\r"`

# Push container to registry
docker push ${REGISTRY}/${IMAGE}:${VERSION}