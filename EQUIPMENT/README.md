# EQUIPMENT

Future player-carried, selectable tools live here; this is **documentation-only scaffolding**, not implemented inventory or equipment.

`Tools/Shovel/` is the initial placeholder tool experiment. Keep input/equip state out of terrain integration. Future tools should reuse the terrain excavation contract rather than call Voxel Tools directly.

The existing `PLAYER/PlayerInput.cs` is responsible for capturing input, not for calculating excavation or holding geological rules. Keep player functionality modular and preserve the existing movement controller.
