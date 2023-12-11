#!/bin/bash

# Define the image name
IMAGE_NAME=GolfClapBot

# Build the Docker image
docker build -t $IMAGE_NAME .

# Run the Docker container
docker run -d -p 8080:80 --name GolfClapBot $IMAGE_NAME