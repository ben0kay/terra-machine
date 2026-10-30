# Player equipment

PlayerEquipment is the minimal DG1 controller: key 1 toggles one placeholder shovel, captured LMB forwards one activation, and an event-driven label shows feedback. No general inventory/hotbar is introduced.

The prefab owns a Shovel child; TestWorld binds the optional ExcavationService through BindExcavation. Equipment checks PlayerInput.Captured (including InputBlocked) and PlayerController.MovementEnabled, and snapshots the camera ray on the click. The tool rechecks state on its single physics callback. Escape/focus release and F4/unequip cancel pending work. Equipment has no process callback and no native voxel calls.

Input Map: equip_shovel (physical top-row 1) and tool_primary (LMB). PlayerInput consumes click-to-recapture before any tool activation. Existing movement/look/jump/debug behaviour is retained. Local validation: ../../WORLD/Terrain/Excavation/DG1_VALIDATION.md.
