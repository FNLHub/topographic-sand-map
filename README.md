# Topographic Sand Map

## Install Linux

### Recommended version: 
```Linux Mint 19.3 Mate 64bit```

#### update the date and time 

### Recommended Hardware: 
```Nvidia GPU```
```
glxinfo | grep vendor
```

#### Should output:
```
server glx vendor string: NVIDIA Corporation
client glx vendor string: NVIDIA Corporation
OpenGL vendor string: NVIDIA Corporation
```

### Vrui VR Development Toolkit
```sh
cd ~
wget http://web.cs.ucdavis.edu/~okreylos/ResDev/Vrui/Build-Ubuntu.sh
bash Build-Ubuntu.sh
```

#### Cleanup Vrui installation scripts
```
rm ~/Build-Ubuntu.sh
```

#### Adjusting the Screen Size in Vrui
```
sudo xed /usr/local/etc/Vrui-8.0/Vrui.cfg
```

```Set autoScreenSize to false (line ~153)```
```
autoScreenSize false
```

#### Install the Kinect 3D Video Package

```
cd ~/src
wget http://web.cs.ucdavis.edu/~okreylos/ResDev/Kinect/Kinect-3.10.tar.gz
tar xfz Kinect-3.10.tar.gz
cd Kinect-3.10
make
sudo make install
sudo make installudevrules
ls /usr/local/bin
```

####  Install the Augmented Reality Sandbox 
CalibrateProjector, SARndbox, and SARndboxClient.

```
cd ~/src
wget http://web.cs.ucdavis.edu/~okreylos/ResDev/SARndbox/SARndbox-2.8.tar.gz
tar xfz SARndbox-2.8.tar.gz
cd SARndbox-2.8
make
ls ./bin
```

NOTE: AR Sandbox calibration utility and main application are now in ```~/src/SARndbox-2.8/bin```.

#### System Integration, Configuration, and Calibration

##### Connect and Configure the 3D Camera

```
sudo /usr/local/bin/KinectUtil getCalib 0
```

##### Calculate Per-pixel Depth Correction (Optional)
```First-generation Kinect cameras (Kinect-for-Xbox-360) have a certain amount of non-linear depth distortion```

```
sudo /usr/local/bin/RawKinectViewer -compress 0
```

##### Align the 3D Camera

```
cd ~/src/SARndbox-2.8
RawKinectViewer -compress 0
```

##### Measure Sandbox's Base Plane Equation

```
cd ~/src/SARndbox-2.8
xed etc/SARndbox-2.8/BoxLayout.txt &
```

```
Camera-space plane equation: x * <some vector> = <some offset>
```

```BoxLayout.txt``` - Plane equation
<some vector>, <some offset>

##### Measure Sandbox's 3D Box Corner Positions

```BoxLayout.txt``` - should look like the following, with different numbers depending on your installation

```
(-0.0076185, 0.0271708, 0.999602), -98.0000
(  -48.6846899089,   -36.4482382583,   -94.8705084084)
(   48.3653058763,   -34.3990483954,   -89.3884158982)
(   -50.674914634,    35.8072086558,   -97.4082571497)
(   48.7936140481,    36.4780970044,     -91.74159795)
```

##### Align the Projector

```
XBackground
```

##### Projector/Camera Calibration

```
cd ~/src/SARndbox-2.8
./bin/CalibrateProjector -s <width> <height>
```

```
./bin/CalibrateProjector -s 1024 768
```

NOTE: ```F11`` - Must run in Full-screen mode.

#### Run the AR Sandbox

```
cd ~/src/SARndbox-2.8
./bin/SARndbox -uhm -fpv
```

#### Create Per-application Configuration File Directory

```
mkdir -p ~/.config/Vrui-8.0/Applications
```

#### Create Configuration File for CalibrateProjector

```
xed ~/.config/Vrui-8.0/Applications/CalibrateProjector.cfg
```

```CalibrateProjector.cfg```

```
section Vrui
    section Desktop
        section Window
            # Force the application's window to full-screen mode:
            windowFullscreen true
        endsection
        
        section Tools
            section DefaultTools
                # Bind a tie point capture tool to the "1" and "2" keys:
                section CalibrationTool
                    toolClass CaptureTool
                    bindings ((Mouse, 1, 2))
                endsection
            endsection
        endsection
    endsection
endsection
```

####  Create Configuration File for SARndbox

```
xed ~/.config/Vrui-8.0/Applications/SARndbox.cfg
```

```SARndbox.cfg```

```
section Vrui
    section Desktop
        # Disable the screen saver:
        inhibitScreenSaver true
        
        section MouseAdapter
            # Hide the mouse cursor after 5 seconds of inactivity:
            mouseIdleTimeout 5.0
        endsection
        
        section Window
            # Force the application's window to full-screen mode:
            windowFullscreen true
        endsection
        
        section Tools
            section DefaultTools
                # Bind a global rain/dry tool to the "1" and "2" keys:
                section WaterTool
                    toolClass GlobalWaterTool
                    bindings ((Mouse, 1, 2))
                endsection
            endsection
        endsection
    endsection
endsection
```

#### Create a Desktop Icon to Launch the AR Sandbox

```
xed ~/src/SARndbox-2.8/RunSARndbox.sh
```

```RunSARndbox.sh```

```
#!/bin/bash

# Enter SARndbox directory:
cd ~/src/SARndbox-2.8

# Run SARndbox with proper command line arguments:
./bin/SARndbox -uhm -fpv
```

```
chmod a+x ~/src/SARndbox-2.8/RunSARndbox.sh
```

```
xed ~/Desktop/RunSARndbox.desktop
```

```RunSARndbox.desktop```

```
#!/usr/bin/env xdg-open
[Desktop Entry]
Version=1.0
Type=Application
Terminal=false
Icon=mate-panel-launcher
Icon[en_US]=
Name[en_US]=
Exec=/home/<username>/src/SARndbox-2.8/RunSARndbox.sh
Comment[en_US]=
Name=Start the AR Sandbox
Comment=
```

```
chmod a+x ~/Desktop/RunSARndbox.desktop
```

#### Launch the AR Sandbox on Login / Boot

#### Use Multiple Screens

```
xrandr | grep connected
```

```
xed ~/.config/Vrui-8.0/Applications/CalibrateProjector.cfg
```

```
xed ~/.config/Vrui-8.0/Applications/SARndbox.cfg
```

```CalibrateProjector.cfg & SARndbox.cfg``` - Following outputName into the existing "Window" section.

```
        section Window
            ...
            # Open the window on a specific video output port:
            outputName <port name>
            ...
        endsection
```

Projector to to right of screen
```
xrandr --output DVI-I-1 --auto --primary --output HDMI-0 --auto --right-of DVI-I-1
```

Turn projector off
```
xrandr --output DVI-I-1 --auto --primary --output HDMI-0 --off
```

#### Show a Secondary View of the AR Sandbox

```
xed ~/.config/Vrui-8.0/Applications/SARndbox.cfg
```

[Source](https://web.cs.ucdavis.edu/~okreylos/ResDev/SARndbox/LinkSoftwareInstallation.html)
