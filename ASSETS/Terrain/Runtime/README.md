# Runtime terrain texture conversions

PNG copies of the normal and roughness maps used by the first ground visual pass. Existing color JPGs are referenced directly without duplication. Source sets: forest_ground_06, grass_ground, gray_rocks from the existing Poly Haven downloads.

Original EXRs remain in their download folders. Godot 4.4.1 rejected their EXR compression during a headless import check; these PNG copies resolve that specific failure. Converted with OpenEXR: preserve linear data, clamp the small out-of-range normal components to 0–1, quantize to 8-bit; normal RGB and roughness single-channel. No sRGB conversion, exposure, tone mapping or image generation. All six are 1024 × 1024. Clamping/quantization is a deliberate runtime format tradeoff; retain EXRs for future higher-precision conversion.

Keep the PNG.import recipes: mipmaps enabled, VRAM compression; normal-map compression enabled for Normal.png. These images are numerical data, so shader samplers do not use source_color. The terrain ShaderMaterial is WORLD/Terrain/Configuration/GroundMaterial.tres. Do not delete the surrounding source texture folders as if they were Godot-extracted GLB duplicates: these terrain files have a different workflow.
