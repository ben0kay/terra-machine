# Shovel — placeholder proof of concept

**Not implemented.** See [the excavation plan](../../../WORLD/Terrain/Excavation/EXCAVATION_IMPLEMENTATION_PLAN.md).

Proposed: press `1` to toggle/equip a fake shovel; click once to dig a small rounded depression at a nearby terrain hit. A HUD label or simple held placeholder is sufficient. Restrict reach/radius/cooldown via future Shovel settings; a failed dig must not modify terrain. Avoid a dedicated `_Process` for an idle tool.

Future owning files, when authorized: `Shovel.cs`, `Configuration/ShovelMaster.tres` (with accompanying typed settings C# resource if needed), optional lightweight art scene. Do **not** add empty C# classes or unreferenced Godot resources now.
