# VT2AssetLib
 
Uploading this here because I've put it off long enough since I was funny enough to think I was gonna clean it up eventually!

This library can read and convert Vermintide 2 `.unit` files into an `.fbx` format. It has leftover code for manually converting these files into `.gltf` as well.

The produced `.fbx` files are not guaranteed to be correct or work for all `.unit` files; there's definitely a few that it currently breaks on.

The main meat of the code for `.unit` conversion is in `Stingray\Resources\UnitAssimpSceneBuilder.cs` and the various serializers in `Serialization`.

Makes use of Assimp.NET, SharpGLTF, and SharpZipLib.

## Credits

**| stot |** for VTExtract. (https://gitlab.com/IstotI/vtextract/)  
**Lucas** for VT2 Bundle Unpacker. (https://gitlab.com/lschwiderski/vt2_bundle_unpacker)  
**philipdestroyer** for the files they managed to get and pass along, which was a massive help in figuring out the formats.
