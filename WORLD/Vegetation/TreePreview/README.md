# Fir and pine visual preview

`TreePreview.tscn` is a reusable, small authored group for `WORLD/Scenes/test.tscn`. `Configuration/TreePreviewMaster.tres` selects twelve fir A/B/C instances plus four PineTree002 LOD1 instances, sixteen fixed XZ offsets relative to spawn, scale range, ground sink, alpha cutoff and loading budget. The default group is 11–55m in front of spawn (negative Z); the path down its centre remains open.

`TestWorld` explicitly calls Initialize after its first successful grounded spawn. The helper uses only ITerrainService. Up to four ground rays per 0.2s retry interval run during initial chunk loading; placement stops completely after all trees are placed or after 40s. F4 does not duplicate existing trees. Remaining unloaded candidates are skipped with one warning. This is a bounded visual preview, not a streamed forest population system or persistent placement.

Original imported mesh pivots include Blender display offsets for B (+5m X) and C (+10m X). The wrapper cancels the first mesh origin without altering GLBs. Deterministic yaw and modest scale variation reuse the actual models. Only source material clones are modified: alpha-blended fir needles become Alpha Scissor at 0.35 cutoff, with anisotropic mip filtering. Already-cutout materials retain their imported threshold (PineTree002: 0.39). Clones are shared across instances of each source material; no per-tree material copies or scripts/ticks. Imported geometry/automatic mesh LOD settings remain unchanged.

These trees are **visual-only**: no trunk collision, chopping, wind or terrain-edit support. They will not re-ground after future excavation. GroundSink accommodates the small root extension below Blender's zero plane. Check root contact, scale and leaf cutouts locally. Disable/remove this scene to compare terrain alone; removing it is supported by TestWorld's optional reference.

GLB inspection confirmed one LOD1 mesh node per variant and six embedded images per file. This corrects the previous cleanup README's unverified-binary limitation for this subsequent job. Normal/roughness/color textures in the GLBs remain embedded and untouched. The inherited source-pack "1K" label is not used to assert all tree image dimensions.

Optional render debugging: SetDebugRendering controls root visibility, cached mesh shadow flags and shared cloned-material unshaded mode. Later-placed meshes honor the active options. These switches do not alter placement simulation. See ../../../DEBUG/Overlay/README.md.

## PineTree002 trial

The existing twelve firs are retained. Four pines use offsets (-19,-12), (22,-17), (-32,-39), (36,-48), leaving the centre open. Variants is an ordered list matching Offsets; repeated scene references reuse imported resources. The pines use the same grounding, scale/yaw variation, timeout and dormant placement helper. No PineTree001 asset is referenced. The F1 visibility/shadows/unshaded controls and comparison now cover all sixteen trees; HUD reports Trees rather than Firs.

PineTree002 export inspection: LOD1 23,644 triangles; LOD2 6,972; original height ~22.74 m including roots, no display translation. Texture paths resolve; needles use MASK 0.39 and double-sided cards. The trial references LOD1 only; the separately exported LOD2 file is not automatically switched in by this helper. Godot's normal import-generated mesh LODs remain available. No trunk collision, chopping or wind was added.

Validation: C# API compilation and Godot 4.7.2 isolated import/resource checks completed. Full Godot .NET build, in-world placement/appearance, canopy alpha edges and F1 performance acceptance remain local checks. Existing source models/textures are untouched. Startup placement is bounded at four rays per 0.2 seconds and stops after sixteen placements or timeout; no new per-frame tree work.
