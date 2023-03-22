# Unity Web Server
Unity Web Server is a simple web framework that runs on Unity applications.  
The supported protocol is only HTTP.

## Install
Open `Packages\manifest.json` and add this line to the "dependencies" section.

```
"jp.co.hololab.unitywebserver": "https://github.com/HoloLabInc/UnityWebServer.git?path=packages/jp.co.hololab.unitywebserver",
```

If you want to parse multipart/form-data, please also add the following line.

```
"jp.co.hololab.unitywebserver.multipart": "https://github.com/HoloLabInc/UnityWebServer.git?path=packages/jp.co.hololab.unitywebserver.multipart",
```

## Play sample scenes
Open the Package Manager window.  
Select "Unity Web Server" and press the "Import" button.

<img width="480" alt="Import sample scenes" src="https://user-images.githubusercontent.com/4415085/226830271-eb238b01-814c-43a5-a0f4-8694078489ed.png">

Play the sample scene and access `http://<ip address>:8080` using your web browser.

## License
MIT