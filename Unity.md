# Unity Topographic Sand Map Guide

## Equipment
- Cart or table with rim that can hold sand
- XBox One Kinect sensor
- Projector
- Overhead mounting system for sensor and projector (for this implementation, we used a wooden beam attached to the cart)
- Computer preferably with a dedicated GPU

Optionally:
- Various tools (paint scraper, metal spoon...) to sculp the sand with
- Some tarp underneath to prevent a mess


## Prerequisites
1. Install Unity
2. Install Kinect V2 API from Microsoft: [https://www.microsoft.com/en-us/download/details.aspx?id=44561](https://www.microsoft.com/en-us/download/details.aspx?id=44561)
3. Install the Unity packages from [http://developer.microsoft.com/windows/kinect/tools](http://developer.microsoft.com/windows/kinect/tools) (you do not need to pay).

## Installation
4. Create a new Unity project for 2d.
5. Import the Kinect API from the toolbar using `Assets → Import Package → Custom Package...`. Then select the file `Kinect.2.0….unitypackage` to import.
6. Download this project into the `Assets` folder. This file should be at the path `Assets/topographic-sand-map/Unity.md` for the textures to be accessed properly.
7. Open the scene `SampleScene.unity` in the `Unity` directory.
8. Plug in the Kinect sensor to USB port and set the projector to be a duplicate of the main screen.
9. Press `Ctrl+B` to build.

## Alignment
Once you have the program setup with a projector and a Kinect sensor, use these instructions and the keybindings below to align the camera and projector. The program uses coordinates in 3d to map out the base of the table and then each corner has it's own base and max height. In effect, this can project from any hexahedron (solid with 6 quadrilateral sides) where the vertical lines are all parallel.

Steps:
- Open the help menu with `h`. This menu shows all keybindings and info on current alignment and visuals.
- Put the mouse in each corner and drag to align. Alternatively, you can select any corner using `1234`, and use `wasd` to move it. You can also hold `shift` to make it go faster.
- Flatten the surface and then adjust the minimum height using `r` and `f`, you should see no contour lines (look at the monitor screen to see it more clearly if there is one).
- Find a solid object and place it near each corner, then use `t` and `g` to adjust the maximum height until it is about consistent everywhere.
- Congrats! The table should now be roughly aligned. Press `ctrl + S` to save the alignment state. This preset will be loaded whenever the program is run, or when `ctrl + L` is pressed.

## Keybindings

`h` - **Show help menu** (should be used for any adjustments)

` (Backtick) - Select all corners

`1234` - Select corner

`shift` - Increase movement speed

`wasd` - Move selected corners

`drag` - Mouse drag moves corners near the cursor

`r` - Move selected corner base up

`f` - Move selected corner base down

`t` - Move selected corner max height up

`g` - Move selected corner max height down

`ctrl + S` - Save alignment

`ctrl + L` - Load previous alignment

`ctrl + Q` - Exit program

---

Visual options:

`c` - Increase contour line opacity by 10% (wraps back to 0% at 100%)

`b` - Increase layer blur by 10% (wraps back to 0% at 100%)

`x` - Increase texture intensity by 10% (wraps back to 0% at 100%)

`n` - Switch layer design (currently there are rainbow and island)
