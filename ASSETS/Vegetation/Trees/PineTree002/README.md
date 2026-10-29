# Pine Tree 002

Owner-authored pine, separate from the PolyHaven PineTree001. Models/ and Textures/ keep relative glTF references. LOD1: 23,644 triangles. LOD2: 6,972 triangles. Export height ~22.74 m including roots. Bark has colour/normal/roughness textures; needles use a double-sided cutout atlas with threshold 0.39. Source exports contain no root display translation.

The test scene now places four LOD1 pines alongside twelve firs through WORLD/Vegetation/TreePreview. This is a visual trial; trunk collision, chopping, wind, custom distance switching to the separate LOD2 and persistence are not implemented. Existing import-generated mesh LODs are preserved. Appearance/performance require local acceptance.
