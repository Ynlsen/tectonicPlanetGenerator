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
    // Calculate rawstress based on relative plate velocities at the vertex and their distances

    float falloffRad = Mathf.DegToRad(TectonicSettings.falloffDeg);
    int plateCount = TectonicSettings.LargePlateCount + TectonicSettings.SmallPlateCount;

    // Get all plate points within a radius around the vertex and assign their plate a weight based on their distances 
    var weightByPlate = new float[plateCount];

    foreach (var platePoint in platePoints)
    {
      float angDis = Mathf.Acos(vertex.Dot(platePoint.Location));
      if (angDis <= falloffRad * 1.2f)
      {
        float w = Mathf.Clamp(1f - angDis / falloffRad, 0, 1);
        weightByPlate[platePoint.Id] += w * w;
      }
    }

    int counter = 0;
    foreach (var weight in weightByPlate)
    {
      if (weight != 0)
      {
        counter++;
      }
    }

    if (counter < 2)
    {
      return 0f;
    }

    // Calculate the centroids of the plates biased towards the vertex
    var centroids = new Vector3[plateCount];
    var velocities = new Vector3[plateCount];

    for (int i = 0; i < plateCount; i++)
    {
      if (weightByPlate[i] == 0)
      {
        continue;
      }

      Vector3 center = Vector3.Zero;
      foreach (var platePoint in platePoints)
      {
        if (platePoint.Id != i)
        {
          continue;
        }
        float d = vertex.DistanceTo(platePoint.Location);
        float w;
        if (d < 0.05f)
        {
          w = 1 / 0.05f;
        }
        else
        {
          w = 1 / d;
        }
        center += platePoint.Location * w;
      }
      centroids[i] = center.Normalized();

      var plate = plates.Find(pl => pl.Id == i); // TODO: BAD SOLUTION :(
      velocities[i] = plate.Speed * plate.MovementAxis.Cross(vertex);
    }

    // Compute the pairwise stress between the centroids based on their relative velocities
    // Blend them by their weight calculated in a previous step
    float stressSum = 0f;
    float weightSum = 0f;

    for (int i = 0; i < plateCount; i++)
    {
      for (int j = i + 1; j < plateCount; j++)
      {
        if (weightByPlate[i] == 0 || weightByPlate[j] == 0)
        {
          continue;
        }

        Vector3 direction = (centroids[i] - centroids[j]).Normalized();

        float stress = (velocities[j] - velocities[i]).Dot(direction);

        float w = weightByPlate[i] * weightByPlate[j];

        stressSum += stress * w;
        weightSum += w;
      }
    }

    if (weightSum == 0f)
    {
      return 0f;
    }

    float rawStress = stressSum / weightSum;

    // Calculate falloff based on distance to the distance of the vertex to the closest plate boundary

    int primaryPlateId = GetPlate(vertex);

    // Get the two closest non primary plate plate points
    float firstDistance = float.MaxValue;
    float secondDistance = float.MaxValue;

    PlatePoint firstForeignPoint = null;
    PlatePoint secondForeignPoint = null;
    foreach (var platePoint in platePoints)
    {
      if (platePoint.Id == primaryPlateId)
      {
        continue;
      }
      float d = vertex.DistanceSquaredTo(platePoint.Location);

      if (d < firstDistance)
      {
        secondDistance = firstDistance;
        secondForeignPoint = firstForeignPoint;

        firstDistance = d;
        firstForeignPoint = platePoint;
      }
      else if (d < secondDistance)
      {
        secondDistance = d;
        secondForeignPoint = platePoint;
      }
    }

    // For both, get the edge projection of the vertex
    Vector3 firstProj = CalculateEdgeProjection(firstForeignPoint, primaryPlateId, vertex);
    Vector3 secondProj = CalculateEdgeProjection(secondForeignPoint, primaryPlateId, vertex);

    // Take the closer projection to calculate the linear falloff
    float firstAngle = Mathf.Acos(vertex.Dot(firstProj));
    float secondAngle = Mathf.Acos(vertex.Dot(secondProj));

    float angle = Mathf.Min(firstAngle, secondAngle);

    float falloff = Mathf.Clamp((0.7f - (angle / (falloffRad * 0.8f))) / 0.7f, 0, 1);


    return rawStress * falloff * falloff;
  }

  private Vector3 CalculateEdgeProjection(PlatePoint foreignPoint, int primaryPlateId, Vector3 vertex)
  {
    //Get the 4 nearest primary plate points to the foreign point
    float firstDistance = float.MaxValue;
    float secondDistance = float.MaxValue;
    float thirdDistance = float.MaxValue;
    float fourthDistance = float.MaxValue;

    PlatePoint firstSelectedPoint = null;
    PlatePoint secondSelectedPoint = null;
    PlatePoint thirdSelectedPoint = null;
    PlatePoint fourthSelectedPoint = null;
    foreach (var platePoint in platePoints)
    {
      if (platePoint.Id != primaryPlateId)
      {
        continue;
      }
      float d = vertex.DistanceSquaredTo(platePoint.Location);

      if (d < firstDistance)
      {
        fourthDistance = thirdDistance;
        fourthSelectedPoint = thirdSelectedPoint;
        thirdDistance = secondDistance;
        thirdSelectedPoint = secondSelectedPoint;
        secondDistance = firstDistance;
        secondSelectedPoint = firstSelectedPoint;

        firstDistance = d;
        firstSelectedPoint = platePoint;
      }
      else if (d < secondDistance)
      {
        fourthDistance = thirdDistance;
        fourthSelectedPoint = thirdSelectedPoint;
        thirdDistance = secondDistance;
        thirdSelectedPoint = secondSelectedPoint;

        secondDistance = d;
        secondSelectedPoint = platePoint;
      }
      else if (d < thirdDistance)
      {
        fourthDistance = thirdDistance;
        fourthSelectedPoint = thirdSelectedPoint;

        thirdDistance = d;
        thirdSelectedPoint = platePoint;
      }
      else if (d < fourthDistance)
      {
        fourthDistance = d;
        fourthSelectedPoint = platePoint;
      }
    }

    // Select the best one by checking alignment with the foreign point and vertex
    PlatePoint selectedPoint = null;
    float bestWeight = float.MaxValue;

    float angFV = Mathf.Acos(foreignPoint.Location.Dot(vertex));

    if (firstSelectedPoint != null)
    {
      float angSV = Mathf.Acos(firstSelectedPoint.Location.Dot(vertex));
      float angSF = Mathf.Acos(firstSelectedPoint.Location.Dot(foreignPoint.Location));
      float w = Mathf.Min(angSV + 1.1f * angSF, angSV + 1.1f * angFV);
      if (w < bestWeight)
      {
        bestWeight = w;
        selectedPoint = firstSelectedPoint;
      }
    }
    if (secondSelectedPoint != null)
    {
      float angSV = Mathf.Acos(secondSelectedPoint.Location.Dot(vertex));
      float angSF = Mathf.Acos(secondSelectedPoint.Location.Dot(foreignPoint.Location));
      float w = Mathf.Min(angSV + 1.1f * angSF, angSV + 1.1f * angFV);
      if (w < bestWeight)
      {
        bestWeight = w;
        selectedPoint = secondSelectedPoint;
      }
    }
    if (thirdSelectedPoint != null)
    {
      float angSV = Mathf.Acos(thirdSelectedPoint.Location.Dot(vertex));
      float angSF = Mathf.Acos(thirdSelectedPoint.Location.Dot(foreignPoint.Location));
      float w = Mathf.Min(angSV + 1.1f * angSF, angSV + 1.1f * angFV);
      if (w < bestWeight)
      {
        bestWeight = w;
        selectedPoint = thirdSelectedPoint;
      }
    }
    if (fourthSelectedPoint != null)
    {
      float angSV = Mathf.Acos(fourthSelectedPoint.Location.Dot(vertex));
      float angSF = Mathf.Acos(fourthSelectedPoint.Location.Dot(foreignPoint.Location));
      float w = Mathf.Min(angSV + 1.1f * angSF, angSV + 1.1f * angFV);
      if (w < bestWeight)
      {
        bestWeight = w;
        selectedPoint = fourthSelectedPoint;
      }
    }

    // Compute the bisector of the foreign and chosen point. Then project the vertex onto the bisector
    Vector3 planeNormal = (selectedPoint.Location - foreignPoint.Location).Normalized();

    Vector3 proj = (vertex - planeNormal * planeNormal.Dot(vertex)).Normalized();

    // Find a plate point that is closer to the projection than the foreign or the selected point
    // When there are multiple, select the one that is more aligned
    float distProjF = proj.DistanceSquaredTo(foreignPoint.Location);
    float distProjS = proj.DistanceSquaredTo(selectedPoint.Location);

    PlatePoint alternativePoint = null;
    bestWeight = float.MaxValue;

    foreach (var platePoint in platePoints)
    {
      if (platePoint == foreignPoint || platePoint == selectedPoint)
      {
        continue;
      }

      float distProjA = proj.DistanceSquaredTo(platePoint.Location);
      if (distProjA > distProjF || distProjA > distProjS)
      {
        continue;
      }

      float angFA = Mathf.Acos(foreignPoint.Location.Dot(platePoint.Location));
      float angVA = Mathf.Acos(vertex.Dot(platePoint.Location));
      float w = angVA + 1.1f * angFA;

      if (w < bestWeight)
      {
        bestWeight = w;
        alternativePoint = platePoint;
      }
    }

    // If there is such a point, calculate the bisector with it and then take the intersection of our two bisectors as the projection.
    if (alternativePoint != null)
    {
      Vector3 alternativePlaneNormal;
      if (alternativePoint.Id == primaryPlateId)
      {
        alternativePlaneNormal = (alternativePoint.Location - foreignPoint.Location).Normalized();
      }
      else
      {
        alternativePlaneNormal = (selectedPoint.Location - alternativePoint.Location).Normalized();
      }

      Vector3 intersection = planeNormal.Cross(alternativePlaneNormal).Normalized();
      proj = (vertex.Dot(intersection) >= 0) ? intersection : -intersection;
    }

    return proj;
  }
}