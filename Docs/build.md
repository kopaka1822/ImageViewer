# Build instructions

Requirements:
- Windows x64
- Visual Studio 2019 (with C++, C# and NuGet Package Manager)
- .NET Framework 4.6.1
- Git LFS: https://git-lfs.github.com/ (Some submodules need git-lfs to work properly when cloning)

Instructions:

1. Do a recursive pull (this project uses submodules)
2. Open the TextureViewer.sln with Visual Studio
3. In the Solution Explorer, select ImageViewer as startup project
4. Switch architecture from "Any Cpu" to "x64" (and Debug to Release for release builds).
5. Build the solution (This should also download the required packages via NuGet)

Debug:
In order to debug the .dll files (namely DxImageLoader.dll) go to the Solution Explorer. Right Click on the ImageViewer Projects and go to Properties->Debug. Select "Enable native code debugging"
