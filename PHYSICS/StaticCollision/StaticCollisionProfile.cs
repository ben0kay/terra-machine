// Asset-agnostic settings for opt-in static collision. Never attach this to moving actors.
using Godot;

public enum StaticCollisionMode { None = 0, AuthoredShape = 1, ConvexMesh = 2 }

[GlobalClass]
public partial class StaticCollisionProfile : Resource
{
    [ExportGroup("Shape")]
    [Export] public StaticCollisionMode Mode { get; set; } = StaticCollisionMode.None;
    [Export] public Shape3D? Shape { get; set; }
    [Export] public Transform3D ShapeTransform { get; set; } = Transform3D.Identity;
    [Export] public bool SimplifyConvexHull { get; set; }
    [ExportGroup("Filtering")]
    [Export(PropertyHint.Layers3DPhysics)] public uint CollisionLayer { get; set; } = 4;
    [Export(PropertyHint.Layers3DPhysics)] public uint CollisionMask { get; set; } = 0;
}
