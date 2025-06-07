using Godot;

[Tool]
public partial class FaceMesh : MeshInstance3D
{
	[Export] public int Resolution = 10;

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

		var vertices = new Vector3[Resolution * Resolution];

		int counter = 0;

		for (int y = 0; y < Resolution; y++)
		{
			float v = y / (float)(Resolution - 1);

			for (int x = 0; x < Resolution; x++)
			{
				float u = x / (float)(Resolution - 1);

				var point = new Vector3((u - .5f) * 2f, 1, -(v - .5f) * 2f);

				vertices[counter++] = point.Normalized();
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

		_mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, combined);
		
		Mesh = _mesh;
	}
}
