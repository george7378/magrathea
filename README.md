# magrathea
Magrathea is an experiment in creating and rendering real-scale procedural planets with 3D terrain. It's a scaled-up adaptation of my [quadtree terrain](https://github.com/george7378/quadtree-terrain) project, with multiple flat adaptive meshes projected onto a sphere. By default, Magrathea renders a planet whose size matches Earth's moon.

When you run the program, it may take a while for the terrain to be built from scratch. When the camera moves, the level of detail changes to match your viewing position. This is constantly happening on a background thread. You can explore the world using the mouse and keyboard. You must press **C** to attach the mouse to the camera, after which mouse movements will control yaw and pitch. Roll is controlled with the **Q** and **E** keys. You can use the **WASD** keys to fly around. Pressing **Up** and **Down** will change the camera's movement speed in powers of 10. There's also a simple atmosphere effect which can be toggled with the **Space** key.

The project isn't perfect yet, but I think it's got a lot of potential, both as a base for games/simulations with full size solar systems and as an educational tool for anyone wanting a clean and minimalistic example of planet rendering. I believe it could be used to create very immersive virtual worlds when coupled with more complex visuals.

Don't panic, and remember to bring your towel!

![Low level](https://github.com/george7378/magrathea/blob/master/_img/1.png)
![On orbit](https://github.com/george7378/magrathea/blob/master/_img/2.png)
![High altitude](https://github.com/george7378/magrathea/blob/master/_img/3.png)
![Simple atmosphere](https://github.com/george7378/magrathea/blob/master/_img/4.png)