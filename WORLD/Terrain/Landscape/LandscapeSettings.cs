// Authored test-landscape dimensions and broad approach. Contains no voxel-addon types.
using Godot;

[GlobalClass]
public partial class LandscapeSettings : Resource
{
    [ExportGroup("Landforms")]
    [Export] public float PlateauHeight { get; set; } = 34f;
    [Export] public Vector2 PlateauCenter { get; set; } = new(-50f, -155f);
    [Export] public Vector2 PlateauRadius { get; set; } = new(80f, 58f);
    [Export] public float CliffWidth { get; set; } = 7f;
    [Export] public float MountainHeight { get; set; } = 100f;
    [Export] public float DetailHeight { get; set; } = 0.65f;
    [ExportGroup("Broad mountain approach")]
    [Export] public Vector2 ApproachCenter { get; set; } = new(40f, -85f);
    [Export] public Vector2 ApproachRadius { get; set; } = new(100f, 115f);
    [Export] public float ApproachHeight { get; set; } = 32f;
}
