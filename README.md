# Kic

This is a game I've been working on for the past couple years.
It is a 2D action platformer with RPG elements (or a Metroidvania, if you will).
You play as Kic, a pair of robotic legs with a mind of its own.
Kic runs, jumps, and kicks its way through an underground lab filled with hostile robots.
Ideally I would like to release this on Steam in the future. (and as a trilogy...)

-=# Enemy, Pod, Cube, Byte #=-

A few of the enemy classes I have completed thus far.
I used virtual overloading (polymorphism) for enemies since different enemies have different AI and display properties.
The Enemy class is the base class while Pod, Cube, and Byte are all the derived classes.

-=# XML, Loader, Writer #=-

These classes handle XML file reading/writing.
The XML class holds numerous functions for reading files such as reading and saving single or multiple nodes.

-=# HitDetection #=-

This class handles any hit detection needed between 2 sprites.
It essentially overlaps the 2 sprites (determined by their current position in-game) and then goes through all the overlapping areas and looks for any spots where 2 non-transparent pixels are touching.

-=# KIC_Object #=-

I was hesitant to post this file, since it's still a  work in progress.
This is where everything associated with Kic is handled.
Actions, animation, graphical displaying, hit detection, controller inputs, even stuff like the flowing scarf Kic wears that flows and is effected by gravity.
Just don't mind the mess, lots of commented-out sections for future reference and whatnot.

