using Godot;

[Tool]
public partial class Planet : Node3D
{
	[Export] public PackedScene FaceScene;
	[Export] public int Resolution = 10;
	[Export] public float Radius = 1f;

	[Export] public ShapeSettings ShapeSettings; 

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

			AddChild(face);
		}
	}
}
