using Godot;

[Tool]
public partial class TectonicSettings : Resource
{
  [Export] public int LargePlateCount = 7;
  [Export] public int SmallPlateCount = 5;

  [Export] public int LargeSubPoints = 10;
  [Export] public float LargeJitterAngle = 20f;

  [Export] public int SmallSubPoints = 3;
  [Export] public float SmallJitterAngle = 5f;
}
