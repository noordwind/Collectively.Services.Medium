#!/bin/bash
DOCKER_ENV=''
DOCKER_TAG=''
case "$TRAVIS_BRANCH" in
  "master")
    DOCKER_ENV=production
    DOCKER_TAG=latest
    ;;
  "develop")
    DOCKER_ENV=development
    DOCKER_TAG=dev
    ;;    
esac

docker login -u $DOCKER_USERNAME -p $DOCKER_PASSWORD
docker build -f ./src/Collectively.Services.Medium/Dockerfile.$DOCKER_ENV -t collectively.services.medium:$DOCKER_TAG ./src/Collectively.Services.Medium
docker tag collectively.services.medium:$DOCKER_TAG $DOCKER_USERNAME/collectively.services.medium:$DOCKER_TAG
docker push $DOCKER_USERNAME/collectively.services.medium:$DOCKER_TAG