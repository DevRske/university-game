# AI Agent Onboarding Guide

Welcome to the **[Working Title: Tactical Top-Down]** repository! This document serves as a quick-start guide for AI agents to understand the project's context, constraints, and coding standards. 

## 0. Ambiguity & Clarification
- **The "Ask First" Rule**: If a task description is vague, or if a design decision isn't explicitly covered in `Docs/`, **DO NOT GUESS**. 
- **Stop and ask** for clarification before generating code. State what you believe the goal is and provide 2-3 brief options for how to proceed.
- **State Assumptions**: If you must make a minor technical assumption to stay within the current sprint's scope, you must explicitly list that assumption at the very top of your response.

## 1. Project Context & Mindset
- **Team**: 4 university students.
- **Goal**: Deliver a stable, well-engineered playable game for a university module. 
- **Scope & Mindset**: The target is always **quality and simplicity**. Avoid complexity, over-engineered patterns, and over-optimizing for extreme edge cases (e.g., no complex server-side anti-cheat). 
- **Sprint-Based Development**: We work iteractively in sprints. **Do not think ten miles ahead**. We develop prototypes further step-by-step; solve the immediate problem simply and cleanly.

## 2. Main Documentation (`Docs/`)
The `Docs/` folder in the root directory will **always be present** and contains the definitive source-of-truth for this project. Always refer to these files for expanded details:
- **`Docs/Architecture.md`**: Technical infrastructure, networking architecture, and core systems breakdown.
- **`Docs/GDD.md`**: Game Design Document detailing mechanics, win conditions, and UX/UI.
- **`Docs/WoW.md`**: "Way of Working" covering Git branching, project management, and code standards.

## 3. Tech Stack & Architecture Basics
- **Engine**: Unity `6000.3.6f1` (Target Platform: PC).
- **Networking**: Client-Host model (Unity NGO or Photon Fusion). We inherently trust the client to reduce networking overhead and complexity.
- **Destruction**: Environments use a **Modular Prefab Grid** (NOT Tilemaps). Walls have specific health and material types (Soft, Hard, Reinforced) and handle network-synced destruction via RPCs.
- **Vision System**: "Fog of War" is calculated strictly client-side using raycasts and Unity rendering layers (stencil buffers).

## 4. Hard Rules & Coding Standards

When writing or modifying code, adhere strictly to these rules defined in our Way of Working (`WoW.md`):

### 4.1. The Prefab Rule (CRITICAL)
- **NEVER** modify main architecture scenes directly.
- All new functionality must be built into **Prefabs**. Test mechanics in temporary sandbox scenes (e.g., `_Sandbox/[DevName]/`).
- *Why?* Binary scene conflicts will destroy work. Only merge tested prefabs into the `Prefabs/` folder.

### 4.2. Unity Code Standards
- **Single Responsibility Principle (SRP)**: Keep scripts aggressively focused. Don't write monolithic managers (e.g., instead of `PlayerManager.cs`, use `PlayerMovement.cs`, `PlayerHealth.cs`, `PlayerShooting.cs`).
- **Encapsulation**: Use `[SerializeField] private` for exposed Inspector variables. Do not use `public` fields for editor assignment.
- **Performance**: Never use `GetComponent()`, `Find()`, or `FindObjectOfType()` inside an `Update()` loop. Cache references in `Awake()` or `Start()`.
- **No Magic Numbers**: Define constants or serialized variables with descriptive names instead of hardcoding raw numbers.

### 4.3. Version Control & Assets
- **Meta Files**: You **must** commit the corresponding `.meta` file whenever adding, modifying, or deleting any asset.
- **Branch Naming**: Uses the format `[JiraKey]-[Description]` (e.g., `SCRUM-55-jump-mechanic`).

## 5. Core Gameplay (At a Glance)
- **Genre**: 3v3 Top-Down Tactical Shooter (Attackers vs Defenders).
- **Win Condition**: Attackers win by defusing the bomb, NOT simply by wiping the defender team. Defenders win if the timer runs out or if attackers are eliminated.
- **Vision & Audio**: Players have limited vision cones. Loud noises (sprinting, shooting) create visual "ripples" on the ground to simulate auditory feedback visually.
- **Loadouts**: Free pick (Primary, Secondary, Gadget) with no class restrictions.
