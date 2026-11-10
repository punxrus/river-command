# River Command — project guide

Personal/portfolio RTS-management game: command a Coast Guard small boat station that grows into a sector.
Full design: https://claude.ai/artifact/SPyrzHPqD8SqYFcCYuNrLS (Phase 1 design document).

## Locked decisions
- Engine: Unity 6 LTS, HDRP. Windows 10/11 64-bit, keyboard + mouse.
- Simulation: engine-agnostic C# library in `Sim/`, fixed 10 Hz tick, the only thing saved.
- Content: JSON in `Content/`, one file per definition, every file has `schemaVersion`.
- Saves: versioned, gzipped JSON snapshot of sim state.
- Geography: fictional. Level 1 "Rock Harbor Station" is inspired by Station Quillayute River (La Push, WA)
  but uses a fictional town, and no real tribal names, symbols, or stories.

## Layout
```
Sim/RiverCommand.Sim/   game logic, netstandard2.1, C# 9 (Unity-compatible), no UnityEngine references
Sim/RiverCommand.Sim.Tests/  NUnit tests, net8.0
Content/                JSON definitions (Units, Incidents, Maps, Rules)
Unity/                  Unity project (created on the developer's Windows PC; presentation only)
docs/                   design notes, art briefs, story bible
.claude/agents/         specialist subagents (sim, unity, narrative, art)
```

## Rules for the simulation (`Sim/`)
- Never reference `UnityEngine`. Unity talks to the sim only through commands in and events out.
- Determinism: no `System.Random`, `DateTime.Now`, `Guid.NewGuid()`, or threads in game logic.
  Use `SeededRandom` and `SimClock`. Iterate lists, not dictionaries, when order affects results.
- Units: meters, seconds, knots only at the content/UI boundary (convert with `Conv.cs`).
- Every new rule gets a unit test. Tuning numbers live in `Content/`, not code.

## Commands
```bash
dotnet test Sim/RiverCommand.Sim.Tests
```

## Accuracy
Stats and procedures are placeholders until checked against public sources or a Coast Guard SME.
Mark unverified numbers with `"_note": "placeholder"` in JSON.
