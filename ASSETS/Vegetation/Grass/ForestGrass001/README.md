# ForestGrass001

Three authored variants (A/B/C), each with LOD1 and LOD2, plus a dark atlas and wind materials. All model, buffer, texture, material and shader filenames and resource references use ForestGrass001 / forest_grass_001. Internal glTF mesh/node names match each variant's filename. The green-named atlas is a retained identical copy of the dark atlas from the upload; existing materials use the dark atlas.

Renaming preserves geometry, texture bytes, import settings and UIDs. Corrected the uploaded glTF buffer URIs (previously meadow_grass_002) and material paths (previously ForestGrass003). These assets are now actively used by the forest's `WORLD/Vegetation/GrassScatter/Configuration/ForestMeadowMaster.tres` (from 2026-10-30), in place of Meadow Grass 2. The original seed, patch positions, density, sizing, near/far LOD switch and distance bands were retained. The active forest material is `Materials/ForestGrass001Wind_DistanceFade.tres`, using the dark atlas. The old Meadow Grass 2 remains near the starting firs.

Validation: parsed all six glTF files, checked buffer byte lengths and local texture/material/shader references. Godot import/rendering remains to be verified locally after pulling.
