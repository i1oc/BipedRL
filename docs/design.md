# BipedRL design

Design decisions for the robot, the physics, the learning setup and the code
architecture. How we work on the repository is described in
[CONTRIBUTING.md](../CONTRIBUTING.md).

## Robot

- **Base model:** Unitree H1, from
  [`unitree_rl_gym/resources/robots/h1/urdf/h1.urdf`](https://github.com/unitreerobotics/unitree_rl_gym)
  (BSD 3-Clause, see [THIRD_PARTY_NOTICES.md](../THIRD_PARTY_NOTICES.md)).
- **Legs only:** 10 actuated joints, 5 per leg: hip yaw → hip roll → hip pitch →
  knee → ankle (pitch). It is the simplest leg of Unitree's bipeds: a 1-DoF ankle
  and straight 0.4 m thigh and shank.
- **Torso:** the torso and arms are not simulated as moving parts. They are one fixed
  block on top of the pelvis with their **real mass** (about 24.5 kg out of a total of
  about 51.7 kg), so the dynamics stay close to the real robot.
- **Geometry:** primitive colliders and visuals (capsules and boxes). The real meshes
  may be added later as visuals only.

## Import

Our own C# editor tool reads the URDF and builds the robot:

1. A parser turns the URDF into plain data classes (links, joints, inertials, limits).
2. A builder creates the `ArticulationBody` hierarchy from that data. A factory
   returns a joint builder for each URDF joint type (revolute, fixed, floating).

URDF uses ROS conventions (Z-up, right-handed) and Unity is Y-up and left-handed; the
conversion is done in a single place.

We do not use Unity's URDF-Importer package: it has been unmaintained since 2022,
has known issues in Unity 6, does not set drive gains and adds a demo controller
that must be removed.

## Physics

- `ArticulationBody` (PhysX reduced coordinates): exact joints that do not stretch,
  and direct mapping from the URDF.
- Floating root, PD drives with `forceLimit` equal to the URDF effort limit.
- Reference gains from Unitree's H1 configuration: stiffness hip 150, knee 200,
  ankle 40; damping hip 2, knee 4, ankle 2.
- Default standing pose (rad): hip pitch -0.1, knee 0.3, ankle -0.2, others 0.

Pitfalls to keep in mind:

- `ArticulationDrive.target` is in **degrees**; `SetDriveTargets`,
  `SetJointPositions` and `jointPosition` are in **radians**.
- Resetting needs `TeleportRoot`, zero root velocities, `SetJointPositions`,
  `SetJointVelocities` and drive targets back to the default pose. Writing
  `transform.position` has no effect.
- Each joint's slot is found with `GetDofStartIndices`, never hard-coded.
- Robots in the same training scene must not collide with each other (layers).

## Learning (milestone 1: stand and resist pushes)

- **Algorithm:** PPO with Unity ML-Agents.
- **Observations (38).** Only what a real H1 can measure with its IMU, joint encoders
  and foot sensors:

  | Observation | Size |
  |---|---|
  | Base angular velocity | 3 |
  | Gravity projected into the base frame | 3 |
  | Joint positions minus default pose | 10 |
  | Joint velocities | 10 |
  | Previous actions | 10 |
  | Foot contacts | 2 |

  No world positions or other information a real robot would not have.
- **Actions (10, continuous in [-1, 1]):** joint target = default angle + 0.25 · action
  (rad), tracked by the PD drives. The policy runs at 50 Hz.
- **Reward:** weighted sum of terms (alive bonus; penalties for tilt, base height
  error, action rate, joint velocity, torque and joint limits). The weights live in a
  ScriptableObject.
- **Episode end (fail):** pelvis below a minimum height, torso tilt above a maximum
  angle, or any body other than a foot touching the ground.
- **Pushes:** random pushes on the pelvis whose strength grows during training.

## Game loop

- Main menu → level → Ready → Running → Won or Failed → Restart or back to the menu.
- **Level 1 win:** survive 20 s and 5 pushes of growing strength. **Fail:** the robot
  falls. The player can also push the robot.
- Each level can show the robot at three training stages (untrained, partially
  trained, final), which are checkpoints of the same training run stored as `.onnx`
  in `Assets/BipedRL/Policies/`.

## Design patterns

Only patterns that solve a real problem here. Every pattern used appears in the UML.

| Pattern | Where | Why |
|---|---|---|
| **State** | Game loop: MainMenu → Ready → Running → Won / Failed → Restart or Menu | The closed loop needs explicit states and transitions instead of boolean flags |
| **Observer** (C# events) | `RobotFell`, `PushApplied`, `LevelWon`, `LevelFailed`, `StateChanged` → HUD and UI | The simulation never references the UI (SRP, DIP) |
| **Strategy** | `IRewardTerm` (alive, tilt, height, action rate, joint limits…), `ITerminationCondition` (low pelvis, tilt, illegal contact); later `IPathfinder` | Each level composes its own list; new terms do not modify the agent (OCP) |
| **Facade** | `RobotBody` over `ArticulationBody` | Hides root teleporting, DOF indices and degree/radian conversions behind `SetJointTargets`, `ResetPose` and `ReadState` (Law of Demeter) |
| **Factory** | URDF importer: one joint builder per URDF joint type | New joint types are new classes, not new `if` branches |
| **Command** | Input System actions (Push, Restart, Pause); a push is a command object used by the player and by the random perturbator | Decouples input from the actions it triggers |

- **Singleton is avoided** on purpose: a bootstrap object creates and wires the
  dependencies explicitly.
- **Bridge and Flyweight are not used:** no problem in this project calls for them.
- **Decorator** only if sensor noise is added for robustness (wrapping sensors).
