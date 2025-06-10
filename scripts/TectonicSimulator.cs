using Godot;
using System;

[Tool]
public partial class TectonicSimulation : Node
{
  [Export] public TectonicSettings TectonicSettings;

  public void GeneratePlates()
  {
    // Generate large and small plates

    // Get a random point on the sphere per plate as the origin
    // Generate jitter points randomly around the origin


    // Create equally distributed points around the sphere

    // Find the closest jitter point and save its plate ID to the point
    // Those points now define the plates on the planet
  }

  public int GetPlate(Vector3 vertex)
  {
      // Find the closest point on the sphere to the vertex
      // Return its plate ID
  }
}