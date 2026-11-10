# Unity setup (Windows)

The Unity project is not created yet. These are the steps to create it so it uses the simulation library.

1. Install Unity Hub and **Unity 6 LTS** with Windows Build Support.
2. In Unity Hub: New project, template **High Definition 3D (HDRP)**, location `river-command/Unity`.
3. Package Manager: add `com.unity.nuget.newtonsoft-json` and `com.unity.inputsystem`.
4. Build the sim DLL on Windows:
   ```bash
   dotnet build Sim/RiverCommand.Sim -c Release
   ```
   Copy `Sim/RiverCommand.Sim/bin/Release/netstandard2.1/RiverCommand.Sim.dll` to `Unity/Assets/Plugins/Sim/`.
   (A build step will automate this later.)
5. Copy or symlink `Content/` to `Unity/Assets/StreamingAssets/Content/` so the game loads the same JSON.
6. Commit the generated `Unity/` folder except what `.gitignore` excludes.

After step 6, tell Claude, and the unity-presentation agent will add the bridge, camera, and HUD scripts.
