# Game Design Document (v1.0)

<a id="project-title-working-title-tactical-top-down"></a>

# Project Title: \[Working Title: Tactical Top-Down\]

|     |     |
| --- | --- |
| **Attribute** | **Details** |
| **Genre** | Tactical 2D Top-Down Shooter |
| **Target Platform** | PC  |
| **Engine** | Unity 6.3 LTS |
| **Perspective** | Top-Down with Limited Vision Cone |
| **Players** | 3v3 (Multiplayer Online) |
| **Visual Style** | Minimalist "Raid Documentary" / Tactical Schematic |

* * *

<a id="1-executive-summary"></a>

## 1\. Executive Summary

A high-stakes, team-based tactical shooter played from a top-down perspective. Two teams of three (Attackers vs. Defenders) compete in tight, destructible environments. The game emphasizes information gathering, limited visibility, and strategic destruction over twitch reflexes. Unlike traditional shooters, wiping the enemy team does not guarantee victory for the Attackers—the mission objective (The Bomb) is paramount.

* * *

<a id="2-core-gameplay-loop"></a>

## 2\. Core Gameplay Loop

<a id="21-match-structure"></a>

### 2.1 Match Structure

- **Format:** Best of 3 Rounds.
- **Teams:** 3 Attackers vs. 3 Defenders.
- **Rotation:**
  - Round 1: Team A attacks / Team B defends.
  - Round 2: Sides Swap.
  - Round 3 (Tie-breaker): Sides assigned Randomly.
- **Game Modes:**
  - **Standard (PvP):** Ranked play with MMR adjustments.
  - **Simulation (PvE):** Co-op practice against AI bots using PvP rulesets.

<a id="22-win-conditions"></a>

### 2.2 Win Conditions

- **Defenders Win If:**
  - The Round Timer expires.
  - They eliminate all Attackers.
  - Attackers kill a hostage/fail a specific fail-state (if applicable).
- **Attackers Win If:**
  - They successfully defuse the Bomb.
  - **Critical Twist:** Eliminating all Defenders does **NOT** end the round. Attackers must still locate and defuse the bomb within the remaining time, or they lose.

* * *

<a id="3-player-mechanics-controls"></a>

## 3\. Player Mechanics & Controls

<a id="31-movement-vision-the-fog-of-war"></a>

### 3.1 Movement & Vision (The "Fog of War")

- **Control Scheme:** WASD for movement, Mouse for aiming.
- **Vision Cone:**
  - Players only see what is within a specific angle in front of their character.
  - The rest of the screen is obscured (Fog of War).
  - **Turn Speed:** Turning is not instantaneous; it has a rotational speed limit to simulate weight and prevent 360-degree "spinning" checks.
- **No Shared Vision:** Teammates' vision cones are **not** shared. If Player A sees an enemy, Player B does not see them on their screen/minimap. Communication is mandatory.

<a id="32-health-status"></a>

### 3.2 Health & Status

- **Friendly Fire:** Always ON.
- **Health State:**
  - **Active:** Full combat capability.
  - **Knocked (DBNO):** Player is down, cannot shoot/move, but can be revived by a teammate.
  - **Eliminated:** Permanent death for the remainder of the round.

<a id="33-audio-visual-feedback"></a>

### 3.3 Audio/Visual Feedback

- **"Visualizing Audio":** Since the view is top-down, loud actions (sprinting, shooting, explosions) generate visual "ripples" or indicators on the screen for nearby players, even through walls or outside the vision cone.

* * *

<a id="4-combat-loadouts"></a>

## 4\. Combat & Loadouts

<a id="41-loadout-system"></a>

### 4.1 Loadout System

- **Philosophy:** Free Pick (No Class restrictions).
- **Slots:**
  1. **Primary Weapon:** (e.g., Assault Rifle, SMG, Shotgun).
  2. **Secondary Weapon:** (e.g., Pistol, Machine Pistol).
  3. **Gadget:** One selectable utility item.

<a id="42-gadget-list-planned"></a>

### 4.2 Gadget List (Planned)

- **Frag Grenade:** Lethal explosive.
- **Flashbang:** Blinds vision cone and deafens audio.
- **Soft Breach Charge:** Quickly destroys soft walls/floors.
- **Hard Breach Charge:** The only way to open reinforced walls.

* * *

<a id="5-map-environment"></a>

## 5\. Map & Environment

<a id="51-map-logic"></a>

### 5.1 Map Logic

- **Zones:**
  - **Outside:** Safe zone for Attackers. Instant death (Sniper script) for Defenders who step out.
  - **Inside:** The building layout containing the Bomb.
- **Verticality:** Single floor layout.

<a id="52-destruction-hierarchy"></a>

### 5.2 Destruction Hierarchy

The environment is interactive and critical to strategy.

|     |     |     |
| --- | --- | --- |
| **Wall Type** | **Description** | **Destruction Method** |
| **Paper Soft** | Drywall, wood partitions. | Destroyed by bullets, shotguns, explosives. |
| **Structural Soft** | Thick wood, brick. | Immune to bullets. Requires Soft Breach or heavy explosives. |
| **Hard Wall** | Concrete, metal beams. | Indestructible unless used with **Hard Breach**. |
| **Reinforced** | A soft wall reinforced by Defenders. | Becomes a Hard Wall. |

<a id="53-defender-setup"></a>

### 5.3 Defender Setup

- **Reinforcement Pool:** A shared team resource (e.g., Team has 5 reinforcements total) to upgrade Soft Walls to Hard Walls during the preparation phase.

* * *

<a id="6-ui-ux"></a>

## 6\. UI & UX

- **HUD:** Minimalist. Ammo count, Health status, Round Timer, Current Objective status.
- **Art Style:** "Tactical Schematic." Clean lines, high contrast.
  - *Reference:* News broadcast visualizations of special ops raids.
  - *Vibe:* Clinical, serious, detached.