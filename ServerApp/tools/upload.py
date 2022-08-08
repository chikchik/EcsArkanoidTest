import requests
import io
SECRET = "iun432qezZqw"
#HOST = "http://localhost:5000"
HOST = "https://c1.ecs.fbpub.net"
args = "ROOM=5000"
with open("container.zip", 'rb') as file:
    url = f"{HOST}/{SECRET}/upload?{args}"
    req = requests.post(url, files={"file":file})
    print(req.text)