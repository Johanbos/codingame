# CodinGame Solutions Repository

This repository contains self-contained solutions for various CodinGame puzzles, implemented in C# and Python. Each puzzle is organized in its own subdirectory under `puzzle/`, with a focus on clarity, rapid prototyping, and direct mapping to CodinGame's input/output requirements.

## Repository Structure
- `puzzle/`
  - `ascii-art/` — Solution for the ASCII Art puzzle (`Main.cs`, `Main.py`)
  - `mars-lander-episode-2/` — Solution for Mars Lander Episode 2 (`Main.cs`, `Context.md`)
  - `thor/` — Solution for Power of Thor (`Main.cs`, `main.py`)
- `.github/`
  - `copilot-instructions.md` — AI agent instructions for contributing and refactoring
  - `agents/` — Custom agent definitions for advanced AI workflows

## Key Conventions
- Each puzzle solution is a single file, designed for copy-paste into CodinGame's online editor.
- No shared code or dependencies between puzzles.
- Input is always read from `Console.ReadLine()`, output with `Console.WriteLine()`, and debug output with `Console.Error.WriteLine()`.
- No unit tests or CI/CD; correctness is validated by CodinGame's test cases.

## Custom Agents Usage
This repository supports advanced AI coding workflows using custom agents, as inspired by the [awesome-copilot](https://github.com/github/awesome-copilot) collection. Custom agents are defined in `.github/agents/` and can:
- Provide expert guidance for C# or Python solutions
- Enforce repository-specific conventions and best practices
- Automate repetitive coding, refactoring, or documentation tasks
- Integrate with Copilot or other AI tools for context-aware code generation

To use a custom agent:
1. Reference the relevant agent file in `.github/agents/` (e.g., `CSharpExpert.agent.md`).
2. Follow the agent's instructions for code style, structure, and workflow.
3. When using Copilot or compatible tools, ensure the agent's rules are loaded for optimal results.

For more on custom agents and advanced AI workflows, see the [awesome-copilot](https://github.com/github/awesome-copilot) repository.

---
For puzzle-specific rules and requirements, see the `Context.md` file in each puzzle directory.
