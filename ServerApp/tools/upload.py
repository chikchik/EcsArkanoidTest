import requests
import zipfile
import os

SECRET = "iun432qezZqw"
CONTAINER_FILES = "../ServerApp/bin/Debug/netcoreapp3.1"
HOST = "http://localhost:5000"
#HOST = "https://c1.ecs.fbpub.net"



zip = zipfile.ZipFile("container.zip", 'w', zipfile.ZIP_DEFLATED)

def add_dir(path, zf):
    for file in os.listdir(path):
        src = f"{path}/{file}"
        print(f"added to zip {src}")
        zf.write(src, file)

with zipfile.ZipFile("container.zip", 'w', zipfile.ZIP_DEFLATED) as zf:
    add_dir(CONTAINER_FILES, zf)


args = "ROOM=5000"
with open("container.zip", 'rb') as file:
    url = f"{HOST}/{SECRET}/upload?{args}"
    req = requests.post(url, files={"file":file})
    print(req.text)