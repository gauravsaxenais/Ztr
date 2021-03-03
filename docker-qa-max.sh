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
echo "Stopping container and removing the said container..."
if [ -n "$OLD" ]; then
  docker stop $OLD && docker rm $OLD
fi

echo "Removing dangling images..."
docker images --no-trunc -q -f dangling=true | xargs -r docker rmi

echo "Removing unused docker images"
images=($(docker images --digests | tail -n +2 | awk '{ img_id=$1; if($2!="<none>")img_id=img_id":"$2; if($3!="<none>") img_id=img_id"@"$3; print img_id}'))
containers=($(docker ps -a | tail -n +2 | awk '{print $2}'))
containers_reg=" ${containers[*]} "
remove=()

for item in ${images[@]}; do
  if [[ ! $containers_reg =~ " $item " ]]; then
    remove+=($item)
  fi
done

remove_images=" ${remove[*]} "
echo ${remove_images} | xargs -r docker rmi

echo "Building api image..."
sudo docker build --rm -t $IMAGE_NAME -f DeviceConfigAPI/Dockerfile .
echo "Setting up container..."
sudo docker run -d -it -p 5000:80/tcp --name $CONTAINER_NAME $IMAGE_NAME
echo "Done.."
