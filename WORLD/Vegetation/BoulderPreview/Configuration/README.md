# Boulder preview configuration

`BoulderMaster.tres` controls a deliberately small number of forest boulders (default 14). `RockCount` is capped at 40, with `MinimumSpacing` and `TreeClearance` preserving open ground. Rocks reuse the latest `ForestPreview.Settings` ellipse and tree sites. `EnableCollision` shares one convex shape across all placed instances; `CastShadows` and `DrawDistance` manage render cost. Restart the scene after changes.
