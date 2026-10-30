# Grass configuration

`GrassMaster.tres` owns mesh selection, density, distance bands, slope limit, scale, seed and work budgets. Restart the test scene after edits. `TuftsPerSquareMeter` counts candidates before patch/slope rejection; candidates are capped at 2048 per tile. Runtime clamps tile size to 8–32 m and draw range to 15–80 m to keep this trial bounded. Asset cutout/normal/roughness inputs live in ASSETS/Vegetation/Grass/Grass001/Materials/GrassMaster.tres. Start with density and scale before increasing distances.

Bermuda defaults were loosened to 8 candidates/m², 62% minimum patch coverage and 12% distant density. The optional `NearScenes/FarScenes` and `HideOutsideArea` settings allow a *second* GrassScatter to use authored meadow meshes in the forest ellipse; original GrassScatter scene/settings remain independent.

The flat-side forest now uses `ForestMeadowMaster.tres` configured for three GrassMeadow002 LOD1/LOD2 scenes, with `MinimumScale = 1.25`, `MaximumScale = 1.65`, 1.4 candidate tufts/m², and unchanged 15 m patch spacing/5.8 m radius. `ForestMeadow002Wind.tres` enables forest-only wind/distance fade while the spawn Meadow002 material retains its previous settings. See `FOREST_MEADOW_README.md` for the current recipe and F1 comparison steps.


The flat-side ForestMeadowMaster now targets lighter clusters: 0.75 candidates/m², 1.4 m minimum root clearance *within each tile*, 85% short clumps at scale 0.75–0.95, 15% taller accents at 1.30–1.65 (weighted mean ~35% below the prior 1.25–1.65 uniform range). The old 15 m patch spacing and 5.8 m patch radius remain. `TallTuftChance=0` and `MinimumTuftSpacing=0` are safe defaults for other GrassScatter instances, including Bermuda.
