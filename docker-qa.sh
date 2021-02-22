#!/bin/bash
# Stop and remove containers with names like "m7-dev"
#Steps to run this file:
#If not already installed, install dos2unix: sudo apt install dos2unix 
#then run dos2unix file.sh
#then run this chmod +x file.sh to change permissions.
#then finally run bash file.sh
CONTAINER_NAME="m7-qa" 
IMAGE_NAME="api:qa"
OLD="$(docker ps --all --quiet --filter=name="$CONTAINER_NAME")"
if [ -n "$OLD" ]; then
  docker stop $OLD && docker rm $OLD
fi
sudo docker image rm -f $IMAGE_NAME
sudo docker build --rm -t $IMAGE_NAME -f DeviceConfigAPI/Dockerfile .
sudo docker run -d -it -p 5000:80/tcp --name $CONTAINER_NAME $IMAGE_NAME
