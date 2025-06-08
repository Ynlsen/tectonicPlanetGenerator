using Godot;
using System;

[Tool]
public partial class ShapeSettings : Resource
{
	[Export] public int Seed = 42;
	[Export] public float NoiseScale = 1f;      // how “zoomed in” the noise is
	[Export] public float MinHeight = 0f;       // displacement at noise = 0
	[Export] public float MaxHeight = 0.2f;     // displacement at noise = 1
}
