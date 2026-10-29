# Forest test configuration

`ForestMaster.tres` is the single resource for adjusting the bounded forest patch. By default its centre is 18 m east and 118 m toward the flat, opposite side of spawn (positive Z) from the world spawn, with 82 × 86 m elliptical radii. The current `TreeCount = 115` is an upper bound; rejected spacing, original trees, cliffs and unloaded collision may result in fewer placed trees.

`CenterSpacing = 7.5` m makes the middle thicker than the 14 m minimum near the edge. `TrailHalfWidth = 3` m excludes a narrow winding lane oriented toward the configured forest centre (positive Z by default). `OriginalTreeClearance = 8` m keeps additional pines away from the 16 authored preview trees. The seed makes it reproducible; changing density/position/seed takes effect when restarting the test scene.

`PineTree` references the existing PineTree002.tscn with its native 70/280 m near/far LOD visibility ranges. `NearShadows` controls near LOD shadow casting, far LOD shadows are disabled. `PlacementRange` is the radius in which actual native terrain collision is probed near the player; the 0.6 s refresh timer stays active while points remain unplaced. Lower the tree count or turn off `NearShadows` first if performance suffers.
