# PLAYER

`player.tscn` is the stable reusable first-person prefab. `PlayerController.cs` owns native capsule movement and event-driven look; `PlayerInput.cs` owns input sampling, jump edges, capture and focus release. No terrain/addon references. Root origin is at the feet; capsule is 1.8m high, camera 1.65m high.

`Configuration/PlayerMaster.tres` groups frequent tuning via `PlayerSettings`. Bindings are editable in Project Settings → Input Map. To use this player elsewhere, instance the scene on a valid collider. The test world suspends it until terrain collision loads through `SetMovementEnabled`.

Movement uses one continuous physics callback while enabled (including idle to retain floor/gravity contact); suspension stops that callback. Look is input-driven, with no render callback. No health, stamina, body mesh, crouch/prone or War Machine controller dependencies are imported. Only one local active player/camera is intended in this first pass.

Local validation: build, check arrow-key movement and relative movement after turning, Shift speed, single RMB jump, slope/landing behavior, Escape/click recapture and focus-loss input release. Inversion and sensitivity are configurable before running. Runtime and compilation are unverified here.

The optional debug overlay saves/restores MovementEnabled and InputBlocked while open. InputBlocked prevents click-to-recapture and mouse look during UI/benchmark interaction; normal gameplay defaults remain unblocked. No debug UI dependency exists in PLAYER.

DG1 adds Equipment/PlayerEquipment.cs and a placeholder Shovel child to the stable prefab. Key 1 toggles selection; LMB while captured queues one physics-frame excavation request through a service supplied by world composition. No native types/calls belong to PLAYER. PlayerInput now consumes mouse recapture and emits CaptureReleased to cancel pending tool work on Escape/focus loss. No new idle frame callback. Local checks: ../WORLD/Terrain/Excavation/DG1_VALIDATION.md.
