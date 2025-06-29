using Godot;

[Tool]
public partial class TectonicSettings : Resource
{
  [Export] public int LargePlateCount = 7;
  [Export] public int SmallPlateCount = 4;

  [Export] public int LargeSubPoints = 20;
  [Export] public float LargeDeviationAngle = 20f;

  [Export] public int SmallSubPoints = 2;
  [Export] public float SmallDeviationAngle = 5f;

  [Export] public int FibonacciPoints = 1000;
  [Export] public float FibonacciDeviationAngle = 2f;

  [Export] public float falloffDeg = 10f;
}
