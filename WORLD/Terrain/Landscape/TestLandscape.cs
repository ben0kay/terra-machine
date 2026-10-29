// One-time authored surface preparation. Native streaming consumes the resulting height image.
// This describes initial surface shape, not a mesh replacement or a full world generator.
using Godot;
using System;

public static class TestLandscape
{
    public const int MapSize = 1024;
    public const float HeightStart = -32f, HeightRange = 224f;

    // =========================================================
    // Bake at one metre per pixel; no per-frame or per-chunk managed callbacks remain.
    public static (Image Heights, Image SlopeMask) Build(LandscapeSettings settings)
    {
        var heights = new float[MapSize * MapSize];
        var mask = new byte[heights.Length];
        for (int z = 0; z < MapSize; z++)
        for (int x = 0; x < MapSize; x++)
        {
            // The native image sampler wraps negative world coordinates into the image.
            float wx = x < MapSize / 2 ? x : x - MapSize;
            float wz = z < MapSize / 2 ? z : z - MapSize;
            float height = Sample(wx, wz, settings);
            int index = z * MapSize + x;
            heights[index] = Mathf.Clamp((height - HeightStart) / HeightRange, 0f, 1f);

        }
        // Central differences use the same full-resolution surface everywhere, including chunk borders.
        for (int z = 0; z < MapSize; z++)
        for (int x = 0; x < MapSize; x++)
        {
            float dx = (heights[z * MapSize + (x + 1) % MapSize] - heights[z * MapSize + (x + MapSize - 1) % MapSize]) * HeightRange * 0.5f;
            float dz = (heights[((z + 1) % MapSize) * MapSize + x] - heights[((z + MapSize - 1) % MapSize) * MapSize + x]) * HeightRange * 0.5f;
            float up = 1f / MathF.Sqrt(1f + dx * dx + dz * dz);
            mask[z * MapSize + x] = (byte)(up * 255f);
        }
        var bytes = new byte[heights.Length * sizeof(float)];
        Buffer.BlockCopy(heights, 0, bytes, 0, bytes.Length);
        return (Image.CreateFromData(MapSize, MapSize, false, Image.Format.Rf, bytes),
            Image.CreateFromData(MapSize, MapSize, false, Image.Format.L8, mask));
    }

    // =========================================================
    // Long landforms first; fine variation is suppressed around the meadow.
    public static float Sample(float x, float z, LandscapeSettings s)
    {
        float meadow = 1f - Smooth(18f, 110f, MathF.Sqrt(x * x + z * z));
        float baseHeight = -4f + 9f * Bell(x, z, 95f, -110f, 125f, 165f)
            + 7f * Bell(x, z, -160f, -70f, 130f, 150f);
        float dx = (x - s.PlateauCenter.X) / MathF.Max(s.PlateauRadius.X, 1f);
        float dz = (z - s.PlateauCenter.Y) / MathF.Max(s.PlateauRadius.Y, 1f);
        // Uneven perimeter, while retaining a distinct steep western escarpment.
        float radius = MathF.Sqrt(dx * dx + dz * dz) + 0.035f * MathF.Sin(x / 23f + z / 31f);
        float edge = MathF.Max(s.CliffWidth, 1f) / MathF.Max(MathF.Min(s.PlateauRadius.X, s.PlateauRadius.Y), 1f);
        float plateau = 1f - Smooth(1f - edge, 1f + edge, radius);
        float height = Mathf.Lerp(baseHeight, s.PlateauHeight + 0.8f * MathF.Sin(x / 48f), plateau);
        // Broad overlapping shoulders form a walkable mountainside, not a cut or flattened track.
        float approach = s.ApproachHeight * Bell(x, z, s.ApproachCenter.X, s.ApproachCenter.Y,
            MathF.Max(s.ApproachRadius.X, 1f), MathF.Max(s.ApproachRadius.Y, 1f));
        approach += 8f * Bell(x, z, 5f, -130f, 75f, 100f);
        // Smooth maximum joins the shoulder into the plateau without a narrow blending trench.
        float blend = Mathf.Clamp(0.5f + 0.5f * (height - approach) / 12f, 0f, 1f);
        height = Mathf.Lerp(approach, height, blend) + 12f * blend * (1f - blend);
        height += s.MountainHeight * (Bell(x, z, -100f, -290f, 100f, 72f)
            + 0.78f * Bell(x, z, 85f, -315f, 72f, 90f));
        height += s.DetailHeight * MathF.Sin(x / 12.1f + MathF.Sin(z / 29f)) * MathF.Sin(z / 17.3f) * (1f - meadow);
        return Mathf.Lerp(height, 0.15f * MathF.Sin(x / 22f), meadow);
    }

    // =========================================================
    // Compact analytic helpers keep terrain composition independent of any addon API.
    private static float Bell(float x, float z, float cx, float cz, float rx, float rz)
        => MathF.Exp(-((x - cx) * (x - cx) / (rx * rx) + (z - cz) * (z - cz) / (rz * rz)));
    private static float Smooth(float a, float b, float x)
    {
        float t = Mathf.Clamp((x - a) / MathF.Max(b - a, 0.001f), 0f, 1f);
        return t * t * (3f - 2f * t);
    }
}
