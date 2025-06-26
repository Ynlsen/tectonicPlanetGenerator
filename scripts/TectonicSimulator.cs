using Godot;
using System.Collections.Generic;

public partial class TectonicSimulation : Node
{
  public TectonicSettings TectonicSettings;

  private class Plate
  {
    public int Id;

    public List<Vector3> SubPoints = new();

    public Vector3 MovementAxis;

    public float Speed;
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

      nextPlate.MovementAxis = Vector3.Up.Rotated(Vector3.Right, Mathf.DegToRad((float)GD.RandRange(-180f, 180f))).Rotated(Vector3.Forward, Mathf.DegToRad((float)GD.RandRange(-180f, 180f)));

      nextPlate.Speed = (float)GD.RandRange(0f, 2f);

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
      var point = new Vector3(x, y, z);

      var angle = TectonicSettings.FibonacciDeviationAngle;
      Basis rotationBasis = Basis.Identity.Rotated(Vector3.Up, Mathf.DegToRad((float)GD.RandRange(-angle, angle))).Rotated(Vector3.Right, Mathf.DegToRad((float)GD.RandRange(-angle, angle)));

      points[i] = rotationBasis * point;
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
  public float GetStress(Vector3 vertex)
  {
    // Get all plate points within a radius around the vertex
    // Determine the top 4 most prominent plates among those points based on individual distance to the vertex
    // Calculate the centroids of those plates, biased toward the vertex
    // Compute the pairwise stress between the centroids based on their relative velocities
    // Blend them weighted on their prominence calculated in a previous step
    // Compute the falloff based on the distance of the vertex to the closest boundary
    //      Get the two closest non primary plate plate points
    //      For each, do the following
    //           Get the 4 nearest primary plate points to the selected foreign point
    //           Choose the best one by checking alignment with the foreign point and vertex
    //           Compute the bisector of the foreign and chosen point
    //           Project vertex on the bisector
    //           Find all points that are closer than the selected point or the foreign point to the projection
    //           If there is at least one of those points
    //                 Select the one that is most aligned with the vertex and foreign point
    //                 Calculate the bisector with this point as well
    //                 Calculate the intersection of these two bisector
    //                 Take the distance of the vertex to this intersection point
    //           Else, take the distance from the vertex to the projection
    //      Take the shorter distance
    //      Calculate falloff based on this distance
    // Return raw stress * falloff
  }
}