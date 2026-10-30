# Forest test configuration

`ForestMaster.tres` is the single resource for adjusting the bounded forest patch. By default its centre is 18 m east and 118 m toward the flat, opposite side of spawn (positive Z) from the world spawn, with 82 × 86 m elliptical radii. The current `TreeCount = 115` is an upper bound; rejected spacing, original trees, cliffs and unloaded collision may result in fewer placed trees.

`CenterSpacing = 7.5` m makes the middle thicker than the 14 m minimum near the edge. `TrailHalfWidth = 3` m excludes a narrow winding lane oriented toward the configured forest centre (positive Z by default). `OriginalTreeClearance = 8` m keeps additional pines away from the 16 authored preview trees. The seed makes it reproducible; changing density/position/seed takes effect when restarting the test scene.

`PineTree` references the existing PineTree002.tscn with its native 70/280 m near/far LOD visibility ranges. `NearShadows` controls near LOD shadow casting, far LOD shadows are disabled. `PlacementRange` is the radius in which actual native terrain collision is probed near the player; the 0.6 s refresh timer stays active while points remain unplaced. Lower the tree count or turn off `NearShadows` first if performance suffers.

## Mixed pine and fir understory (2026-10-29)

Mature pines: `TreeCount = 130` (was 115) and `CenterSpacing = 6.4` m (was 7.5), while `EdgeSpacing = 14` m is unchanged. This thickens the **core** while retaining a spacious perimeter. The original 8 tall PineTree003 selections remain among the mature trees.

`SaplingVariants` points to the existing A/B/C FirSaplingMedium wrapper scenes, which already include the shared trunk collision. `SaplingCount = 34` caps a separate deterministic understory pass biased within 76% of the forest radii. Saplings are at least 2.9 m from a mature pine, around 5.5 m from another sapling, within 12 m of a mature pine, and excluded from the winding walking gap. Their scale varies 0.85–1.14 and yaw covers 360°. `SaplingShadows = false` and `SaplingCullDistance = 125` m keep this optional scenery inexpensive. Terrain placement uses the forest's existing ray budget and low-frequency retry timer. `Forest` HUD count includes both species, and the separate `Fir saplings` count helps verification.
