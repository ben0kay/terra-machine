// Event-driven removal of batched ground foliage. Cut history is indexed in 8 m cells.
// Per-owner lifetime: no static cache, frame callbacks, terrain rays or collider polling.
using Godot;
using System.Collections.Generic;

public sealed class GroundFoliageEdits
{
    private readonly Dictionary<Vector2I, List<(Vector3 Center, float Radius)>> _cuts = new();
    private sealed class Batch
    {
        public Node3D Owner = null!;
        public MultiMesh Mesh = null!;
        public List<Vector3> Roots = new();
        public Aabb Bounds;
    }
    private readonly List<Batch> _batches = new();

    // =========================================================
    // Include a small root tolerance for existing grounding sink and voxel surface interpolation.
    public bool Contains(Vector3 root)
    {
        var key = new Vector2I(Mathf.FloorToInt(root.X / 8f), Mathf.FloorToInt(root.Z / 8f));
        if (!_cuts.TryGetValue(key, out var cuts)) return false;
        foreach (var cut in cuts)
            if (root.DistanceSquaredTo(cut.Center) <= cut.Radius * cut.Radius) return true;
        return false;
    }

    // =========================================================
    // Register immutable world roots aligned with instance indices; prune pending cut plants too.
    public void Register(Node3D owner, MultiMesh mesh, List<Vector3> roots)
    {
        if (roots.Count == 0) return;
        Aabb bounds = new(roots[0], Vector3.Zero);
        foreach (Vector3 root in roots) bounds = bounds.Expand(root);
        var batch = new Batch { Owner = owner, Mesh = mesh, Roots = roots, Bounds = bounds.Grow(0.15f) };
        _batches.Add(batch);
        Prune(batch);
    }

    // =========================================================
    // Streaming owners release batch references before freeing their scene nodes.
    public void Forget(Node3D owner) => _batches.RemoveAll(batch => batch.Owner == owner);

    // =========================================================
    // Store only local cut cells. A later tile rebuild cannot resurrect foliage in a hole.
    public void RemoveSphere(Vector3 center, float radius)
    {
        radius += 0.12f;
        int minX = Mathf.FloorToInt((center.X - radius) / 8f), maxX = Mathf.FloorToInt((center.X + radius) / 8f);
        int minZ = Mathf.FloorToInt((center.Z - radius) / 8f), maxZ = Mathf.FloorToInt((center.Z + radius) / 8f);
        for (int z = minZ; z <= maxZ; z++)
        for (int x = minX; x <= maxX; x++)
        {
            var key = new Vector2I(x, z);
            if (!_cuts.TryGetValue(key, out var cuts)) { cuts = new(); _cuts.Add(key, cuts); }
            cuts.Add((center, radius));
        }
        Aabb area = new(center - Vector3.One * radius, Vector3.One * radius * 2f);
        foreach (Batch batch in _batches)
            if (batch.Bounds.Intersects(area)) Prune(batch);
    }

    // =========================================================
    // Compact surviving instances in order, preserving LOD/distance-thinning prefix behaviour.
    private void Prune(Batch batch)
    {
        if (!GodotObject.IsInstanceValid(batch.Owner) || !GodotObject.IsInstanceValid(batch.Mesh)) return;
        var roots = batch.Roots;
        int removed = 0;
        foreach (Vector3 root in roots) if (Contains(root)) removed++;
        if (removed == 0) return;
        int oldCount = batch.Mesh.InstanceCount;
        float fraction = batch.Mesh.VisibleInstanceCount < 0 ? 1f : (float)batch.Mesh.VisibleInstanceCount / Mathf.Max(1, oldCount);
        var transforms = new List<Transform3D>(roots.Count - removed);
        var survivors = new List<Vector3>(roots.Count - removed);
        for (int i = 0; i < roots.Count; i++)
        {
            if (Contains(roots[i])) continue;
            survivors.Add(roots[i]); transforms.Add(batch.Mesh.GetInstanceTransform(i));
        }
        batch.Mesh.InstanceCount = transforms.Count;
        for (int i = 0; i < transforms.Count; i++) batch.Mesh.SetInstanceTransform(i, transforms[i]);
        batch.Mesh.VisibleInstanceCount = Mathf.CeilToInt(transforms.Count * fraction);
        batch.Roots = survivors;
    }
}
