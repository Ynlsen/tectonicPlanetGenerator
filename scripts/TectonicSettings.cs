using Godot;

[Tool]
public partial class TectonicSettings : Resource
{
  [Export] public int LargePlateCount = 7;
  [Export] public int SmallPlateCount = 4;

  [Export] public int LargeSubPoints = 20;
  [Export] public float LargeJitterAngle = 20f;

  [Export] public int SmallSubPoints = 2;
  [Export] public float SmallJitterAngle = 5f;
}
