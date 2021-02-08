To Host the backend API.

Run docker build with the docketfile provided.
Run docker run command with the image and desired port.
e.g : > docker run -d -it -p 5000:80/tcp --name m7-api api:latest
here -p 5000 is the port.

Make sure the port is accessible from the browser outside the VM.

Test the API
http://<<VM IP>>:<<port>>
e.g: http://10.0.0.1000:5000

If you can see the swagger page, api is up & running.