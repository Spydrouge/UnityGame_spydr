# CubSub  

* Purpose: Top-Level Folder for CubSub scripts. 

Notes:
* There is also a CubSub Folder under StreamingAssets for materials we might have to access dynamically by path name at run time (and as a place to put materials should we save them at run time)
* At some later time, this folder could be gobbled up into an OpenCog folder. When any relocation occurs, check up that connections aren't lost somewhere in the StreamingAssets > Cubsub > VoxelDatabases area. 

Structure:  

* Editor: Scripts specific to menu items, editor windows, and inspectors; code that runs at edit time and enhances the editor. May serve as editors for scripts in the Play folder, and interact with tools in the Utility folder.   
* Play: Scripts derived from MonoBehaviors and GameObjects which are dragged onto objects in Edit Mode and do their dirty work at Play time. May be edited/inspected/enhanced by scripts in the Edit Folder and operate using the tools in the Utility folder.  
* Utility: Scripts that are not attached to objects and do not modify editor windows, and which are often used as tools, enumerated types, etc. Often used by both Play and Editor scripts to make tasks easier.   
* Scenes: A place for test spaces while we are building the CubSub scripts, which currently is unnecessary to include in final games.    


