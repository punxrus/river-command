# River Command

A real-time strategy and management game about commanding a Coast Guard small boat station.
Read the board, send the right boat with the right crew, and keep your people safe while the clock runs.

Status: **Phase 2, vertical slice, in progress.** Personal / portfolio project.

## What's here
| Folder | Contents |
| --- | --- |
| `Sim/` | Game logic as a plain C# library, plus tests. Runs without Unity. |
| `Content/` | Boats, incidents, rules, and map data as JSON. |
| `docs/` | Level design, story bible, art briefs. |
| `.claude/agents/` | Specialist Claude Code subagents for mechanics, Unity, story, and art. |

## Run the tests
Needs the .NET 8 SDK.
```bash
dotnet test Sim/RiverCommand.Sim.Tests
```

## Unity
The Unity 6 (HDRP) project goes in `Unity/`. The game logic compiles to a .NET Standard 2.1 DLL that Unity loads.
See `docs/unity-setup.md`.

## Disclaimer
Fan-made, non-commercial, not affiliated with or endorsed by the U.S. Coast Guard.
All places, people, and vessels in the game are fictional.
