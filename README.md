# [Working Title: Tactical Top-Down]

A 3v3 tactical top-down shooter built in Unity for a university module.

Attackers and Defenders fight in destructible environments with limited vision, high information pressure, and objective-focused round logic.

## Overview

This project focuses on delivering a stable, well-engineered playable prototype for PC.

Core gameplay direction:
- 3v3 online multiplayer (Attackers vs Defenders)
- Top-down perspective with client-side Fog of War
- Destructible modular environment (prefab-based, not tilemaps)
- Tactical objective play where Attackers must defuse the bomb to win

## Core Features

- Objective-first round system
  - Attackers win by defusing the bomb
  - Defenders win on timer expiry or attacker elimination
- Limited vision cone per player
  - No shared vision between teammates
- Destruction hierarchy for walls
  - Soft, Structural Soft, Hard, Reinforced
- Visualized audio cues
  - Loud actions can create visual ripples
- Free-pick loadouts
  - Primary, Secondary, Gadget

## Tech Stack

- Engine: Unity `6000.3.6f1`
- Platform: PC
- Networking: Host-client model (NGO or Photon Fusion host/shared mode)
- Version Control: GitHub
- Planning/Tracking: Jira

## Repository Structure

```text
Assets/
  _Sandbox/        # Personal testing scenes
  Core/            # Networking and shared systems
  Prefabs/         # Production-ready prefabs
  Scenes/          # Main menu and maps
  Scripts/         # Gameplay scripts
Docs/
  Architecture.md  # Technical architecture
  GDD.md           # Game design document
  WoW.md           # Team workflow and coding standards
Packages/
ProjectSettings/
```

## Getting Started

### Prerequisites

- Unity Hub
- Unity Editor `6000.3.6f1`
- Git

### Setup

1. Clone the repository.
2. Open the project with Unity `6000.3.6f1`.
3. Let Unity import packages and compile scripts.
4. Open a sandbox scene under `Assets/_Sandbox/` for local feature testing.

## Team Workflow (Important)

- Branch naming: `[JiraKey]-[Description]`
- One ticket = one branch
- Open PRs into `dev`
- Keep `main` stable for demos

Unity workflow rules:
- Do not build new functionality directly in shared architecture scenes
- Build and test in prefabs and sandbox scenes
- Always include matching `.meta` files for asset changes

## Coding Standards

- Apply SRP: one script, one responsibility
- Use `[SerializeField] private` for Inspector exposure
- Cache component lookups in `Awake()`/`Start()`
- Avoid `Find`/`GetComponent` inside `Update()` loops
- Avoid magic numbers

## Documentation

Project source-of-truth lives in:
- `Docs/GDD.md`
- `Docs/Architecture.md`
- `Docs/WoW.md`

## Status

In active sprint-based development for a university module.

Current focus:
- Stable multiplayer round flow
- Core combat loop
- Destruction and vision systems

---

If you are contributing, start by checking Jira, then follow the branch and prefab rules above before opening a PR.
