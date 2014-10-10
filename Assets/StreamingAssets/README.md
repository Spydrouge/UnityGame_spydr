# Streaming Assets

* Purpose: Folder for assets that need to be accessible by pathname at runtime.  

Unity Documentation Reads:  
"Most assets in Unity are combined into the project when it is built. However, it is sometimes useful to place files into the normal filesystem on the target machine to make them accessible via a pathname. An example of this is on mobile devices (iOS). You can retrieve the folder using the Application.streamingAssetsPath property."  

Notes:    
* Cubiquity has a strange/interesting habit of trying to save all .vdb, even those created by users, with randomly generated names in it's own nested Cubiquity/VoxelDatabases folder. There is clearly support in Cubiquity for letting us pick where to save our .vdbs, but Cubiquity doesn't expose it right off the bat in the Menu options available to it. Odd, but we'll work around it.  
* CubSub as of 10/10/14 will default to saving its .vdb data at edit time into CubSub/VoxelDatabasesm, with non-random, user-picked names.  
