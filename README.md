# BipedRL

A Unity simulation of a legs-only bipedal robot, based on the real **Unitree H1**,
that learns to stand and walk from scratch with **reinforcement learning**
(Unity ML-Agents, PPO).

Final project for *Paradigmas y Técnicas de Programación* (Ingeniería Matemática e IA,
Comillas ICAI), academic year 2026/27.

## Goal

The robot starts with no knowledge of how to move. Through trial and error in a
physics simulation it learns, milestone by milestone, to:

| # | Milestone | Status |
|---|---|---|
| 1 | Stand and resist pushes | Planned |
| 2 | Walk on flat ground to a target | Planned |
| 3 | Walk on ramps and uneven terrain | Stretch goal |
| 4 | Climb low steps | Stretch goal |
| 5 | Climb stairs of variable height | Stretch goal |

Every milestone is released on its own and is playable without the next ones.

## How it works

- **Robot.** Only the legs and pelvis of the H1 are simulated: 10 actuated joints
  (hip yaw/roll/pitch, knee and ankle on each leg). The torso is a single fixed
  block with the real torso and arm mass. Joints use Unity `ArticulationBody`
  with PD drives limited to the real motor torques.
- **Learning.** At 50 Hz the policy reads what a real robot could measure (IMU,
  joint encoders, foot contacts) and outputs a target angle for every joint.
- **Application.** A main menu lets you pick a level per milestone and watch the
  robot at different training stages (untrained, partially trained, fully
  trained). Each level has a clear win and fail condition and can be restarted,
  and lets you interact with the robot, for example by pushing it.

## Repository layout

```
BipedRL/                  repository root
├─ BipedRL/               Unity project (open this folder with Unity Hub)
├─ training/              ML-Agents training configuration and Python setup
├─ docs/                  design notes, UML (draw.io) and delivery documents
├─ CONTRIBUTING.md        Git workflow, code conventions and Unity rules
└─ THIRD_PARTY_NOTICES.md licenses of third-party material
```

## Requirements

- Windows 10/11
- Unity **6000.6.8f1**
- Python **3.10.x** (3.10.1 to 3.10.12) for training only

## Getting started

### Run the simulation

1. Open the `BipedRL/` folder with Unity Hub (editor 6000.6.8f1).
2. Open the main menu scene and press Play.

### Set up training

From the repository root, in PowerShell:

```powershell
py -3.10 -m venv .venv
.\.venv\Scripts\Activate.ps1
python -m pip install --upgrade pip
pip install -r training\requirements.txt
mlagents-learn --help
```

Versions: ML-Agents 4.2.0 in Unity (with Sentis 2.6.1), `mlagents` 1.1.0 and
`torch` 2.8.0 (CUDA 12.8) in Python. Do not upgrade `torch` past 2.8: PyTorch 2.9 and
later break the ONNX export ML-Agents relies on.

### Train

With the virtual environment active, start a run from the repository root, then open
the training scene in Unity and press Play when the console asks for it:

| Milestone | Config | Training scene |
|---|---|---|
| 1. Stand | `training/config/stand.yaml` | `Assets/BipedRL/Scenes/Training/StandTraining.unity` |

```powershell
mlagents-learn training\config\stand.yaml --run-id=m1-stand-01 --results-dir=training\results
```

Resume a stopped run with the same command plus `--resume`, and follow the progress with:

```powershell
tensorboard --logdir training\results
```

- `<run-id>` format: `<milestone>-<short-description>-<nn>`, for example `m1-stand-01`.
- The chosen checkpoints are copied to `BipedRL/Assets/BipedRL/Policies/`. Training
  configs and chosen policies are committed; `training/results/` is not.

## Design

The robot, physics, learning setup and design patterns are described in
[docs/design.md](docs/design.md).

## How we work

Every change goes `feature/*` → `develop` → `main` through reviewed pull requests.
Each milestone is merged to `main` and tagged with a GitHub Release. The full rules
are in [CONTRIBUTING.md](CONTRIBUTING.md).

## Authors

- Pablo Hernández ([@i1oc](https://github.com/i1oc))
- Hugo Lara ([@hugo0607](https://github.com/hugo0607))

## Credits

Robot model derived from Unitree Robotics' H1 description (BSD 3-Clause). See
[THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).
