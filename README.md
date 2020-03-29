# magrathea
Magrathea is an experiment in creating and rendering real-scale procedural planets with 3D terrain. It's a scaled-up adaptation of my [quadtree terrain](https://github.com/george7378/quadtree-terrain) project, with multiple flat adaptive meshes projected onto a sphere. By default, Magrathea renders a planet whose size matches Earth's moon.

When you run the program, it may take a while for the terrain to be built from scratch. When the camera moves, the level of detail changes to match your viewing position. This is constantly happening on a background thread. You can explore the world using the mouse and keyboard. You must press **C** to attach the mouse to the camera, after which mouse movements will control yaw and pitch. Roll is controlled with the **Q** and **E** keys. You can then use the **WASD** keys to fly around. Pressing **Up** and **Down** will change the camera's movement speed in powers of 10.

The project isn't perfect yet, and there are some areas I'd like to improve. For example, the height algorithm is slow and doesn't scale well to high altitude viewing. I could also put more work into culling unseen terrain data which is occluded by the forward facing part of the planet. I'm happy with Magrathea's potential though, and I believe it could be used to create very immersive virtual worlds when coupled with more impressive visuals.

![Low level](https://github.com/george7378/magrathea/blob/master/_img/1.png)
![On orbit](https://github.com/george7378/magrathea/blob/master/_img/2.png)
