using Godot;
using System;

[Tool]
public partial class ShapeSettings : Resource
{
  [Export] public int Seed = 69;
  [Export] public float NoiseScale = 1f;
  [Export] public float MinHeight = 0f;
  [Export] public float MaxHeight = 0.2f;
}
