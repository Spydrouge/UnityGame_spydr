### Cubiqity-Substrate  
* Purpose: Currently attempting to port our data from Cubiqity to Substrate  
* Author: Melissa Kronenberger (Spydrouge)  


### Some Journaled Observations  

I am noticing some things about cubiquity and it's editor that I would like to jot down before I do any further work, so that if they can or should be addressed later that I do not forget about them...  
  
* Holding 'Alt' and left clicking to look around is very natural for me, but doing this does not disable the editing tools for cubiquity, causing thousands of blocks to appear as I look around the map.   
* Cubiquity determines if it can draw/delete a new cube every time there is a fresh mouse message (such as move mouse or left down). Is this necessarily the best editing behavior? For me, minecraft objects are often built in 'layers' (you place a wall, then another wall, then a ceiling, or you place a ring and build the ring up). As such, it may be valuable to restrict the POW of the brushes as one is editing, based on contextual information. For example, it may be valuable to say that the only cubes which can be added until the mouse is released are cubes which build off of cubes that existed previous to the mouse button being depressed. This would allow builders to lay down a layer of cubes without causing a rapid spiral of cubes to explode into existance springing out at the editor's viewport. It would be like painting a fresh layer of cubes onto the existing architecture. Likewise, deletion could be restricted to cubes which share either an xy, yz, or xz lane (depending on viewport) with the initially deleted cube when the left mouse button was first depressed. This allows an editor to skim off a layer of blocks. From my perspective, this gives the editor the power of a pixel artist, and the control. I don't know many people who actually have any need for long irregular spurts of cubes bounding towards the viewport.   
* Implementing brushes seems worthwhile, though again I feel it needs some intentionally designed restrictions to prevent wrist quaverings from blowing holes through the terrain, especially as...  
* There is no undo.   
* I feel like editing the inspector/editor component is definitely the key part of all of this- once I get data to import/export correctly.   
* I feel like it is important to design exactly which functionalities need to be added to the editor, at what priorities, with what constraints for usability.  
* I feel like blocks which have been displaced by the physics engine should 'roll' into new voxel positions to rejoin the march of cubes ;) Perhaps later.  

### Remembering how to clean files  

I realized Cubiqity came with a bunch of documentation (PDF) files embedded in with it, as well as some examples that were more than a few megs in size. BLASPHEMY. I went about re-remembering how to clean Git repositories. Here's what I came back with:  

(the -f in the filterbranch is to replace any existing rewrite info, and -r is for recursive)  

git filter-branch -f --tree-filter 'git rm -rf Assets/Cubiqity/Documentation' HEAD  
git reflog expire --expire=now --all  
git gc --prune=now  
git push origin --force -all  

### Source tracking  
I did my best to get some up-to-date information on how to do source code tracking in Unity.     
The solution was to change some information under Unity/Preferences (specifically, set Metadata to visible and Force Text in the editor).     
These two options will supposedly allow me to cart my project around from computer to computer with just the Project Settings and Assets folders (everything else is in .gitignore). The idea is that when new people pull my project, they should be able to build it in its entirity from just these two folders. The options I set should make sure that important settings (such as textures/materials/script instances) get sent along with the source code (through metadata) and aren't solely embedded in the Library folder in an uncontrolled binary somewhere...  


