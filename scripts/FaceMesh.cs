using Godot;

[Tool]
public partial class FaceMesh : MeshInstance3D
{
	[Export] public int Resolution = 10;

	[Export] public ShapeSettings ShapeSettings;

	[ExportToolButton("Build mesh")] public Callable BuildMeshButton => Callable.From(BuildMesh);

	private ArrayMesh _mesh =  new ArrayMesh();

	public override void _Ready()
	{
		_mesh = new ArrayMesh();
		BuildMesh();
	}

	private void BuildMesh()
	{
		_mesh.ClearSurfaces();

		var noise = new FastNoiseLite
		{
			Seed           = ShapeSettings.Seed,
			NoiseType      = FastNoiseLite.NoiseTypeEnum.Simplex,
			Frequency      = ShapeSettings.NoiseScale,
			FractalOctaves = 1,
			FractalLacunarity = 2.0f,
			FractalGain       = 0.5f
		};

		var vertices = new Vector3[Resolution * Resolution];

		int counter = 0;

		for (int y = 0; y < Resolution; y++)
		{
			float v = y / (float)(Resolution - 1);

			for (int x = 0; x < Resolution; x++)
			{
				float u = x / (float)(Resolution - 1);

				var raw = new Vector3((u - .5f) * 2f, 1, -(v - .5f) * 2f);

				var spherical = raw.Normalized();

				float n = noise.GetNoise3D(spherical.X,spherical.Y,spherical.Z);

				float elevation = Mathf.Lerp(ShapeSettings.MinHeight, ShapeSettings.MaxHeight, n);

				vertices[counter++] = spherical * (1 + elevation);
			}
		}

		var indices = new int[(Resolution - 1) * (Resolution - 1) * 6];

		counter = 0;

		for (int y = 0; y < Resolution - 1; y++)
		{
			for (int x = 0; x < Resolution - 1; x++)
			{
				int i = x + y * Resolution;

				indices[counter++] = i;
				indices[counter++] = i + Resolution;
				indices[counter++] = i + Resolution + 1;
				indices[counter++] = i;
				indices[counter++] = i + Resolution + 1;
				indices[counter++] = i + 1;
			}
		}

		var combined = new Godot.Collections.Array();

		//All the Mesh. are just numbers. Ill leave them in for clarity only
		combined.Resize((int)Mesh.ArrayType.Max);
		combined[(int)Mesh.ArrayType.Vertex] = vertices;
		combined[(int)Mesh.ArrayType.Index] = indices;

		combined[(int)Mesh.ArrayType.Normal] = vertices; // This is a very temporary solution. On a perfect sphere they are the same!!

		_mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, combined);
		
		Mesh = _mesh;
	}
}
