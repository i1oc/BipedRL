# Contributing to BipedRL

How we work on this repository: Git workflow, code conventions and Unity rules.
The robot, physics and learning design is described in [docs/design.md](docs/design.md).

## Git and GitHub workflow

### Branches

| Branch | Purpose | Receives changes from |
|---|---|---|
| `main` | Released milestones only | `develop`, via PR |
| `develop` | Integration | `feature/*`, via PR |
| `feature/<issue>-<kebab-name>` | Any work: features, fixes, docs, setup | Branched from up-to-date `develop` |

- Example: `feature/12-urdf-parser`.
- There are no other branch types. A quick fix after a feature was merged is a new
  short `feature/<issue>-fix-<something>` branch with its own PR to `develop`.
- Start every branch from an updated `develop`:
  ```powershell
  git switch develop
  git pull
  git switch -c feature/<issue>-<name>
  ```
- Resolve conflicts locally by merging `develop` into the feature branch
  (`git fetch origin`, `git merge origin/develop`), never in the GitHub web editor.

### Protection (`main` and `develop`)

- A pull request with **1 approval** is required; authors cannot approve their own PR.
- Stale approvals are dismissed when new commits are pushed.
- Conversations must be resolved before merging.
- Force pushes and branch deletion are blocked, with no bypass for anyone.
- Only **merge commits** are allowed (no squash or rebase); head branches are
  deleted automatically after merging.

### Commits

- Plain imperative summary in English, no type prefixes, at most 72 characters:
  `Add URDF joint limit parsing`, `Fix drive targets using radians`.
- Optional body explaining **what and why**, wrapped at 72 characters.
- Reference the issue in the body when useful: `Refs #12`.
- Before committing, run `git status` and check that no unintended scene, prefab or
  settings changes are included.

### Pull requests

- The base branch is **`develop`**. GitHub suggests `main` (the default branch), so
  change it when opening the PR.
- The PR description includes:
  - what changes and why;
  - `Closes #<issue>` to link the issue;
  - design notes: patterns or principles applied, and why;
  - Unity assets touched (scenes, prefabs, settings), to avoid conflicts;
  - how to test it, step by step.
- The reviewer reads every file in *Files changed*, leaves at least one line comment,
  checks out the branch locally to verify it compiles and runs when there is code,
  and then approves or requests changes.
- **The author merges** after the approval.

### Issues and milestones

- Issues have a short description of what is needed and a list of acceptance
  criteria. Default GitHub labels are used (`enhancement`, `documentation`, `bug`).
- `Closes #<issue>` only closes issues automatically when merging into the default
  branch (`main`). Our PRs target `develop`, so **after merging, close the issue by
  hand** with the comment `Done in #<pr> (merged into develop).`
- Milestones: `Delivery 1 - Design` (due 2026-12-03), `Milestone 1 - Stand and resist pushes`,
  `Milestone 2 - Walk to targets`, `Milestone 3 - Ramps`, `Milestone 4 - Low steps`,
  `Milestone 5 - Stairs` and `Delivery 2 - Final` (due 2027-01-10). Issues are
  detailed only for the current milestone.
- The issue assignees on GitHub define who does what.

### Releases

When a milestone is done and playable on its own, `develop` is merged into `main`
through a PR, and then:

```powershell
git switch main
git pull
git tag -a v0.2.0 -m "Milestone 1: stand and resist pushes"
git push origin v0.2.0
```

followed by a GitHub Release from the tag with the Windows build attached.

| Version | Content |
|---|---|
| `v0.1.0` | Delivery 1 (description and architecture) |
| `v0.2.0` | Milestone 1: stand and resist pushes |
| `v0.3.0` | Milestone 2: walk to targets |
| `v0.4.0`+ | Milestones 3 to 5 |
| `v1.0.0` | Delivery 2 (final) |
| `v0.x.1` | Fixes released from `develop` |

## Unity

### Editor settings

Set when the project was created; never change them:

- Asset Serialization: **Force Text**.
- Version Control: **Visible Meta Files**.

### Avoiding scene and prefab conflicts

- One owner per scene or prefab at a time. The issue says which assets it touches.
- Small scenes: one per level and one per training setup. Scenes only place prefabs.
- Change prefabs (nested prefabs), not scene overrides.
- Tunable values live in ScriptableObjects (reward weights, level settings).
- Prefer wiring in code over long chains of inspector references.
- Pull `develop` before opening a scene. On a scene conflict, keep one side and redo
  the other change by hand; never merge the YAML blindly.

### Unity Smart Merge

`.gitattributes` sends Unity YAML files to the `unityyamlmerge` merge driver. Configure
it once in your local clone (adjust the path to your editor installation):

```powershell
git config merge.unityyamlmerge.name "Unity SmartMerge"
git config merge.unityyamlmerge.driver "'C:/Program Files/Unity/Hub/Editor/6000.6.8f1/Editor/Data/Tools/UnityYAMLMerge.exe' merge -h -p --force %O %B %A %A"
git config merge.unityyamlmerge.recursive binary
```

## Code

### Project layout

```
BipedRL/                                   repository root
├─ BipedRL/                                Unity project
│  └─ Assets/
│     ├─ BipedRL/
│     │  ├─ Scripts/Runtime/               game and simulation code
│     │  ├─ Scripts/Editor/                editor tools (URDF importer)
│     │  ├─ Scenes/  Prefabs/  Configs/    (Configs = ScriptableObject assets)
│     │  └─ Policies/                      trained .onnx models used by the levels
│     └─ ThirdParty/Unitree/H1/            h1.urdf (BSD-3, see THIRD_PARTY_NOTICES.md)
├─ training/
│  ├─ config/                              ML-Agents trainer .yaml files
│  ├─ requirements.txt
│  └─ results/                             training output (ignored)
└─ docs/
   ├─ design.md                            robot, physics and learning design
   ├─ uml/                                 .drawio sources and PDF/PNG exports
   └─ delivery1/                           project description
```

Folders are created by the first feature that needs them (Git does not keep empty
folders, and Unity would leave orphan `.meta` files).

### C# conventions

- `PascalCase`: classes, structs, enums, methods, properties, events.
- `camelCase`: fields, parameters, local variables.
- Interfaces start with `I` (`IRewardTerm`).
- No public fields: use `[SerializeField] private` fields and properties.
- One public type per file, named after the type.
- Namespaces under `BipedRL`: `BipedRL.Robot`, `BipedRL.Agents`, `BipedRL.Training`,
  `BipedRL.Game`, `BipedRL.UI`, `BipedRL.Editor`.
- Comments explain *why*, not *what*. Everything is in English.

### Principles

SOLID, DRY, KISS, YAGNI and the Law of Demeter (no `a.GetB().C.Do()` chains).
Avoid spaghetti code, God Objects, dead code and Golden Hammer solutions.
Design patterns are used only where they solve a real problem; see
[docs/design.md](docs/design.md#design-patterns).
