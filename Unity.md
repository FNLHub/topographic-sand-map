# Steps for Installing Unity Topographic Sand Map

## Prerequisites
1. Install Unity
2. Install Kinect V2 API from Microsoft: [https://www.microsoft.com/en-us/download/details.aspx?id=44561](https://www.microsoft.com/en-us/download/details.aspx?id=44561)
3. Install the Unity packages from [http://developer.microsoft.com/windows/kinect/tools](http://developer.microsoft.com/windows/kinect/tools) (you do not need to pay).

## Installation
4. Download this project foler as a .zip file.
5. Create a new Unity project for 2d.
6. Import the Kinect API from the toolbar using `Assets → Import Package → Custom Package...`. Then select the file `Kinect.2.0….unitypackage` to import.
7. Move all files and folders from this project directory into that projects assets directory.
8. Open the scene
9. Plug in Kinect sensor to USB port and set the projector to be a duplicate of the main screen.
10. Press `Ctrl+B` to build.

## Alignment
Once you have the program setup with a projector and a Kinect sensor, use these keybindings to align the camera and projector. The program uses coordinates in 3d to map out the base of the table and then each corner has it's own max height. In effect, this makes a hexahedron where the vertical lines are parallel to the depth (because the upper and lower bounds of each corner must be in the same 2d alignment).

`Backtick` - Select all corners

`1` - Select corner 1

`2` - Select corner 2

`3` - Select corner 3

`4` - Select corner 4

---

`shift` - Increase movement speed

`wasd` - Move selected corners

`r` - Move selected corner base up

`f` - Move selected corner base down

`t` - Move selected corner max height up

`g` - Move selected corner max height down

`right shift + S` - Save alignment

`right shift + L` - Load previous alignment

`h` - Show keybindings help

---

## Visual manipulation
There are also several keybindings to change how the projection looks.

`c` - Increase contour line opacity by 10% (wraps back to 0% at 100%)

`b` - Increase layer blur by 10% (wraps back to 0% at 100%)

`n` - Switch layer design (currently there are rainbow and island)

`x` - Switch debugging displays

`h` - Show keybindings help
