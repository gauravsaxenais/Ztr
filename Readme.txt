---------------------------------------------------------------
Steps to host the backend API
---------------------------------------------------------------

Run docker build with the dockerfile provided (path: /DeviceConfigAPI/dockerfile) and following the steps below:
Note: Steps 1-6 are not required for first time.

1. Enter command - docker container ls -a
2. Enter command - docker stop "m7-prod"
4. Enter command - docker rm "m7-prod"
5. Enter command - docker images
You can see the images that are currently present:
root@SEZZRT-VM1:# docker images
REPOSITORY                             TAG               IMAGE ID       CREATED             SIZE
api                                    prod               2bd6320d3a3d   About an hour ago   540MB
<none>                                 <none>            e818098a076a   About an hour ago   1.41GB
6. Remove image using command - docker image rm -f "api:prod"
7. Build image using command  - docker build -t api:prod -f DeviceConfigApi/Dockerfile . (there is a . at the end)
8. The following command will run app in a container that you can access in your web browser at http://<<VM IP>:5000.
Run image using command - docker run -d -it -p 5000:80/tcp --name m7-prod api:prod

You should see the following console output as the application starts:
Hosting environment: Production
Content root path: /app
Now listening on: http://[::]:80
Application started. Press Ctrl+C to shut down.

Make sure the port is accessible from the browser outside the VM.

9. Test the API by going to the following url: 
http://<<VM IP>>:<<port>>
e.g: http://10.0.0.1000:5000

If you can see the swagger page, then API is up & running.

Note: The -p argument in step 8 maps port 5000 on your local machine to port 80 in the container (the form of the port mapping is host:container). See the Docker run reference for more information on command-line parameters. In some cases, you might see an error because the host port you select is already in use. Choose a different port in that case.

-- End of file ---