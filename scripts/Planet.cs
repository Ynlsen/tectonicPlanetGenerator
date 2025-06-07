using Godot;

[Tool]
public partial class Planet : Node3D
{
	[Export] public PackedScene FaceScene;
	[Export] public int Resolution = 10;
	[Export] public float Radius = 1f;

    [ExportToolButton("Build planet")] public Callable BuildPlanetButton => Callable.From(BuildPlanet);

	public override void _Ready()
	{
		BuildPlanet();
	}

	private void BuildPlanet()
	{
		foreach (Node child in GetChildren())
		{
			child.QueueFree();
		}

		var rotations = new Vector3[]
		{
			new Vector3(  0,   0,   0),
			new Vector3(180,   0,   0), 
			new Vector3(  0,   0,  90), 
			new Vector3(  0,   0, -90),
			new Vector3(-90,   0,   0),
			new Vector3( 90,   0,   0)
		};

		foreach (var rotation in rotations)
		{
			var face = FaceScene.Instantiate<FaceMesh>();

			face.Resolution = Resolution;

			face.Scale = Vector3.One * Radius;

			face.RotationDegrees = rotation;

			AddChild(face);
		}
	}
}
