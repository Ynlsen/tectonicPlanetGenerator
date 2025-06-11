using Godot;
using System;
using System.Collections.Generic;

[Tool]
public partial class TectonicSimulation : Node
{
  [Export] public TectonicSettings TectonicSettings;

  private class Plate
  {
    public int Id;

    public List<Vector3> SubPoints;
  }

  private List<Plate> plates;

  private int idCounter;

  public void GeneratePlates()
  {
    plates = [];
    idCounter = 0;
    GD.Randomize();
    CreateDeviationPlates(TectonicSettings.LargePlateCount, TectonicSettings.LargeSubPoints, TectonicSettings.LargeDeviationAngle);
    CreateDeviationPlates(TectonicSettings.SmallPlateCount, TectonicSettings.SmallSubPoints, TectonicSettings.SmallDeviationAngle);

    // Create equally distributed points around the sphere

    // Find the closest jitter point and save its plate ID to the point
    // Those points now define the plates on the planet
  }

  private void CreateDeviationPlates(int count, int subPoints, float deviationAngle)
  {
    for (int i = 0; i < count; i++)
    {
      var nextPlate = new Plate();
      nextPlate.Id = idCounter++;

      var origin = Vector3.Up.Rotated(Vector3.Right, Mathf.DegToRad((float)GD.RandRange(-180f, 180f))).Rotated(Vector3.Forward, Mathf.DegToRad((float)GD.RandRange(-180f, 180f)));

      for (int j = 0; j < subPoints; j++)
      {
        Basis rotationBasis = Basis.Identity.Rotated(Vector3.Up, Mathf.DegToRad((float)GD.RandRange(-deviationAngle, deviationAngle))).Rotated(Vector3.Right, Mathf.DegToRad((float)GD.RandRange(-deviationAngle, deviationAngle)));
        nextPlate.SubPoints.Add(rotationBasis * origin);
      }
      plates.Add(nextPlate);
    }
  }
  
  public int GetPlate(Vector3 vertex)
  {
    // Find the closest point on the sphere to the vertex
    // Return its plate ID
  }
}