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
    public static (Image Heights, Image RouteMask) Build(LandscapeSettings settings)
    {
        Vector3[] route = BakeRoute(settings.Route);
        var heights = new float[MapSize * MapSize];
        var mask = new byte[heights.Length];
        for (int z = 0; z < MapSize; z++)
        for (int x = 0; x < MapSize; x++)
        {
            // The native image sampler wraps negative world coordinates into the image.
            float wx = x < MapSize / 2 ? x : x - MapSize;
            float wz = z < MapSize / 2 ? z : z - MapSize;
            float height = Sample(wx, wz, settings, route, out float path);
            int index = z * MapSize + x;
            heights[index] = Mathf.Clamp((height - HeightStart) / HeightRange, 0f, 1f);
            mask[index] = (byte)(Mathf.Clamp(path, 0f, 1f) * 255f);
        }
        var bytes = new byte[heights.Length * sizeof(float)];
        Buffer.BlockCopy(heights, 0, bytes, 0, bytes.Length);
        return (Image.CreateFromData(MapSize, MapSize, false, Image.Format.Rf, bytes),
            Image.CreateFromData(MapSize, MapSize, false, Image.Format.L8, mask));
    }

    // =========================================================
    // Long landforms first; fine variation is suppressed around the meadow and walking bench.
    public static float Sample(float x, float z, LandscapeSettings s, Vector3[] route, out float path)
    {
        float meadow = 1f - Smooth(25f, 75f, MathF.Sqrt(x * x + z * z));
        float baseHeight = -4f + 9f * Bell(x, z, 95f, -110f, 125f, 165f)
            + 7f * Bell(x, z, -160f, -70f, 130f, 150f);
        float dx = (x - s.PlateauCenter.X) / MathF.Max(s.PlateauRadius.X, 1f);
        float dz = (z - s.PlateauCenter.Y) / MathF.Max(s.PlateauRadius.Y, 1f);
        float radius = MathF.Sqrt(dx * dx + dz * dz);
        float edge = MathF.Max(s.CliffWidth, 1f) / MathF.Max(MathF.Min(s.PlateauRadius.X, s.PlateauRadius.Y), 1f);
        float plateau = 1f - Smooth(1f - edge, 1f + edge, radius);
        float height = Mathf.Lerp(baseHeight, s.PlateauHeight + 0.8f * MathF.Sin(x / 48f), plateau);
        // A separate distant ridge provides a backdrop, rather than dozens of small hillocks.
        float mountain = s.MountainHeight * (Bell(x, z, -100f, -290f, 100f, 72f)
            + 0.78f * Bell(x, z, 85f, -315f, 72f, 90f));
        height += mountain;
        height += s.DetailHeight * MathF.Sin(x / 8.1f + MathF.Sin(z / 19f)) * MathF.Sin(z / 11.3f) * (1f - meadow);
        height = Mathf.Lerp(height, 0.15f * MathF.Sin(x / 22f), meadow);
        float best = float.MaxValue, routeHeight = 0f;
        for (int i = 0; i + 1 < route.Length; i++)
        {
            Vector3 a = route[i], b = route[i + 1];
            float vx = b.X - a.X, vz = b.Z - a.Z;
            float t = Mathf.Clamp(((x - a.X) * vx + (z - a.Z) * vz) / MathF.Max(vx * vx + vz * vz, 0.001f), 0f, 1f);
            float px = x - a.X - vx * t, pz = z - a.Z - vz * t;
            float distance = px * px + pz * pz;
            if (distance >= best) continue;
            best = distance;
            routeHeight = Mathf.Lerp(a.Y, b.Y, t);
        }
        float d = MathF.Sqrt(best);
        float width = MathF.Max(s.PathHalfWidth, 1f);
        float shoulder = MathF.Max(s.PathShoulderWidth, 1f);
        float bench = 1f - Smooth(width, width + shoulder, d);
        height = Mathf.Lerp(height, routeHeight + 0.025f * MathF.Sin(z / 5f), bench);
        path = 1f - Smooth(width * 0.55f, width + 1.5f, d);
        return height;
    }

    // =========================================================
    // Smooth authored control points once; sampled segments are shared by height and soil mask.
    public static Vector3[] BakeRoute(Vector3[] points)
    {
        if (points.Length < 2) return Array.Empty<Vector3>();
        var result = new Vector3[(points.Length - 1) * 8 + 1];
        for (int i = 0; i + 1 < points.Length; i++)
        for (int j = 0; j < 8; j++)
            result[i * 8 + j] = points[i].CubicInterpolate(points[i + 1], points[Math.Max(i - 1, 0)], points[Math.Min(i + 2, points.Length - 1)], j / 8f);
        result[^1] = points[^1];
        return result;
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
