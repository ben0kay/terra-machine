// Defaults for the optional rendering comparison overlay; no per-frame configuration polling.
using Godot;

[GlobalClass]
public partial class DebugSettings : Resource
{
    [Export] public bool Enabled { get; set; } = true;
    [Export(PropertyHint.Range, "0.5,10,0.5")] public float WarmupSeconds { get; set; } = 2f;
    [Export(PropertyHint.Range, "1,15,0.5")] public float SampleSeconds { get; set; } = 3f;
    [Export(PropertyHint.Range, "0.1,2,0.05")] public float ReadoutInterval { get; set; } = 0.25f;
}
