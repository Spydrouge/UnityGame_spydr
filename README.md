# Cubiqity-Substrate  
* Purpose: Currently attempting to port our data into Cubiqity from Substrate  
* Author: Melissa Kronenberger (Spydrouge)  

## Journalling  
* Purpose: Figure out where we're at and where we're going

In general it appears that we are gutting the majority of code prefaced with OC that has to do with Voxels in the whole of our game. We are attempting to replace a hand-ironed system with a more professional and well-supported package. Our initial testing has suggested this will play nicer with Unity. 

### Some Journaled Observations about Cubiquity  

** Useful **

* [Documentation](http://www.cubiquity.net/cubiquity-for-unity3d/1.1/docs/class_cubiquity_1_1_region.html)

** Unexpected Notes **

* Cubiquity is not simply the Unity3D port of PolyVox (the original code that the author was working on). There is a Cubiquity  native code library itself which wraps PolyVox, which is not open source or editable (though I believe it to be a wrapper for the most part). This _could_ (as of yet we do not know) make it difficult to do some fun things, like forcing Cubiquity to store data with our tags. We will have to plan around any limitations this poses.  
* The Cubiquity voxel engine stores a volume as a *Voxel Database*.  
* Cubiquity can import voxel data from 'better' Voxel editors, which might void the needs listed below under "Observations on playing with Editor." Specifically it lists Magica Voxel and says that it comes with the ability to import from Magica Voxel, Voxelap, and Images Slices automatically using a command prompt!  
* Cubiquity volumes have their Width/Height/Depth set through Create Terrain Volume Data or Create Colored Cubes Volume Data.  Cubiquity says that 512x512x64 is *the limit* for Volume Data for a reasonable desktop PC. 
    * While there is support in PolyVox for infinite terrain, it is NOT yet exposed to Cubiquity 

** Observations on playing with Editor **
I am noticing some things about cubiquity and it's editor that I would like to jot down before I do any further work, so that if they can or should be addressed later that I do not forget about them...  
  
* Holding 'Alt' and left clicking to look around is very natural for me, but doing this does not disable the editing tools for cubiquity, causing thousands of blocks to appear as I look around the map.   
* Cubiquity determines if it can draw/delete a new cube every time there is a fresh mouse message (such as move mouse or left down). Is this necessarily the best editing behavior? For me, minecraft objects are often built in 'layers' (you place a wall, then another wall, then a ceiling, or you place a ring and build the ring up).   
    * As such, it may be valuable to restrict the POW of the brushes as one is editing, based on contextual information. For example, it may be valuable to say that the only cubes which can be added until the mouse is released are cubes which build off of cubes that existed previous to the mouse button being depressed.  
	* This would allow builders to lay down a layer of cubes without causing a rapid spiral of cubes to explode into existance springing out at the editor's viewport. It would be like painting a fresh layer of cubes onto the existing architecture.  
	* Likewise, deletion could be restricted to cubes which share either an xy, yz, or xz lane (depending on viewport) with the initially deleted cube when the left mouse button was first depressed. This allows an editor to skim off a layer of blocks.  
	* From my perspective, this gives the editor the power of a pixel artist, and the control. I don't know many people who actually have any need for long irregular spurts of cubes bounding towards the viewport.   
* Implementing brushes seems worthwhile, though again I feel it needs some intentionally designed restrictions to prevent wrist quaverings from blowing holes through the terrain, especially as...  
* There is no 'undo' functionality. Which, if you just punched an unintentional hole through the terrain, could turn catastrophic...     
* I feel like editing the inspector/editor component is definitely the key part of all of this- once I get data to import/export correctly from Minecraft/Substrate to Cubiquity of course.   
* I feel like it is important to design exactly which functionalities need to be added to the editor, at what priorities, with what constraints for usability.  
* I feel like blocks which have been displaced by the physics engine should 'roll' into new voxel positions to rejoin the march of cubes ;) Perhaps later.  

### Some Journaled Observations about Substrate  

Q: Where did it come from?   
A: Substrate is a .dll which can be found under the References folder in the unity3d-opencog-game project folder when navigating about in MonoDevelop.  
It is clearly designed for Minecraft. I have found the forum post introducing it [here](http://www.minecraftforum.net/forums/mapping-and-modding/minecraft-tools/1261313-sdk-substrate-map-editing-library-for-c-net-1-3-8)  
The github project page is [here](https://github.com/jaquadro/Substrate)  
According to this post: Substrate is a .NET/Mono SDK written in C# for reading, writing, and manipulating data in Minecraft worlds. Substrate isolates the different levels of map data such as blocks, chunks, and regions, and natively supports modifying Alpha and Beta worlds using the same block and chunk interfaces. Substrate also provides interfaces for other data such as Entities, players, and general level data.  

Q: How do *we* use it? (This is key to translating to Cubiquity, as we basically do the same thing for the new library)  
A: The only file in which Substrate is referenced is "OCFileTerrainGenerator.cs"  which extracts an AnvilRegion. The chunks are extracted from the AnvilRegion and transformed into OCChunk objects.   
We grab a three dimensional vector, check out the blocks one by one and use mcToOCBlockDictionary to convert the blocks from Substrate/Minecraft ids to our block ids. .  
For unknown reasons to me at present, we have individual Battery and Hearth objects instead of just having them be blocks, like any of the other blocks.  
By taking a peek at this, we can look at how Substrate (C# minecraft editor api) maps are loaded into our existing renderer.  

Q: My question is then, what is our in-game renderer at present (the part we are actually replacing with Cubiquity)?  
A: Create new Journal topic!

### Some Journaled Observations about our 'Voxel Renderer'   

Q: Where did it come from?  
A: We seem to handle our own personal jury-rigged Voxel Renderer that we've created from scratch. Based on the assets I see available, we are working off of a free opensource set of scripts provided as part of online tutorials and so forth.  
To find out more information, I headed to this [Voxel Resources & Engines for Unity](http://unitycoder.com/blog/2012/10/18/voxel-resources-engines-for-unity/) page.  
That led me to this [Youtube Video](https://www.youtube.com/watch?v=cgWM75QTr2o) which has a link to a download from dropbox.com that includes a CubicWorld .unitypackage file. This has to be our game because it has our exact assets.  

Q: Aside from the TerrainGenerator, what other important aspects does it have? ie: How does it store sets of blocks?  
A: Our Voxel renderer (which I am to understand is derived from something called 'pixelland' that the author did not want us to credit him for as part of the license) uses a datatype called 'BlockSet' which has its own mini-editor and so forth in game. 
THIS may be something **we want to port to Cubiquity.**




## Journals on Maintenance & Source Tracking  

### Remembering how to clean repositories with GIT  

I realized Cubiqity came with a bunch of documentation (PDF) files embedded in with it, as well as some examples that were more than a few megs in size. BLASPHEMY. I went about re-remembering how to clean Git repositories. Here's what I came back with:  

(the -f in the filterbranch is to replace any existing rewrite info, and -r is for recursive)  

git filter-branch -f --tree-filter 'git rm -rf Assets/Cubiqity/Documentation' HEAD  
git reflog expire --expire=now --all  
git gc --prune=now  
git push origin --force -all  

### Source tracking in Unity3D notes  
 
I did my best to get some up-to-date information on how to do source code tracking in Unity.     
The solution was to change some information under Unity/Preferences (specifically, set Metadata to visible and Force Text in the editor).     
These two options will supposedly allow me to cart my project around from computer to computer with just the Project Settings and Assets folders (everything else is in .gitignore). The idea is that when new people pull my project, they should be able to build it in its entirity from just these two folders. The options I set should make sure that important settings (such as textures/materials/script instances) get sent along with the source code (through metadata) and aren't solely embedded in the Library folder in an uncontrolled binary somewhere...  




