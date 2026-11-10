# Handoff: local Claude Code session (Windows PC with Unity)

The cloud session built the simulation (`Sim/`, 46 passing tests) and content (`Content/`).
It cannot reach the PC, so Unity work happens in a Claude Code session running on the PC.

## Start the local session
1. Pull the latest repo:
   ```bash
   git pull origin main
   ```
2. In the repo folder, start Claude Code (Claude desktop app, Code tab, or `claude` in a terminal).
3. Paste the prompt below as the first message.

## First prompt for the local session
```
Read CLAUDE.md, docs/level1-rock-harbor.md, docs/unity-setup.md, and the agent files in .claude/agents/.
Goal: create the Unity 6 HDRP project in Unity/ and get the Rock Harbor vertical slice on screen.

Step 1 (verify environment): find the Unity 6 editor install and the Unity CLI; report versions.
Install the .NET 8 SDK if `dotnet --version` fails. Run `dotnet test Sim/RiverCommand.Sim.Tests`.

Step 2 (project): create an HDRP project at Unity/ (Hub HDRP template, or the editor's
-createProject with the HDRP template from the editor's ProjectTemplates folder). Add packages
com.unity.nuget.newtonsoft-json and com.unity.inputsystem. Commit.

Step 3 (sim hookup): add an editor/build step that builds Sim/RiverCommand.Sim (Release,
netstandard2.1) and copies the DLL to Unity/Assets/Plugins/Sim/. Copy Content/ to
Unity/Assets/StreamingAssets/Content/ the same way. Confirm Unity compiles with no errors by running
the editor in batch mode and reading the log.

Step 4 (slice, use the unity-presentation agent): Boot scene that loads ContentDatabase from
StreamingAssets, creates SimWorld("rock_harbor"), and ticks it at 10 Hz with pause and 1x/2x/4x/8x.
Placeholder visuals: flat ocean plane, land as a simple mesh, boats as scaled boxes, incidents as markers.
Strategic camera (pan, wheel zoom, icons past 2 km altitude). Click to select, right-click Move,
Shift to queue, order lines. Incident feed panel with Search and Screen buttons. GAR dialog on launch.
Bar report on the HUD.

Rules: the sim owns all state; Unity never edits it. Run batch-mode compiles after each step and fix
errors before moving on. Commit after each step with a clear message and push to main.
```

## Open questions to settle on the PC
- Which exact Unity 6 LTS version is installed (keep everyone on it; record in `Unity/ProjectSettings`).
- Min-spec target GPU for profiling.
