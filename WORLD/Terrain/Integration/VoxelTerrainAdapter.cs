// The only C# owner of Voxel Tools API calls. Native generation/streaming run in the addon.
using Godot;

public partial class VoxelTerrainAdapter : Node3D, ITerrainService
{
    private Node3D? _viewer;
    private Node3D? _target;
    private int _viewDistance;
    private PhysicsRayQueryParameters3D _groundQuery = null!;
    public bool IsAvailable { get; private set; }
    public string Status { get; private set; } = "Waiting for terrain integration";

    // =========================================================
    // Validate the extension and instantiate its isolated scene only if available.
    public override void _Ready()
    {
        SetProcess(false);
        _groundQuery = new PhysicsRayQueryParameters3D { CollisionMask = 1, CollideWithAreas = false };
        if (!ClassDB.ClassExists("VoxelLodTerrain") || !ClassDB.ClassExists("VoxelViewer"))
        {
            Status = "Voxel Tools did not load. Check the Godot extension errors and matching binaries.";
            GD.PushError(Status);
            return;
        }
        var scene = GD.Load<PackedScene>("res://WORLD/Terrain/Integration/voxel_terrain.tscn");
        var terrain = scene.Instantiate<Node3D>();
        _viewDistance = terrain.Get("view_distance").AsInt32();
        AddChild(terrain);
        IsAvailable = true;
        Status = "Terrain streaming active";
    }

    // =========================================================
    // Attach a native viewer to an ordinary transform; the target needs no addon knowledge.
    public void BindViewer(Node3D target)
    {
        if (!IsAvailable) return;
        if (_viewer != null && GodotObject.IsInstanceValid(_viewer)) _viewer.Free();
        _target = target;
        _viewer = ClassDB.Instantiate("VoxelViewer").AsGodotObject() as Node3D;
        if (_viewer == null)
        {
            IsAvailable = false;
            Status = "VoxelViewer could not be instantiated";
            GD.PushError(Status);
            return;
        }
        _viewer.Name = "TerrainStreamingViewer";
        _viewer.Set("view_distance", _viewDistance);
        _viewer.Set("requires_collisions", true);
        _viewer.Set("requires_visuals", true);
        target.AddChild(_viewer);
    }

    // =========================================================
    // Confirm actual native physics collision, not just visible terrain meshes.
    public bool TryFindGround(Vector2 position, float top, float bottom, out Vector3 ground)
    {
        ground = Vector3.Zero;
        if (!IsAvailable || _target == null) return false;
        _groundQuery.From = new Vector3(position.X, top, position.Y);
        _groundQuery.To = new Vector3(position.X, bottom, position.Y);
        var hit = GetWorld3D().DirectSpaceState.IntersectRay(_groundQuery);
        if (hit.Count == 0) return false;
        ground = hit["position"].AsVector3();
        return true;
    }

    // =========================================================
    // A viewer parented under an external target is still owned by this integration.
    public override void _ExitTree()
    {
        if (_viewer != null && GodotObject.IsInstanceValid(_viewer)) _viewer.QueueFree();
        _viewer = null;
        _target = null;
    }
}
