# Terra Machine — Vegetation & Natural Asset Visual Authoring Guide

> **Status:** Initial art-direction guide (2026-10-29). Applies to future AI-assisted and human-authored natural 3D assets.  
> **Purpose:** Preserve the grounded, detailed-but-efficient look established while iterating the mature oak, northern conifer, and mossy cover boulder.  
> **Scope:** **Visual design, geometry, PBR materials, LOD art, and portable exports.** This is NOT the gameplay/placement/collision specification. A separate structural/runtime authoring guide can define collisions, vegetation streaming, wind integration, destruction, interaction, and save behaviour later.

Read the root `README.md` and `CodePreferences.md` before authoring. The current priority is one attractive first-person wilderness benchmark in `WORLD/Scenes/test.tscn`, **not** a massive procedural generator. Assets should remain usable both in this benchmark and in later deterministic world placement. Visual quality must be judged in Godot from player eye level, not only in Blender.

## Required step — choose and inspect a reference asset FIRST

**Before modelling any new tree, rock, grass, flower or other natural asset, find the closest existing Terra Machine model and study it.** A new asset should belong to the same believable, grounded art family, not start from generic procedural defaults. A written description alone is not enough when actual meshes and textures are available.

| New asset request | Preferred starting reference | What to study |
| --- | --- | --- |
| Pine, spruce, fir or other conifer | **`ASSETS/Vegetation/Trees/PineTree002/`** (authored northern pine, `Models/` and `Textures/`) | Tall narrow proportions, open tiered needle sprays, central leader, branch spacing, natural rounded base, 1K materials and near/far LOD construction |
| Mature broadleaf tree (oak, maple, etc.) | **Mature Oak A V2** from the earlier design iteration **if the model is available**; use a current user-supplied example or reference images otherwise | Broad asymmetric crown, powerful curved limbs, bark mapping and varied leaf cluster depth. Do not reuse pine growth architecture for an oak |
| Large boulder, rock outcrop or smaller stone | **Boulder001** (our first mossy cover boulder) **when its model is available** | Angular broken stone masses, believable ground contact, low geometry cost, localized moss, colour/normal/roughness mapping and near/far LODs |
| **Meadow / natural grass clumps** | **`ASSETS/Vegetation/Grass/GrassMeadow002/`** (preferred lightweight meadow reference); compare `Grass001` for short Bermuda grass | 54–66-triangle near clumps, 24–30-triangle far clumps, 1K shared photographic colour/alpha atlas, full leafy silhouettes rather than distinct stiff stalks |

**Availability caveat (checked 2026-10-29):** `PineTree002`, `Grass001`, and `GrassMeadow002` are present in this repository. **Boulder001 and Mature Oak A V2 were produced separately during earlier asset-design work; their source model files are not confirmed in the current GitHub tree.** Do not invent paths or claim to have read them. When the owner adds their files to the repo, update this table with the verified directories (suggested boulder home: `ASSETS/Nature/Boulders/Boulder001/`). Until then, request/use their supplied ZIP, glTF/GLB or screenshots as available, and clearly distinguish visual references from inspected geometry.

### Required reference workflow for AI authors

1. **Identify the asset family and target:** species/geology, biome, purpose, target dimensions and intended density/quality tier. Choose the closest reference from the table or a newer owner-designated example. **The owner's specific image/reference takes priority for the requested silhouette**, while the existing asset keeps materials and export quality consistent.
2. **Open and inspect the actual reference** when accessible: mesh geometry, silhouette from front/side/3/4/top, scale and pivot, triangle counts, material slots, UVs, colour/normal/roughness/alpha maps, foliage construction, and LOD naming. Read its README. If only screenshots are available, say what cannot be verified.
3. **Explain what carries over and what changes:** e.g. for a new pine retain PineTree002's realistic branch-tier language, material strategy, and ground contact, but create a genuinely new silhouette; for a new rock retain Boulder001's fractured geology and moss placement, but use a distinct shape.
4. **Keep family resemblance without cloning:** match the grounded level of detail, shader/material behaviour, natural colours, believable scale, and efficient mesh methods. Do **not** copy the reference object's exact branch positions, rock outline or foliage cards blindly. Specific species and rocks must still look physically distinct.
5. **Compare outputs side by side** at the same human scale, similar view angle and lighting. Check both close-up material quality and a distant silhouette. Report what was improved, what differs and any remaining weaknesses.
6. **Record reference provenance** in the new asset README: exact reference path or supplied file/image, what was inspected, what was only inferred, plus any third-party material origins and licence. Don't assume references missing from the repo exist on the author's local computer.

**Default rule:** When asked for “another pine,” start by inspecting **PineTree002**, not a generic tree generator. When asked for “another boulder,” start with **Boulder001** if supplied or available, not a random noisy sphere. If the closest asset is inaccessible, state that and use the best actual reference provided—never claim to have inspected a missing file.

---

## 1. Art direction: the look to keep

**North star:** A believable, atmospheric **northern Canadian / boreal and mixed woodland**—grounded semi-realism, detailed natural materials, recognizable species, and strong silhouettes. Attractive in close-up, readable across a valley, inexpensive enough for repeated use.

- **Organic but not chaotic:** Avoid perfect symmetry, uniform cone/sphere crowns, repeated identical branch spacing, or obviously mathematical noise. Use controlled asymmetry and a few dominant structural forms before adding small detail.
- **Silhouette before polygons:** First make an identifiable tree/rock in flat shading at 20–80 m. Textures should enhance a strong shape, not rescue a weak one.
- **Realistic scale:** Work in metres; approximately 1 Blender/Godot unit = 1 m. Check against a **1.8 m person**. Never claim a species' precise dimensions from a reference image alone.
- **Natural colour palette:** Bark greys/browns, subdued needle greens, varied leaf greens, weathered neutral stone, localized moss. Avoid saturated uniform green, excessive gloss and conspicuously repeated colour patterns.
- **Layered detail:** Large structural masses → secondary branch/rock forms → fine texture and foliage cards. Let normal/roughness maps supply microdetail instead of modelling it all.
- **Grounded roots and bases:** Lower tree trunks and boulders should look embedded in terrain, not suspended or resting on a flat plate. Allow a little visual overlap or terrain sink later; don't build unnatural star-like root spikes.
- **Irregular natural variation:** Give every asset family several meaningful silhouettes/branch or surface variants—not only random yaw or colour.
- **Consistent but not cartoonish:** Slight stylization is acceptable to preserve readability and performance. Do not chase costly photorealism if the result is less usable in a forest.

### Lessons from our first custom assets

These are **art-direction references**, not a claim that every model is currently committed in this repo.

| Reference | Keep | Avoid / improvement learned |
| --- | --- | --- |
| Mature Oak A V2 (custom design) | Broad asymmetric crown, weighty flared trunk, spreading curved branches, layered foliage, photographic/PBR bark and better circumference-aware UVs | Straight twig forests, long hanging foliage curtains, repeated round leaf blobs, stretched bark at forks |
| PineTree002 (current repo) | Tall ~22.7 m boreal spire, tiered gently drooping branch whorls, visible central trunk, controlled gaps, slim silhouette | V1's dense green curtain, generic Christmas-tree cone and flat star-shaped base |
| Boulder001 (custom design) | Large (~5.5 × 3.5 × 4.3 m) asymmetric cover-sized form, broken planar faces, localized moss, textured microdetail | Smooth ellipsoid, regular low-poly gemstone, all-over green moss, no convincing ground contact |
| Grass001 (current repo) | Very low-poly cutout tuft shapes, multiple forms, natural size/rotation variation, shared materials | One mesh/node/script per blade, dense opaque cards, huge tufts with no variation, alpha sorting everywhere |

Reference paths in the repository: `ASSETS/Vegetation/Trees/PineTree002/README.md`, `ASSETS/Vegetation/Grass/Grass001/README.md`, and **`ASSETS/Vegetation/Grass/GrassMeadow002/README.md`**. The runtime trial belongs to `WORLD/Vegetation/` and is a separate concern.

## 2. Geometry recipes by asset family

### A. Broadleaf trees (oak, maple and similar)

1. Establish the trunk's **weight and character** first: subtle lean or twist; taper from a thicker, irregular base; no perfect cylinder.
2. Build 3–7 major structural limbs with visibly different angles and lengths, then secondary curved, tapering branches. Branching should follow load and available light, not a mirrored radial template.
3. Make the crown from **overlapping, irregular leaf-cluster volumes** with pockets of visible sky and interior shadow. Cluster boundaries should be uneven and recognizable from multiple angles.
4. Use leaf cards / small low-poly clusters with a carefully masked **oak-specific leaf silhouette**. Avoid generic long drooping leaf strips that read as willow or fern.
5. Make wood and foliage separate material surfaces. Thick limbs remain visible near the central trunk, without leaving a forest of dark, unsupported twig spikes below the canopy.
6. Align bark UVs with branch length; adjust texture frequency by branch circumference so tiny branches do not display the same oversized bark ridges as the trunk.
7. Add mild bark colour changes at roots and large forks, but do not fake geometry with an obviously repeating flat overlay.

### B. Northern pines, spruces and firs

1. Prioritize a **tall, narrow, slightly irregular spire**; keep a continuous central leader and taper toward a finer top.
2. Construct distinct branch **whorls / horizontal layers**. Lower branches are generally longer, with gently drooping tips; the top crown is shorter, lighter and tighter.
3. Preserve visible gaps between tiers and occasional open views of the trunk. Reduce unnecessary foliage on the shaded inside of the crown.
4. Build needle sprays from **crossing/angled textured cards or compact cluster meshes**; don't model individual needles unless justified by a close-up asset.
5. Break symmetry: vary azimuth, tier height, branch length, density and occasional missing/damaged branches. The silhouette should not become randomly ragged.
6. Make the base a naturally rounded taper/flared trunk, perhaps a few short curved root forms partly buried. **No flat circular skirt, floating disc or sharp radial root spikes.**
7. Use dark/cool greens with restrained lighter tips. At distance, branch-tier rhythm and outer silhouette matter more than tiny needle detail.

### C. Boulder / rocky landscape props

Although these may live in `ASSETS/Nature/` rather than `ASSETS/Vegetation/`, **apply this visual standard** to them.

1. Start from an **asymmetric mass**—elongated, tilted, flattened on one side, or partially buried. Give it a useful readable profile at 1.8 m human scale.
2. Introduce several dominant fracture planes, chipped corners, ledges and broad changes in slope. Avoid perfect spheres and evenly repeated triangulated noise.
3. Keep micro-surface grain, pores, tiny cracks and subtle colour changes in the **normal/albedo/roughness**, not thousands of miniature geometry chips.
4. Moss is **localized**: upper horizontal surfaces, moist crevices and sheltered patches. Leave significant rock visible on vertical faces.
5. Check front, 3/4, side, rear and top views. It should still look like a believable boulder from every direction.
6. For potential cover-sized rocks, visually target a meaningful mass (e.g. ~3–6 m wide and ~2–4 m high), then review scale in-game. **Collision and cover rules come later.**

### D. Grass, ferns, flowers and small plants

1. Prefer **small, readable clumps**, not isolated hyper-detailed leaves/blades. A few well-oriented alpha-cutout planes or lean meshes are usually sufficient.
2. Provide several silhouettes/heights per species and local colour variation. The cluster base should sit at the mesh origin.
3. Use an authored colour+alpha foliage atlas with clean edge padding and proper mipmaps; consider normal maps only where visibly useful.
4. Grass tips can sway using **vertex deformation in a shared shader** with root-weighting or geometry masks so the base stays planted. Wind is a later runtime integration, not a requirement for every exported model.
5. Avoid individual per-plant script updates and expensive transparent blending when masked foliage works.
6. Author assets that can be batched by mesh/material. The current GrassScatter uses `MultiMesh` tile batches; do not copy its live range or density settings into the mesh geometry itself.

### E. GrassMeadow002 case study — detailed appearance with very few triangles (preferred method)

**Follow this for future grass, fern, weed, moss and low-growing foliage assets where a detailed silhouette can come from a texture.** The result looked better than increasing the number of stiff stalk-shaped GrassMeadow001 clumps in the user's meadow screenshot. The key improvement was the *asset's silhouette and source foliage texture*, not simply adding density.

**Verified reference in this repository:** `ASSETS/Vegetation/Grass/GrassMeadow002/` (read its `README.md`, and inspect its `Models/`, `Textures/` and `Materials/` before creating a similar asset).

| Example | Near mesh triangles per A/B/C variant | Far mesh triangles per A/B/C variant | Observation |
| --- | --- | --- | --- |
| **GrassMeadow002** | **60 / 66 / 54** (LOD1) | **24 / 30 / 24** (LOD2) | Fuller, softer green meadow tuft despite very little geometry; all variants share a 1K colour+alpha texture |
| GrassMeadow001 (earlier tall asset) | ~204–228 (LOD1) | ~72–78 (LOD2) | More geometry did **not** cure separated, stiff-looking yellow stalks when clustered in the scene |
| GrassMedium001 (source collection) | Highly variable, some individual meshes **thousands** of triangles | Varies by source LOD and mesh | Avoid selecting a high-poly mesh simply because its source pack is named “grass” |

**The modelling method:**

1. Start with **good real grass/leaf imagery with clean cutout alpha**, preferably matching the desired meadow species and colour. Detailed photos and believable leafy silhouettes do much of the visible work.
2. Form a **compact individual tuft** from a small set of *crossed, angled, intersecting low-poly cards/strips*. One tuft should contain enough apparent leaves to read as full, but each leaf does **not** need its own 3D blade mesh. Cards should have UVs positioned on the foliage texture, not empty transparent atlas space.
3. **Shape the whole tuft**, not just its polygon budget: use varied heights, tilt, radial spread and a fuller lower-to-middle section. Avoid rigid parallel stalks, evenly spaced fans, overly regular X-card crosses and conspicuous isolated bundles.
4. Author **several distinct A/B/C shapes** for natural repetition breaking. They share one texture/material and consistent pivot but differ in width, posture, leaf angle and height. A/B/C are **appearance variants**, **not** LOD numbers.
5. Create a second, cheaper mesh for each shape, keeping the same scale and silhouette. GrassMeadow002 demonstrates **54–66 triangles near and 24–30 far**. These are *achieved examples*, not minimums or strict budgets for all grass and ferns.
6. Keep **one or very few materials**, a shared ~1K colour+alpha atlas, properly padded/mipmapped alpha, alpha-scissor/masked cutouts where viable, and double-sided rendering only as needed. Prefer alpha-cutout rather than full alpha blending for many overlapping tufts.
7. Store the **base/pivot on the ground**, with no large invisible display offset; avoid a flat rectangle or visible card base. Reuse the same material across A/B/C and their LODs.
8. **Compare a single tuft and a dense patch at player-eye level** against the reference model before accepting it. The same tuft that looks nice alone can look sparse, overly yellow, repetitive or like vertical sticks when repeated. Screenshots from neutral daylight and the actual Godot scene matter more than a software-preview triangle figure.
9. If a meadow has visible gaps, **first correct plant shape, colour, layering and scattering distribution**. Increasing density or clumping a poor source mesh can make the flaws more obvious and can increase GPU cost without achieving a continuous meadow.
10. For large populations, use **Godot MultiMesh/shared batching**, reasonable patches and scene LOD/distance thinning rather than nodes, collision objects or C# updates for every tuft. Wind can use one shared vertex shader; root vertices should remain relatively anchored.

**Performance warning: low-poly does not automatically mean cheap.** The 54–66 triangles are a genuine *geometry* saving compared with heavier assets, but many large alpha-cutout cards can still create substantial **pixel overdraw, mip/alpha shimmer, bandwidth, wind/shadow cost and instance/draw cost**. Measure with actual density, viewing angle, sun and shadows. A 1K atlas does not guarantee good performance if most pixels are discarded. Record the actual counts per mesh, materials, textures and test conditions; never claim an FPS gain without a measured before/after comparison.

**Visual design rule for future AI:** Prioritize **a realistic foliage silhouette baked into clean textures + very lightweight intersecting card geometry + A/B/C variation + shared atlas**, rather than spending thousands of triangles modelling grass blade-by-blade. Preserve the **good visual result of GrassMeadow002** as the benchmark, but do not assume every future plant should be exactly 60 triangles.

## 3. PBR materials and UV rules

- Prefer **1K** colour/normal/roughness maps for small/medium vegetation and repeatable assets; use **2K only when a close-up visual test proves an improvement**. The 1K/2K suffix describes texture resolution, *not* model complexity.
- Albedo/base colour: sRGB PNG or JPG; normal: **OpenGL (+Y) tangent-space** map; roughness: single-channel or packed as appropriate; metallic normally **0** for wood, leaves, soil and ordinary stone.
- Preserve UV maps, vertex normals and tangents when needed. Test for flipped normals, seams, mirrored shading and stretched grain at tapered branches/roots.
- Bark UVs should run lengthwise along trunk/branch geometry. Keep a consistent physical bark scale rather than blindly repeating a texture a fixed number of times around every cylinder.
- Prefer photographic/reworked PBR bark and rock textures with tasteful tint/contrast control over visually obvious procedural line noise. Shared textures across variants/LODs are desirable.
- Leaves/needles: use an image with proper alpha, masked/alpha-scissor materials and two-sided cards **only where necessary**. Avoid translucent alpha blending for dense foliage unless explicitly justified. Preview shadow behaviour and mipmapped edges.
- Add macro-scale colour variation via geometry vertex colours or shader variation sparingly, to prevent a tiled or copy-pasted appearance.
- If different LOD meshes share the same materials, reference the **same external texture files** rather than copying one set per mesh.
- Keep source asset provenance and licences in the asset README. When adapting third-party maps, record the source and any material changes; do not claim original authorship of those photographs.

## 4. Geometry / performance budgets — starting points, not hard requirements

| Art category | Near / main visual target | Distance / simplified target | Most important GPU risk |
| --- | --- | --- | --- |
| Mature showcase broadleaf tree | ~25k–50k triangles | ~6k–15k | Foliage overdraw and shadows |
| Tall boreal conifer | ~15k–25k triangles | ~5k–10k | Dense overlapping needle cards |
| Large cover boulder | ~2k–6k triangles | ~500–1.5k | Too many unique materials/draws |
| Small plant / bush | Hundreds to a few thousand, depending on plant | Much less or culled | Alpha overdraw and instance count |
| Grass tuft | **~40–100** triangles for a simple meadow cutout tuft (starting goal, not a rule) | **~20–40** triangles, or culled/faded | Total instances, alpha overdraw, shadows |

Known reference data: PineTree002 LOD1 **23,644** triangles; LOD2 **6,972**. Grass001 source card meshes range **8–268** triangles. **GrassMeadow002 A/B/C: LOD1 = 60/66/54 and LOD2 = 24/30/24 triangles per clump.** Earlier GrassMeadow001 LOD1 was ~204–228 triangles per clump; inspected GrassMedium001 source meshes vary widely, including versions with several thousand triangles. Custom Mature Oak A V2 was approximately **45,820** triangles; custom Boulder001 approximately **5,120/1,280** triangles. These values are examples from our asset iterations, not measured real-world FPS limits. Do not label an asset “optimized” merely because it falls under a triangle threshold.

**Practical guidelines:**

- Spend triangles on silhouette and recognizable branch/rock forms, not hidden interior geometry.
- Prefer shared textures, fewer material surfaces, instancing and scene/mesh LODs. Be mindful of alpha-card **overdraw**, draw calls and shadow caster cost.
- Build each distance LOD as genuinely simplified geometry; a different filename alone doesn't enable distance switching.
- Godot may generate importer mesh LODs, but authored `LOD1`/`LOD2` glTF files are **not automatically switched** by name. Runtime switching and draw distances belong to the separate vegetation implementation.
- A good hero tree is not necessarily a good mass-instanced tree. Test with realistic densities and camera angles, not just one model in isolation.
- Never add an always-active C# script to a decorative mesh merely to animate or check it. Shared shaders, managers and deferred placement are preferable when runtime work is needed.

## 5. Asset packaging, naming and export

**Preferred for newly authored natural assets:** glTF Separate, so multiple versions can reference a single `Textures/` folder. This matches the checked-in PineTree001/PineTree002 structure. Self-contained `.glb` remains valid for a quick preview or a deliberately single-file asset (e.g. the existing FirSaplingMedium collection). Choose **one intended source format per new asset family** rather than tracking redundant copies without reason.

Suggested directories (follow existing asset category conventions; do not mass-rename existing files):

```text
ASSETS/
  Vegetation/
    Trees/
      PineTree003/
        Models/
          pine_tree_003_a_LOD1.gltf
          pine_tree_003_a_LOD1.bin
          pine_tree_003_a_LOD2.gltf
          pine_tree_003_a_LOD2.bin
        Textures/
          pine_tree_003_bark_diff.png
          pine_tree_003_bark_nor_gl.png
          pine_tree_003_bark_rough.png
          pine_tree_003_needles_diff.png
        README.md
    Grass/
      Grass002/
        Models/
        Textures/
        README.md
  Nature/
    Boulders/
      Boulder002/
        Models/
        Textures/
        README.md
```

- Names should identify **family, variant (A/B/C), and LOD**. Conventional `LOD0` = most detailed, followed by lower-detail `LOD1`, `LOD2`, etc. If an existing asset uses LOD1 as its closest available model, **preserve and document that** rather than silently renaming it LOD0.
- In Blender export **only the intended selected mesh(es)**; don't include the source scene's demonstration lineup, extra LOD collections, cameras, lights or geometry-node staging objects.
- **Apply/verify transforms**, consistent up-axis and a meaningful pivot: tree/plant origin at ground level near trunk/stem centre; boulder origin near a useful ground-contact centre. Avoid carrying display-row offsets into the exported object. Check scale in Godot.
- For glTF Separate, ensure `.gltf` references its matching `.bin` and texture URIs such as `../Textures/example.png`. Do not break those relative paths by moving folders independently.
- For GLB, embedded textures can stay embedded in Godot (e.g. Basis Universal import); Godot may otherwise extract images. Do not delete extracted files unless the import settings and model are verified.
- Keep only game-needed textures and model files in the project. Keep original bulky `.blend` files, full Poly Haven downloads and large archive backups outside the Git repository unless source tracking is intentional (Git LFS where needed). Keep `.import` metadata when Godot uses it and ignore generated `.godot/` caches.
- An optional authored `.blend` or reproducible modelling script can be kept in an asset-specific `Tools/` directory where reasonable; it is **not** required to ship Godot-compatible model files.

## 6. Quality-control checklist for every new asset

Before calling a new asset “game-ready”, include evidence for the following:

- [ ] Reference and intended species/shape are identified; proposed dimensions checked against a **1.8 m player**.
- [ ] Front, side, 3/4, rear and top views show an appealing, non-repetitive silhouette.
- [ ] Ground-contact base looks natural; no star spikes, flat skirts, clipping gaps or unintended floating geometry.
- [ ] Trunk/branch/rock UVs have sensible scale; materials do not stretch noticeably at joints, roots or cliffs.
- [ ] Normals, face winding, texture channels, opacity cutouts and shadows look correct from both sides where applicable.
- [ ] Textures are reused between variants and LODs where practical; external paths resolve.
- [ ] Real triangle counts recorded **per LOD**, alongside dimensions, material/surface count and texture dimensions.
- [ ] Materials and foliage are examined under daylight **in Godot**, not only in Blender Material Preview.
- [ ] LOD comparison is inspected at roughly the intended screen sizes; no obvious silhouette popping or missing crown structure.
- [ ] Current game performance impact is measured separately (FPS/frame time, foliage overdraw, shadows, instance counts) before claiming any benchmark gain.
- [ ] Per-asset README records origin/licensing, intended use, limits, export instructions and any unverified runtime behaviour.

### Minimal asset README example

```markdown
# <AssetName>
- Purpose / visual reference:
- Species or material / biome fit:
- Source and licence (or original-author statement):
- Dimensions (m) and model pivot:
- Available variants and LODs (actual triangle counts):
- Material slots and shared textures:
- Export format / expected folder layout:
- Art decisions and known visual compromises:
- Blender visual check:
- Godot visual and performance check (done / pending):
- Later technical work (collision, placement, wind, interactions): pending
```

## 7. Scope boundary / future companion guide

This document describes **what natural objects should look like and how to deliver their render assets**. Do **not** infer collision, damage, harvesting, navigation blocking, hiding/cover, placement, wind simulation, vegetation persistence, voxel-edit responses or procedural population behaviour from it.

A later **structural authoring guide** can specify: `StaticBody3D` and collision modes; cover sizes; collision LOD; wind vertex weights and runtime materials; ground-normal alignment; streaming/pooling; breakable trees; save IDs; interactions; and reference prefab `.tscn` structure. Until separately implemented, art exports should stay visual-only.

**Default AI workflow:** read this guide → propose species/asset silhouette + triangle/texture targets → create a standalone model and materials → inspect multiple views and actual counts → export in the preferred folder layout → document what was verified → only then ask to integrate into the game world.
