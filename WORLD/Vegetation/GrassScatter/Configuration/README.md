# Grass configuration

`GrassMaster.tres` owns mesh selection, density, distance bands, slope limit, scale, seed and work budgets. Restart the test scene after edits. `TuftsPerSquareMeter` counts candidates before patch/slope rejection; candidates are capped at 2048 per tile. Runtime clamps tile size to 8–32 m and draw range to 15–80 m to keep this trial bounded. Asset cutout/normal/roughness inputs live in ASSETS/Vegetation/Grass/Grass001/Materials/GrassMaster.tres. Start with density and scale before increasing distances.

Bermuda defaults were loosened to 8 candidates/m², 62% minimum patch coverage and 12% distant density. The optional `NearScenes/FarScenes` and `HideOutsideArea` settings allow a *second* GrassScatter to use authored meadow meshes in the forest ellipse; original GrassScatter scene/settings remain independent.
