# Topographic Sand Map

This is a work in progress project by a team at Friday Night Lab in College of the Sequoias to make a working topographic sand map table. A topographic sand table has both a projector and a depth sensor (which is the Xbox One Kinect in our case) hanging over a surface covered in sand. The depth sensor detects the height of the sand, which is then processed into a topographic contour map and projected onto the sand. The current software we are using is AR Sandbox, but sadly this only works in Linux and can only run six frames per second.

[AR Sandbox installation guide](ARSandbox.md)

Our goal is to write a different program that directly uses depth data from the Kinect (which would be at 30 frames per second) using Unity and shaders.


