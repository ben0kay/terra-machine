# Placeholder shovel — DG1 awaiting local validation

Press 1 to toggle the equipped test shovel. A small bottom-left label is the placeholder visual; no held art/inventory is required. Captured LMB schedules one request for the next physics tick, then the tool sleeps again. Holding does not repeat. Configuration/ShovelMaster.tres sets 3.5 m reach, 1 m radius, -0.75 m surface inset (0.25 m shallow cap depth at default 1.0) and 0.65 s cooldown.

The tool calls ExcavationService, never native voxel APIs. PlayerEquipment supplies player/input state and forwards selection/activation. Loading, blocked UI, released mouse/focus, unequip and respawn prevent/cancel pending actions. Click-to-recapture is explicitly consumed. The player prefab still works without a terrain service and gives unavailable feedback.

Terrain/props are distinguished by the native adapter's first physical hit on layers 1+4. No digging through a trunk/rock. Native requests cover only loaded padded bounds. Shovel settings are checked against excavation service limits; large edits are refused, not silently clamped.

No held mesh, stamina, hardness, inventory drops, audio/noise, animations or persistence in DG1. See ../../../WORLD/Terrain/Excavation/DG1_VALIDATION.md for build/run and contact/idle checks. Compilation and actual native-binary behaviour remain unverified.

F1 EXCAVATION → Dig amount changes a session-only depth multiplier (0.25–6, default 1). The brush radius remains one metre because simply shrinking it below voxel spacing can produce inconsistent/no visible cuts. Amount is applied by ExcavationService; the tool settings resource remains unchanged by UI interactions. Ground foliage rooted in accepted cut spheres is now removed by composition/scatter owners. Native edit bounds and cooldown remain capped.
