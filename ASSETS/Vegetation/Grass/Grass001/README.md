# Bermuda grass variants

The imported model contains 21 small card meshes, 8–268 triangles each. Models/ and Textures/ preserve the exported relative paths. The glTF material is opaque; `Materials/GrassMaster.tres` supplies the uploaded separate alpha mask through an alpha-scissor shader with double-sided rendering. WORLD/Vegetation/GrassScatter now assigns this material to batches of individual meshes in test.tscn. It does not rewrite the imported scene's materials automatically. The shader also thins/shrinks tufts with camera distance; configuration is supplied by the placement helper. Original glTF/PNG/JPG files remain unchanged.

Placement extracts individual named mesh nodes, removes their Blender display offsets, and batches nearby tiles with MultiMesh rather than instantiate this entire asset collection per tuft. Validate size, UV/mask alignment and two-sided lighting in engine. This shared shader has no script callbacks or wind animation.
