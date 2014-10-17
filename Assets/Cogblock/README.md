# CogBlock

* Purpose: Top-Level Folder for CogBlock scripts (related to a CogBlock Volume that derives from Cubiquity's ColoredCubes Volume)

Notes:
* It is expected that CogBlock scripts will be subsumed by Lake's extension of ColoredCubes Volume)

Structure:  

* Editor: Scripts specific to menu items, editor windows, and inspectors; code that runs at edit time and enhances the editor. May serve as editors for scripts in the Play folder, and interact with tools in the Utility folder.   
* Play: Scripts derived from MonoBehaviors and GameObjects which are dragged onto objects in Edit Mode and do their dirty work at Play time. May be edited/inspected/enhanced by scripts in the Edit Folder and operate using the tools in the Utility folder.  
* Utility: Scripts that are not attached to objects and do not modify editor windows, and which are often used as tools, enumerated types, etc. Often used by both Play and Editor scripts to make tasks easier.   
* Plugins: Empty at present; a space for .dlls


