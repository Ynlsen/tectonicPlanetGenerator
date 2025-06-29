using Godot;

[Tool]
public partial class Planet : Node3D
{
	[Export] public PackedScene FaceScene;
	[Export] public int Resolution = 10;
	[Export] public float Radius = 1f;

	[Export] public ShapeSettings ShapeSettings; 

  [Export] public TectonicSettings TectonicSettings; 

	[ExportToolButton("Build planet")] public Callable BuildPlanetButton => Callable.From(BuildPlanet);

  private TectonicSimulation tectonicSimulation;

  public override void _Ready()
  {
    BuildPlanet();
  }

	private void BuildPlanet()
	{
    // Clears the previous faces
		foreach (Node child in GetChildren())
		{
			child.QueueFree();
		}

    tectonicSimulation = new TectonicSimulation();
    tectonicSimulation.TectonicSettings = TectonicSettings;

    tectonicSimulation.GeneratePlates();

    // Spawns the 6 faces of the sphere and assigns them a direction and more data
    var directions = new Vector3[]
		{
			Vector3.Up,
			Vector3.Down,
			Vector3.Left,
			Vector3.Right,
			Vector3.Forward,
			Vector3.Back,
		};

		foreach (var direction in directions)
		{
			var face = FaceScene.Instantiate<FaceMesh>();

			face.Resolution = Resolution;

			face.ShapeSettings  = ShapeSettings;			

			face.Scale = Vector3.One * Radius;

			face.FaceDirection = direction;

      face.tectonicSimulation = tectonicSimulation;

			AddChild(face);
		}
	}
}
