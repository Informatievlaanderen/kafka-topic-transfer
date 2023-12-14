IMAGE=kafka-transfer
VERSION=development
APPSETTINGS_FILE=$1

dotnet build -c Release
docker build -t ${IMAGE}:${VERSION} -f Dockerfile.local .
docker run -it --rm -v "./${APPSETTINGS_FILE}:/app/appsettings.json" ${IMAGE}:${VERSION}
