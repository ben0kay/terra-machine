# Fir visual preview

`TreePreview.tscn` is a reusable, small authored group for `WORLD/Scenes/test.tscn`. `Configuration/TreePreviewMaster.tres` selects A/B/C GLB scenes, twelve fixed XZ offsets relative to spawn, scale range, ground sink, alpha cutoff and loading budget. The default group is 11–55m in front of spawn (negative Z); the path down its centre remains open.

`TestWorld` explicitly calls Initialize after its first successful grounded spawn. The helper uses only ITerrainService. Up to four ground rays per 0.2s retry interval run during initial chunk loading; placement stops completely after all trees are placed or after 40s. F4 does not duplicate existing trees. Remaining unloaded candidates are skipped with one warning. This is a bounded visual preview, not a streamed forest population system or persistent placement.

Original imported mesh pivots include Blender display offsets for B (+5m X) and C (+10m X). The wrapper cancels the first mesh origin without altering GLBs. Deterministic yaw and modest scale variation reuse the actual models. Only source material clones are modified: needle alpha blending becomes Alpha Scissor at 0.35 cutoff, with anisotropic mip filtering. Clones are shared across instances of each source material; no per-tree material copies or scripts/ticks. Imported geometry/automatic mesh LOD settings remain unchanged.

These trees are **visual-only**: no trunk collision, chopping, wind or terrain-edit support. They will not re-ground after future excavation. GroundSink accommodates the small root extension below Blender's zero plane. Check root contact, scale and leaf cutouts locally. Disable/remove this scene to compare terrain alone; removing it is supported by TestWorld's optional reference.

GLB inspection confirmed one LOD1 mesh node per variant and six embedded images per file. This corrects the previous cleanup README's unverified-binary limitation for this subsequent job. Normal/roughness/color textures in the GLBs remain embedded and untouched. The inherited source-pack "1K" label is not used to assert all tree image dimensions.

Optional render debugging: SetDebugRendering controls root visibility, cached mesh shadow flags and shared cloned-material unshaded mode. Later-placed meshes honor the active options. These switches do not alter placement simulation. See ../../../DEBUG/Overlay/README.md.
