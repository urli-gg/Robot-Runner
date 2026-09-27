========================================================================

INDUSTRIAL ROBOT FACTORY

Version: 1.0 

Author: CookieEfedu

Platform Compatibility: GameMaker, Godot Engine, Unity, Tiled, LDTK, Construct

========================================================================


Thank you for downloading/purchasing the Industrial Robot Factory Asset Pack! 
This Package contains Pixel Art Assets Optimized for 16-Bit Retro Action-Platformers.


------------------------------------------------------------------------

1. FILE STRUCTURE & ATLAS/SHEETS PROPERTIES

------------------------------------------------------------------------


Root Folder/

 ├── 📁 tileset/                            
 │    ├── 📁 gray/                    --> Original Gray Scale Assets (Ready for custom recoloring)
 │    │   ├── 📁 sheets/                      -->PNG Atlas/Sheets in Gray Scale and JSON/XML Metadata Files
 │    │   └── 📁 individual_sprites/          --> Separated PNG Frames sorted by Category(folders)
 │    │                       
 │    │
 │    └── 📁 color/                    --> Full Color Versions of Each Biome (4 Biomes)  
 │        ├── 📁 Biome_Day_Factory/    
 │        ├── 📁 Biome_Hot_Factory/    
 │        ├── 📁 Biome_Nigth_Factory/    
 │        └── 📁 Biome_Toxic_Factory/           
 │                ├── 📁 sheets/                       --> PNG Atlas/Sheets and JSON/XML Metadata Files 
 │                └── 📁 individual_sprites/           --> Separated PNG Frames sorted by Category(folders)   
 │                                   
 │
 │
 └── 📁 palletes/                       --> Palletes in PNG and GPL Files for Pallete Swap
     
- Grid Size: 32x32 Pixels (Standard Sheets) and 14x14 pixels (Small Props Sheet Only)
- Margin:2px 
- Spacing: 2px 


------------------------------------------------------------------------

2. TILESET CONTENT PACK LIST & BIOMES

------------------------------------------------------------------------

-The Tileset has splited in different Images for 
Optimize the Animations , Enemies, Active Objects and Terrain Functions:
  -Terrain: Contain the Terrain, Background and Liquid Tiles
  -Props: Contain the Static Props
  -Small Props: Contain the Static Small Props
  -Props Animated: Contain the Animated Props and Liquid Animations
  -Vfx:  Contain the VFx Animations
  -Enemies Animated: Contain the 5 Animated Enemies 
 
- For Automatic Extraction of the Sheets/Atlas Sprites, use the Metadata Files included:
    -Format: XML and JSON  files ( FreeTexture Packer format)
    -Filenames: Each Metadata File has the same Name as The PNG Sheet File
 
-For Access to each Individual Sprite of Tileset, use the Folder Individual_Sprites , it has each 
Sprite of Tileset as Individual Image sorted by Categories ( Folders)

-The Asset has Available a gray Scale Version for Pallete Swap and Full Color Versions for 4 Biomes : 

   A. Day Biome (Base Biome)  
   B. Toxic Biome
   C. Hot Biome 
   D. Night Biome
      

------------------------------------------------------------------------

3. PALLETE & COLOR INDEXING
------------------------------------------------------------------------

-This Pack Include full support for Palette-Swapping 
-The Folder Palletes Contain:
  1-Individual Pallettes (PNG /GPL Files):
      -Format: Separate in 1-row color bars for general Engine Compatibility
      -Include: Gray Scale pallete and 4 biomes Pallete ( Day Biome, Hot Biome,
                Nigth Biome and Toxic Biome)
  2-Unified Palette Map (pallete_map_strip.png):
     A Single Texture Map for Indexed Shaders (commonly used in Gamemaker)
     -Row 0 (Y=0): GrayScale
     -Row 1 (Y=1): Day Biome (Base Biome)
     -Row 2 (Y=2): Hot Biome
     -Row 3 (Y=3): Nigth Biome
     -Row 4 (Y=4): Toxic Biome


------------------------------------------------------------------------

4. TERRAIN CONTENT
------------------------------------------------------------------------

-The Terrain Include :
    -Tiles for 2  Terrain Textures:
       -Tile Neon Plate: Dark Metal Plates with Border of Ligth Metal Plate
       -Tile Iron Grid: Iron Grid with Border of Girders
    -Tiles for Background Walls (Single Metal Plates and  2x2 Metal Plates )
    -Tiles for Background Doors with Light Led on top ( One Open and another Close)
    -Tiles for Liquid ( 3 layers of Deep and Transition Tiles with Dithering for each Deep)

-Terrain Texture Specifications :
     -Layout type: 47-Tile Autotile Template ( Standard Godot Terrain / GM Layout Format )
     -Dimension: 32x32 px per Tile
     -Extra Slopes: 2 Variants of Downward Slopes included at the Bottom of each Texture
      for Physical-Based Levels Design (Ideal for High-Speed Plataform)

------------------------------------------------------------------------

5. CHARACTER & ENEMY ANIMATIONS 

------------------------------------------------------------------------

-Each Animation has 3 Frames. Some Animations are Normals anothers in Loop:

  1- Drone Type A (Basic Patrolling Enemy) and Drone TypeB (Advanced Patrolling Enemy)
    -Iddle: Loop Animation, Drone Hovering
    -Move:  Loop Animation, Fly Movement
    -Death: Normal Animation,  Destroy of Drone
    -Damage: Normal Animation, the Drone is Damaged 
    -Attack: Normal Animation, the Drone Throw a Shot with the Canon at the Bottom
    -Intro:  Normal Animation, the Drone Destroy the Bars are Grabbin Him
                     
 2- Bot Patrol (Advanced Mecha Bot)
    -Iddle: Loop Animation, for Wait State
    -Run:   Loop Animation, for Walk or Run Movement
    -Death: Normal Animation, Destroy of Bot
    -Damage: Normal Animation, the Bot is Damaged 
    -Attack: Normal Animation, the Bot Throw a Shot with the Canon at the the Top
    -Intro:  Normal Animation, the Bot Destroy the Bars are Grabbin Him

 3- Turret TypeA (Basic Turret for floors) and Turret TypeB (Advanced Turret for floors)
    -Iddle: Loop Animation, for Wait State
    -Death: Normal Animation, Destroy of Turret
    -Damage: Normal Animation, the Turret is Damaged 
    -Attack: Normal Animation, the Turret throw a Heavy Shot
 
------------------------------------------------------------------------

6.  STATIC PROPS

------------------------------------------------------------------------

  -The Props are divided in Props of Normal Size and Props of Small Size
  -The Props Content include:
      1-Boxes(4 types):Robust and Flat( 2 Variants ), Brekeables  (with X Simbol) and Explosives (Alert Simbol).
      2-Machinery : Heavy  , Small, Balves, With Imans,..
      3-Plataforms (2 types):Each Plataform has Two States (Active and Inactive)
      4-Leds: for Wall and Roofs
      5-Modular Props: Crusher Machine, Chains, Tubes, Girders, ZipLines , Wire and Lamp
      6-Metal and Bots Pieces 
      7-Switches ( 2 Variant): Button style and toggle style                
      8- Leds for On/Off States

------------------------------------------------------------------------

7.  PROPS ANIMATED

------------------------------------------------------------------------

-Each Animation of Animated Prop has 3 Frames (same of Animations of Enemies)
-Each Animation can be Normal, Loop or PingPong Type .

Animated Machinery:
    - Conveyor_Loop : Loop Animation
    - Conveyor_ExtremLeft_Loop: Loop Animation
    - Conveyor_ExtremRight_Loop: Loop Animation
    - Electric_Saw_loop: Loop Animation
    - Electric_Saw_Activation: PingPong Animation
    - RoboticArm_Grab_PingPong: PingPong Animation
    - Door_Open_PingPong : PingPong Animation
    - Door_Unlock_PingPong : PingPong Animation
    -Toggle_Activation_PingPong: PingPong Animation
    -Energy_Reactor_Destroy: Loop Animation (the frame 1 for Stable state, frame 2 for Damaged and frame 3 for Destroyed)

Animated Fluid:
    - Liquid_Loop: Loop Animation
    - Fluid_Pipe_Flow_Loop : Loop Animation
    - Fluid_WaterFall_Impact_Loop: Loop Animation
    - Liquid_Splash: Normal Animation


------------------------------------------------------------------------

8. VFX

------------------------------------------------------------------------

- Spark (Two directions)  : Normal Animation, one Start at Top(Frames 1-3), 
                       the other Start at Bottom  (Frames 4-6)
- Bullets (3 Variants):Composed for Muzzle( Normal Animation), Loop (Loop and PingPong Animation) ,
                        Destroy ( Normal Animation) and Trail ( For Trail Particles)
- Vfx_Steam :  Can be Used as Loop or Normal Animation           
- Explosions( 2 types) :  Normal Animation , one is Explosive Type the other is of Plasma 

------------------------------------------------------------------------

9. TECHNICAL NOTES FOR PROGRAMMERS (Implementation Tips)

------------------------------------------------------------------------

*PROPS ANIMATIONS:
   - Robotic Arm:
        Frame 1 is the default "Iddle" state. 
        Trigger the Animation to Stretch Down.
        Play the Animation in Revere Way(Ping-Pong) to retract. 
        The center has empty Pixels designed to Attach Small Props (metal pieces or bots pieces) via code.

  - Nuclear Reactor :
      - Frame 1: Full Power / Stable state.
      - Frame 2: Cracking core state/ Damage state.
      - Frame 3: Completely Destroyed / Offline state.
      TIPS:
         -On Frame 3, Spawn the Vfx for Plasma Explosion at the Reactor's Center for maximum Game Feel
         -Spawn the Vfx Spark when the Reactor Receive Damage

   - Conveyor:
      - For Achive The Opposite Direction Switch The Rotation or use the Animation
        in Reverse Way (PingPong)
      - Ensure The Extremes  parts has The Animation Frames Coordinantes with The Center Animation 

   -Door Open:
      -Use the 1 Frame as Close state and the frame 3 as Close state
      -Trigger the Animation in Normal way to Open the Door and in 
       Reverse Way (PingPong) to Close the Door

   -Door UnLock:
      -Use the Frame 1 as Lock State and frame 3 as Unlock State (or the Frame 1 of Door Open Animation)
      -Trigger the Animation in Normal way to Unlock the Door and in 
       Reverse Way (PingPong) to Lock the Door

   -Toogle Animation:
      -Use the 1 Frame as off state and the Frame 3 as On State
      -Trigger the Animation in Normal way to Activate the Toggle and in 
       Reverse Way (PingPong) to Desactivate the Toggle
  
   - Electric Saw:
       
       -For Activate or Desactivate Play the 'Electric Saw Activation' Animation in Normal Way (Activation) or Reverse Way/PingPong (Desactivation)
       -While the Machine is in Activate State Play ' Electric Saw Loop Animation' as loop Animatiom
       -For Show the Half of Electric Saw Ensure the Sprites of Terrain/Walls Draw After the Machine Sprites( Adjust Z index , Position of Z Axis or Layer Index)
       -You Can Combine the Electric Saw with ZipLines for show the Path of Saw

   -Fluid_Pipe_Flow_Loop: 
      Use it Animated prop at the Bottom Output of Tubes

   -Fluid_WaterFall_Impact_Loop
     -Replace the a static Tile of Liquid Surface for it Animation for Show the Impact of a Fluid with the Surface
     -Ensure it Animation is in Coordination with Fluid_Pipe_Flow_Loop Animation Frames

   -Liquid_Loop
     Use it Animation for dinamic Liquid Surface ( replace the Static Tiles for the Animation)
   
   -Liquid_Splash:
     Use it Animation On a Liquid Tile When a Object Impact on the Liquid Surface

* VFX FEEDBACK:
     
     -Spark: Use the Spark Animation when a Enemy is Damaged , Before Init the Dead Animation ( Bots, Turret,  Drones) or When Attack the Energy Reactor,
     
     -Steam: Use Vfx_Steam on Machines with Outputs or Tubes with Horizontal Outputs or When a Heavy Box/Machinery Impact with the Floor
     
     -Explosions:
	-Use it Animation When a Enemy is Dead, Something Hit a Explosive Box or Destroy the Energy Reactor

     -Bullet:
      -Use the Frame 1 as Muzzle Animation
      -For Loop Movement,Play the Frames 2 to 4: at the Init (Frame 2) Play in normal Way ,
       at the end(Frame 4) Play in Reverse Way (PingPong)
      -Use the Frames 5 and 6 for Destruction Animation 
      -Use the Frame 7 for Trail of Bullet	
	    
STATIC PROPS:

   -Crusher Machine:
      -Move in Y axis All parts at same time for Emulate Movement of the Machine
      -To Hide the Parts of Machine When It is Rising , Ensure the Sprites of Terrain Draw
      After the Crusher Machine Sprites( Adjust Z index , Position of Z Axis or Layer Index)
  
   - ZipLines:
          Use It Small Prop For  Guide the Iman Machine, Electric Saws or Robotic Arm Machine
   
    -Girders:
       Use It Prop as Foreground Element:
            Ensure the Sprites of the Girder Draw After the rest of Sprites( Adjust Z index , Position of Z Axis or Layer Index)
  

ENEMIES:
  - Intro Animation (Drons and Patrol Bot):Start it Animation inside of Open Background Doors of Terrain File
  - Damage Animation: Combine it with Spark Vfx 
  - Dead Animation: Before of Play the Animation show the Frame 1 of Damage Animation and the Vfx of Spark Animation.
    At the End of Animation Show the a Vfx of Explosion and hide the Sprite
  -Attack Animation: On the Frame 1 of the Animation,Show the 1 frame of a Bullet Animation (Muzzle Animation) and Instantiate in it position a Bullet Object

------------------------------------------------------------------------

10. QUICK LICENSE SUMMARY

------------------------------------------------------------------------

 -Commercial & Non-Commercial: Allowed in Games, Interactive Projects and Media
 -Modifications: Allowed (Recolor, Edit, Crop)
 -Restrictions: Prohibited Redistribution, Resale, Sharing , AI Training Use, or NFT Minting
 -Attribution/Credit: Not Required but Greatly Appreciated .If you Wish you may Attribute/Credit in the Following Way:
  "2D Art by CookieEfedu(https://cookieefedu.itch.io)"

For the Full Legal Termns and License Text Please Read the Included LICENSE.txt
   

========================================================================

For support or feedback: 
 Contact Email: cookieefedu.gamedev@gmail.com 
 Art Portafolio:https://www.deviantart.com/cookieefedu
 Social/ Updates: https://instagram.com/cookie_efedu (@cookie_efedu)

Have fun building your factory!

===============================================================