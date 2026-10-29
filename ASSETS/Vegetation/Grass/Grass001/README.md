# Bermuda grass variants

The imported model contains 21 small card meshes, 8–268 triangles each. Models/ and Textures/ preserve the exported relative paths. The glTF material is opaque; `Materials/GrassMaster.tres` supplies the uploaded separate alpha mask through an alpha-scissor shader with double-sided rendering. This material is ready to assign when instancing individual meshes for a grass trial. It does not rewrite the imported scene's materials automatically. Grass scattering is not enabled in test.tscn by this tree-only trial. Original glTF/PNG/JPG files remain unchanged.

Later placement should extract individual named mesh nodes, remove their Blender display offsets, and batch nearby patches with MultiMesh rather than instantiate this entire asset collection per tuft. Validate size, UV/mask alignment and two-sided lighting in engine. This shared shader has no script callbacks or wind animation.
