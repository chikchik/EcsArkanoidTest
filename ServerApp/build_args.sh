#!/bin/bash
TOKEN="paste your token here"
PATHTOSOLUTION="ServerApp/"
NAME="image-name"
VERSION="1.0.40"
dotnet run --project ./tools/ContainerBuilder/ -- process -n $NAME -p $PATHTOSOLUTION -v $VERSION -t $TOKEN
read 
