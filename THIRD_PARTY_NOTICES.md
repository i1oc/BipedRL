# Third-party notices

BipedRL uses or derives from the following third-party material.

## Unitree H1 robot description

- **What we use:** the H1 robot description (`h1.urdf`: link dimensions, masses,
  inertias, joint axes, joint limits and effort limits) and, as design reference,
  the default joint angles and PD gains published for the H1.
- **Source:** [unitreerobotics/unitree_rl_gym](https://github.com/unitreerobotics/unitree_rl_gym),
  `resources/robots/h1/urdf/h1.urdf`.
- **Changes:** the arms and head are not simulated. The torso and arms are kept as a
  single fixed block with their real mass on top of the pelvis. Collision and visual
  geometry are replaced by simple primitives.
- **License:** BSD 3-Clause, reproduced below.

```
BSD 3-Clause License
Copyright (c) 2016-2023 HangZhou YuShu TECHNOLOGY CO.,LTD. ("Unitree Robotics")
All rights reserved.
Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:
1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.
3. Neither the name of the copyright holder nor the names of its
   contributors may be used to endorse or promote products derived from
   this software without specific prior written permission.
THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
```

BipedRL is not affiliated with, sponsored by or endorsed by Unitree Robotics.

## Unity ML-Agents

- **What we use:** the `com.unity.ml-agents` package (Unity side) and the
  `mlagents` Python package (training), installed as dependencies. Their code is
  not copied into this repository.
- **Source:** [Unity-Technologies/ml-agents](https://github.com/Unity-Technologies/ml-agents).
- **License:** Apache License 2.0.
