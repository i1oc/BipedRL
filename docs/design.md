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
- Robots in the same training scene must not collide with each other. For standing they
  are placed 4 m apart, more than a robot travels before its episode ends; walking
  milestones will need to revisit this.

## Learning (milestones 1 and 2: locomotion)

We follow Unitree's own H1 training
([`unitree_rl_gym`](https://github.com/unitreerobotics/unitree_rl_gym), `h1_config.py` and
`h1_env.py`): a single policy learns to walk at a requested velocity while stepping to a
fixed rhythm. Level 1 asks for zero velocity, so the robot steps in place and recovers
from pushes by stepping; level 2 asks for the velocity towards the target.

A first stand-only policy learned to stand but resisted pushes only by bending its knees:
it never stepped and fell after forward pushes. Stepping has to be trained, not hoped for.

- **Algorithm:** PPO with Unity ML-Agents.
- **Velocity target:** forward, sideways and turning speed in the pelvis frame. While
  training it is random and changes every 10 s, as in Unitree's training: walking speeds
  up to 1 m/s (below 0.2 m/s they become zero) and a random heading the robot must turn
  to. Unitree calls it a command; we call it a target so it is not mistaken for the
  Command pattern.
- **Gait clock:** a phase that repeats every 0.8 s, with the right leg half a cycle
  behind the left one. The rewards ask each foot to touch the ground in the first 55 % of
  its leg's cycle and to lift during the rest.
- **Observations (41), in Unitree's order and scales.** What a real H1 can measure with
  its IMU and joint encoders, plus the target and the clock:

  | Observation | Size |
  |---|---|
  | Base angular velocity | 3 |
  | Gravity projected into the base frame | 3 |
  | Velocity target | 3 |
  | Joint positions minus default pose | 10 |
  | Joint velocities | 10 |
  | Previous actions | 10 |
  | Sine and cosine of the gait phase | 2 |

  No world positions or other information a real robot would not have.
- **Actions (10, continuous in [-1, 1]):** joint target = default angle + 0.75 · action
  (rad), tracked by the PD drives. ML-Agents clips the network output to [-3, 3] and
  divides it by 3, so this is Unitree's 0.25 rad per unit of network output. Physics runs
  at 100 Hz (Unitree uses 200 Hz) and the policy decides every second step, at 50 Hz.
- **Reward:** weighted sum of Unitree's H1 terms: following the velocity target, stepping
  to the clock, a set swing height, no foot slip, an upright pelvis at the right height,
  smooth actions, staying away from joint limits, hips and knees off the ground, and an
  alive bonus.
- **Episode end (fail):** the pelvis touches the ground, or it leans more than 0.8 rad
  sideways or 1.0 rad forwards or backwards. Episodes last 20 s.
- Each reward term and termination condition is a ScriptableObject asset holding its
  weight or threshold, and an episode configuration asset lists the ones in use, so each
  level or training stage can combine them differently.
- **Robustness, as in Unitree's training:** random ground friction and added pelvis mass
  per episode, noise on the observations, and a push every 5 s that sets the pelvis
  horizontal velocity to a random value of up to 1.5 m/s per axis.

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
| **Strategy** | `IRewardTerm` (velocity tracking, gait contact, height, action rate…), `ITerminationCondition` (pelvis contact, lean limit), `IVelocityTargetSource` (random for training, fixed for level 1, towards the goal for level 2); later `IPathfinder` | Each level composes its own list and target source; new terms do not modify the agent (OCP) |
| **Facade** | `RobotBody` over `ArticulationBody` | Hides root teleporting, DOF indices and degree/radian conversions behind `SetJointTargets`, `ResetPose` and `ReadState` (Law of Demeter) |
| **Factory** | URDF importer: one joint builder per URDF joint type | New joint types are new classes, not new `if` branches |
| **Command** | Input System actions (Push, Restart, Pause); a push is a command object used by the player and by the random perturbator | Decouples input from the actions it triggers |

- **Singleton is avoided** on purpose: a bootstrap object creates and wires the
  dependencies explicitly.
- **Bridge and Flyweight are not used:** no problem in this project calls for them.
- **Decorator** only if sensor noise is added for robustness (wrapping sensors).
