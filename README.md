# TuxPaperEngine
Frontend for https://github.com/Almamu/linux-wallpaperengine. Tries to replicate the feel of wallpaper engine on windows.
Uses a fork of the (linux-wallpaperengine) https://github.com/NeXx42/linux-wallpaperengine to replicate some of the wallpaper engine properties like offsets, saturation, and contrast

The main intention was to create an application in which i can use wallpaper engine workshop items with my dual monitor setup.


## Screenshots
![ezgif-2d38ef5be79c38e9](https://github.com/user-attachments/assets/98d16764-7562-48bf-ac5c-9b08fe79ce66)
<img width="3841" height="1081" alt="2025-12-23-205639_hyprshot" src="https://github.com/user-attachments/assets/409dcdd3-a351-4a69-a00a-7c2444cac1c8" />

**Installed Wallpapers**

<img width="500" alt="image" src="https://github.com/user-attachments/assets/488d3ece-39c3-490b-9da2-692406df5311" />
<img width="500" alt="image" src="https://github.com/user-attachments/assets/5ee1c429-8ef8-4713-b311-743f86874c10" />

**Steam workshop**
>[!WARNING]
>The steam integration can be finicky, If you are able to get to the login on the first download you may need to approve steamcmd access through the steam authenicator.
<img width="500" alt="image" src="https://github.com/user-attachments/assets/ea8633d6-1139-4aec-b42b-f2beaa1d6458" />
<img width="500" alt="image" src="https://github.com/user-attachments/assets/e62c1d8d-bb3c-4f4f-ab65-d1dfeef7f34a" />
<img width="1919" height="1080" alt="image" src="https://github.com/user-attachments/assets/5bbaf289-4edf-4ef5-9ee3-d05e156a6688" />

**Settings**
>This will only pull down the github project and build it. You will still need to download the dependencies for your respective distro.
<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/392b4a08-1b6b-44e7-914e-9edf979f275c" />


## Requires

* Install requirements following: https://github.com/Almamu/linux-wallpaperengine
* .Net

## Installation

1. Clone
```bash
git clone https://github.com/NeXx42/linux-wallpaperengine-gui.git --recursive
```
2. Build
```bash
make build
```
3. Run
```sh
./Build/Output/TuxPaperEngine/TuxPaperEngine
```

Or build the appimage with 
2. Build
```bash
make publish
```
3. Run
```sh
./Build/Output/TuxPaperEngine.appimage
```
