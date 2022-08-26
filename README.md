# VT2AssetLib
 
Uploading this here because I've put it off long enough; I was funny to think I was gonna clean it up eventually! That means it's currently a mess.

This library can read and convert Vermintide 2 `.unit` files into an `.fbx` format. It has leftover code for manually converting these files into `.gltf` as well.

The produced `.fbx` files are not guaranteed to be correct or work for all `.unit` files; there's definitely a few that it currently breaks on.

It also has some code for reading VT2 bundle files.

The main meat of the code for `.unit` reading and conversion is in `Stingray\Resources\UnitAssimpSceneBuilder.cs` and the various serializers in `Stingray\Serialization\` and `Stingray\Resources\Serialization\`.

Makes use of Assimp.NET, SharpGLTF, and SharpZipLib.

## Credits

**| stot |** for VTExtract. (https://gitlab.com/IstotI/vtextract/)  
**Lucas** for VT2 Bundle Unpacker. (https://gitlab.com/lschwiderski/vt2_bundle_unpacker)  
**philipdestroyer** for the files they managed to get and pass along, which was a massive help in figuring out the formats.
