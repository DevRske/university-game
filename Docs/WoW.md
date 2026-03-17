# Way of Working

<a id="1-the-tech-stack"></a>

## 1\. The Tech Stack

These are the core tools we will use to collaborate.

| Platform | Usage |
| --- | --- |
| **Jira** | **Project Management.** Used to plan sprints, track progress, distribute work, and centralize feedback. |
| **GitHub** | **Version Control.** Used strictly for code/asset storage and history. (Issue tracking happens in Jira, not GitHub). |
| **Unity** | **Game Engine.** Version: `6000.3.6f1` |
| **Discord** | **Communication.** Used for daily comms, meetings, and brainstorming. Decisions made here must be documented in Jira. |
| **Confluence** | **Documentation.** Linked to Jira. Used for Game Design Documents (GDD) and technical guides. |

* * *

<a id="2-project-management-jira"></a>

## 2\. Project Management (Jira)

<a id="21-work-units"></a>

### 2.1. Work Units

- **Epics:** High-level goals (e.g., "Combat System", "UI Overhaul"). These categorize the work.
- **Stories:** Specific design objectives. These may require creativity and involve multiple people (e.g., "As a player, I want to shoot enemies").
- **Tasks:** Small, technical units of work tackled by one person (e.g., "Implement Raycast logic").

<a id="22-ticket-stages"></a>

### 2.2. Ticket Stages

The lifecycle of a ticket on our Kanban board:

1. **To Do:** No progress made yet.
2. **In Progress:** Someone has assigned themselves to the ticket and is actively working.
3. **In Review:** Work is finished and a Pull Request (PR) has been created. The developer is waiting for feedback.
4. **Done:** The PR has been approved and merged into the `dev` branch.
5. **Cancelled:** The task is no longer needed (managed by Maintainers).

<a id="23-sprints"></a>

### 2.3. Sprints

- We work in **2-week sprints**.
- Work is prioritized at the start of the sprint.
- If you finish early, pick a task from the backlog—but ensure it is feasible to complete within the remaining time.

* * *

<a id="3-version-control-git"></a>

## 3\. Version Control (Git)

<a id="31-branch-strategy"></a>

### 3.1. Branch Strategy

We use a simplified Git-Flow workflow.

- `main`: **The Demo Branch.** Strictly operational. Contains the stable build to show university staff. *Only Maintainers can merge here.*
- `dev`: **The Development Branch.** All feature branches merge here. This is the active "working" version of the game.
- **Work Branches:** created by team members to tackle specific Jira tickets.

<a id="32-branching-rules"></a>

### 3.2. Branching Rules

1. **One Ticket = One Branch.**
2. **Naming Convention:** Branches must include the Jira Key.
  - *Format:* `[JiraKey]-[Description]`
  - *Example:* If the ticket is **SCRUM-77**, the branch is named `SCRUM-77-bullet-raycasting`.
3. **No Chaining:** Do not branch off another person's feature branch unless absolutely necessary. This prevents "Dependency Hell" if the original branch is rejected.
4. **Review Mandatory:** Every branch must be reviewed by at least one other peer before merging into `dev`.

* * *

<a id="4-unity-asset-workflow"></a>

## 4\. Unity & Asset Workflow

<a id="41-the-prefab-rule-crucial"></a>

### 4.1. The "Prefab" Rule (Crucial)

**Never work directly in main architecture scenes (e.g., Level1.unity).**

- Functionality must be built into **Prefabs**.
- To test a mechanic, create a temporary scene in a `_Sandbox/YourName` folder.
- Apply changes to the **Prefab**, not the Scene object.
- *Why?* Binary scene files cannot be merged. If two people edit the scene, one person loses their work.

<a id="42-git-lfs-meta-files"></a>

### 4.2. Git LFS & Meta Files

- **LFS (Large File Storage):** Do not upload binaries (Images, Audio, Models) larger than 100MB unless LFS is configured. Update `.gitattributes` immediately if adding a new file type (e.g., `.mp4`).
- **Meta Files:** Always commit the `.meta` file associated with your asset.
  - *Rule:* If you commit `Player.png`, you **must** also commit `Player.png.meta`. Failing to do so breaks references for everyone else.

* * *

<a id="5-development-workflow"></a>

## 5\. Development Workflow

*How to go from idea to finished feature:*

1. **Select:** Developer moves a ticket from **To Do** to **In Progress**.
2. **Branch:** Developer creates a branch (e.g., `SCRUM-55-jump-mechanic`) based on `dev`.
3. **Work:** Developer works and commits often.
  - *Commit Rule:* Ask: "If I break this, how far back do I want to revert?" (Aim for <500 lines, >50 lines per commit).
4. **PR:** Developer opens a Pull Request to `dev` and moves ticket to **In Review**.
  - *Note:* Use the PR template in GitHub.
5. **Review:** A team member reviews the code.
  - *Changes Requested?* Developer fixes them in the *same* branch and pushes again.
  - *Approved?* Reviewer merges the branch into `dev`.
6. **Close:** The ticket is moved to **Done**.

* * *

<a id="6-coding-standards"></a>

## 6\. Coding Standards

*Code must be maintainable by others.*

<a id="61-single-responsibility-principle-srp"></a>

### 6.1. Single Responsibility Principle (SRP)

**One Script, One Job.**

- **Bad:** `PlayerManager.cs` handles movement, health, and shooting.
- **Good:** `PlayerMovement.cs`, `PlayerHealth.cs`, `PlayerShooting.cs`.
- *Benefit:* If shooting breaks, movement still works. Multiple people can work on the Player simultaneously.

<a id="62-encapsulation"></a>

### 6.2. Encapsulation

**Never make a variable public just to see it in the Inspector.**  
Use `[SerializeField]` instead. Keep variables `private` unless other scripts explicitly need access.

```
// GOOD
[SerializeField] private float movementSpeed = 10f;

// BAD
public float movementSpeed = 10f; 
```

<a id="63-performance-optimization"></a>

### 6.3. Performance Optimization

**No** `Find` in Update Loop.  
Functions like `GetComponent()`, `Find()`, or `FindObjectOfType()` are slow.

- **Bad:** Calling these inside `Update()`.
- **Good:** Call them in `Awake()` or `Start()` and cache the reference.

<a id="64-no-magic-numbers"></a>

### 6.4. No Magic Numbers

Code should be self-documenting.

```
// BAD
if (temperature < 71) { ... }

// GOOD
float maxWeaponHeat = 71f;
if (currentTemp < maxWeaponHeat) { ... }
```

<a id="65-comments-safety"></a>

### 6.5. Comments & Safety

- **The Psycho Rule:** "Write code as if the person who has to maintain it is a violent psychopath who knows where you live."
- **Sandbox:** If you are unsure if your code will break the game, build it in `_Sandbox/YourName` first. Once working, ask a lead to help you clean it up and migrate it to the main folder.