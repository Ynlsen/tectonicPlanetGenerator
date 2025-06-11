using Godot;
using System.Collections.Generic;

public partial class TectonicSimulation : Node
{
  public TectonicSettings TectonicSettings;

  private class Plate
  {
    public int Id;

    public List<Vector3> SubPoints = new();
  }

  private class PlatePoint
  {
    public int Id;
    public Vector3 Location;
  }

  private List<Plate> plates;

  private List<PlatePoint> platePoints;

  private int idCounter;

  public void GeneratePlates()
  {
    plates = [];
    idCounter = 0;
    GD.Randomize();
    CreateDeviationPlates(TectonicSettings.LargePlateCount, TectonicSettings.LargeSubPoints, TectonicSettings.LargeDeviationAngle);
    CreateDeviationPlates(TectonicSettings.SmallPlateCount, TectonicSettings.SmallSubPoints, TectonicSettings.SmallDeviationAngle);

    platePoints = [];
    var points = FibonacciSphere(TectonicSettings.FibonacciPoints);

    foreach (var point in points)
    {
      var nextPoint = new PlatePoint();
      nextPoint.Location = point;
      nextPoint.Id = GetDeviationPlate(point);
      platePoints.Add(nextPoint);
    }
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

  // Found online, translated and adjusted.
  private Vector3[] FibonacciSphere(int count)
  {
    var points = new Vector3[count];
    float goldenAngle = Mathf.Tau / ((1f + Mathf.Sqrt(5f)) * 0.5f);
    for (int i = 0; i < count; i++)
    {
      float y = 1f - 2f * (i + 0.5f) / count;
      float radius = Mathf.Sqrt(1f - y * y);
      float theta = goldenAngle * i;
      float x = radius * Mathf.Cos(theta);
      float z = radius * Mathf.Sin(theta);
      points[i] = new Vector3(x, y, z);
    }
    return points;
  }

  private int GetDeviationPlate(Vector3 vertex)
  {
    int bestPlate = -1;
    float bestDistance = float.MaxValue;

    foreach (var plate in plates)
    {
      foreach (var subPoint in plate.SubPoints)
      {
        float distance = vertex.DistanceSquaredTo(subPoint);
        if (distance < bestDistance)
        {
          bestDistance = distance;
          bestPlate = plate.Id;
        }
      }
    }

    return bestPlate;
  }

  public int GetPlate(Vector3 vertex)
  {
    int bestPlate = -1;
    float bestDistance = float.MaxValue;

    foreach (var platePoint in platePoints)
    {
      float distance = vertex.DistanceSquaredTo(platePoint.Location);
      if (distance < bestDistance)
      {
        bestDistance = distance;
        bestPlate = platePoint.Id;
      }
    }

    return bestPlate;
  }
}