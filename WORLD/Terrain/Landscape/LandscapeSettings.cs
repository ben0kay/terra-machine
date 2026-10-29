// Authored test-landscape dimensions and route. Contains no voxel-addon types.
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
    [ExportGroup("Walking route")]
    [Export] public float PathHalfWidth { get; set; } = 3.5f;
    [Export] public float PathShoulderWidth { get; set; } = 8f;
    // X/Z are map coordinates; Y is the desired walking elevation, in metres.
    [Export] public Vector3[] Route { get; set; } = {
        new(0, 0, 0), new(20, 2, -30), new(50, 7, -55), new(60, 14, -85),
        new(35, 22, -110), new(-5, 29, -125), new(-42, 34, -150) };
}
