# Copilot Instructions for codingame

This repository contains solutions for CodinGame puzzles, organized by puzzle name and language. The codebase is structured for rapid prototyping and iterative improvement, with a focus on clarity and direct mapping to CodinGame's input/output requirements.

## Architecture Overview
- Each puzzle is in its own subdirectory under `puzzle/`, e.g., `ascii-art/`, `mars-lander-episode-2/`, `thor/`.
- Each puzzle directory contains a `Main.cs` (C#) and sometimes a `main.py` (Python) file. These are standalone entry points for CodinGame's online judge.
- There is no cross-puzzle code sharing; each solution is self-contained.
- The C# solutions use minimal dependencies and are designed to run in CodinGame's environment (single file, no external libraries).

## Key Patterns & Conventions
- **Input/Output:** All input is read from `Console.ReadLine()`, and all output is written with `Console.WriteLine()`. Debug output goes to `Console.Error`.
- **No Unit Tests:** The code is not structured for unit testing; correctness is validated by CodinGame's test cases.
- **Procedural/Minimal OOP:** Most solutions use a single class with static methods, except for more complex puzzles (e.g., `mars-lander-episode-2`), which may use multiple classes for clarity.
- **No Project-level Build Scripts:** Each solution is meant to be copy-pasted into CodinGame's editor. The `Puzzle.csproj` and `CodinGame.sln` are for local development only.
- **No external dependencies** beyond the .NET standard library.

## Developer Workflows
- To run a solution locally, open the relevant `Main.cs` in VS Code and use the C# extension to run/debug.
- Build with `dotnet build puzzle/Puzzle.csproj` (if using the project file), but this is optional.
- There are no automated tests or CI/CD workflows.

## Project-specific Details
- **Mars Lander Episode 2:** Uses multiple classes (`Cockpit`, `Lander`, `LandingZone`, `Coordinate`) to model the simulation. All logic is in a single file for CodinGame compatibility.
- **Ascii Art & Thor:** Use a single class with all logic in `Main`.
- **No shared utilities:** Each puzzle re-implements any needed logic.

## Example: Input/Output Pattern
```csharp
var inputs = Console.ReadLine()!.Split(' ');
int x = int.Parse(inputs[0]);
// ...
Console.WriteLine(result);
```

## When Contributing or Refactoring
- Keep each puzzle self-contained and compatible with CodinGame's single-file requirement.
- Avoid introducing dependencies or project-level complexity.
- Use `Console.Error.WriteLine` for debug output only.

---
For more, see the puzzle descriptions in each `Context.md` (if present) for rules and requirements.
