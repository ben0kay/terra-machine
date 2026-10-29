// The only C# owner of Voxel Tools API calls. Native generation/streaming run in the addon.
using Godot;

public partial class VoxelTerrainAdapter : Node3D, ITerrainService, ITerrainRenderDebug, ITerrainSurfaceService
{
    [Export] public LandscapeSettings Landscape { get; set; } = new();
    private Node3D? _viewer;
    private Node3D? _terrainNode;
    private ShaderMaterial? _texturedMaterial;
    private ShaderMaterial? _simpleMaterial;
    private ShaderMaterial? _normalMapsOffMaterial;
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
        if (!ClassDB.ClassExists("VoxelLodTerrain") || !ClassDB.ClassExists("VoxelViewer") || !ClassDB.ClassExists("VoxelGeneratorImage"))
        {
            Status = "Voxel Tools did not load. Check the Godot extension errors and matching binaries.";
            GD.PushError(Status);
            return;
        }
        var scene = GD.Load<PackedScene>("res://WORLD/Terrain/Integration/voxel_terrain.tscn");
        var terrain = scene.Instantiate<Node3D>();
        // Prepare once before adding the native node: worker threads see immutable image data.
        var surface = TestLandscape.Build(Landscape);
        Resource generator = ((Resource)terrain.Get("generator").AsGodotObject()).Duplicate();
        generator.Set("image", surface.Heights);
        terrain.Set("generator", generator);
        _terrainNode = terrain;
        _texturedMaterial = (ShaderMaterial)((ShaderMaterial)terrain.Get("material").AsGodotObject()).Duplicate();
        _texturedMaterial.SetShaderParameter("surface_slope", ImageTexture.CreateFromImage(surface.SlopeMask));
        _texturedMaterial.SetShaderParameter("surface_map_size", (float)TestLandscape.MapSize);
        _normalMapsOffMaterial = (ShaderMaterial)_texturedMaterial.Duplicate();
        _normalMapsOffMaterial.SetShaderParameter("normal_maps_enabled", false);
        _simpleMaterial = new ShaderMaterial { Shader = GD.Load<Shader>("res://WORLD/Terrain/Integration/ground_simple.gdshader") };
        terrain.Set("material", _texturedMaterial);
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
        return TryFindSurface(position, top, bottom, out ground, out _);
    }

    // =========================================================
    // One shared physics ray returns the actual surface normal for grass slope filtering.
    public bool TryFindSurface(Vector2 position, float top, float bottom, out Vector3 ground, out Vector3 normal)
    {
        ground = Vector3.Zero; normal = Vector3.Up;
        if (!IsAvailable || _target == null) return false;
        _groundQuery.From = new Vector3(position.X, top, position.Y);
        _groundQuery.To = new Vector3(position.X, bottom, position.Y);
        var hit = GetWorld3D().DirectSpaceState.IntersectRay(_groundQuery);
        if (hit.Count == 0) return false;
        ground = hit["position"].AsVector3();
        normal = hit["normal"].AsVector3();
        return true;
    }

    // =========================================================
    // Apply only on UI changes. A private material copy protects the saved master resource.
    public void SetDebugRendering(bool visible, bool textured, bool normalMaps)
    {
        if (_terrainNode == null || _texturedMaterial == null || _simpleMaterial == null || _normalMapsOffMaterial == null) return;
        _terrainNode.Visible = visible;
        ShaderMaterial selected = textured ? (normalMaps ? _texturedMaterial : _normalMapsOffMaterial) : _simpleMaterial;
        // Swap cached resources so native chunk material copies refresh on every mode change.
        _terrainNode.Set("material", selected);
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
