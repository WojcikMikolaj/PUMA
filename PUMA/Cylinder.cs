using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using MH = OpenTK.Mathematics.MathHelper;

namespace TemplateProject;

public class Cylinder
{
    public Cylinder(float h = 2, int sectionsCount = 10)
    {
        this._h = h;
        this._sectionsCount = sectionsCount;
        ConstructCylinder();
    }

    private Mesh _mesh;


    private int _sectionsCount;

    public int SectionsCount
    {
        get => _sectionsCount;
        set
        {
            _sectionsCount = value;
            ConstructCylinder();
        }
    }

    private float _h;

    // ReSharper disable once InconsistentNaming
    public float h
    {
        get => _h;
        set
        {
            _h = value;
            ConstructCylinder();
        }
    }

    private float _r = 1;
    // ReSharper disable once InconsistentNaming
    public float r
    {
        get => _r;
        set
        {
            _r = value;
            ConstructCylinder();
        }
    }

    private void ConstructCylinder()
    {
        List<(Vector3 vert, Vector2 tex)> podstawa = new(_sectionsCount + 1);
        List<(Vector3 vert, Vector2 tex)> sufit = new(_sectionsCount + 1);
        for (int i = 0; i <= _sectionsCount; i++)
        {
            var u = (float) i / _sectionsCount;
            var alpha = u * MH.TwoPi;

            podstawa.Add((
                new Vector3(_r * (float) MH.Cos(alpha), _r * (float) MH.Sin(alpha), 0),
                new Vector2(u, 0)
            ));
            sufit.Add((
                new Vector3(_r * (float) MH.Cos(alpha), _r * (float) MH.Sin(alpha), _h),
                new Vector2(u, 1)
            ));
        }

        List<int> indices = new List<int>();
        for (int i = 0; i < _sectionsCount; i++)
        {
            indices.Add(i);
            indices.Add(i + 1);
            indices.Add(i + 1 + _sectionsCount + 1);

            indices.Add(i);
            indices.Add(i + _sectionsCount + 1);
            indices.Add(i + 1 + _sectionsCount + 1);
        }

        //dolne denko
        for (int i = 0; i < _sectionsCount; i++)
        {
            indices.Add(i);
            indices.Add(i + 1);
            indices.Add(_sectionsCount * 2 + 2);
        }

        //górne denko
        for (int i = 0; i < _sectionsCount; i++)
        {
            indices.Add(i + _sectionsCount + 1);
            indices.Add(i + 1 + _sectionsCount + 1);
            indices.Add(_sectionsCount * 2 + 3);
        }

        var verticesVector3s = podstawa.ConvertAll(input => input.vert).Concat(sufit.ConvertAll(input => input.vert))
            .ToArray();
        var texCoordsVector2s = podstawa.ConvertAll(input => input.tex).Concat(sufit.ConvertAll(input => input.tex))
            .ToArray();

        List<float> vertices = new List<float>();
        foreach (var verticesVector3 in verticesVector3s)
        {
            vertices.Add(verticesVector3.X);
            vertices.Add(verticesVector3.Y);
            vertices.Add(verticesVector3.Z);
        }

        //dolny środek
        vertices.Add(0);
        vertices.Add(0);
        vertices.Add(0);

        //górny środek
        vertices.Add(0);
        vertices.Add(0);
        vertices.Add(_h);

        List<float> texCoords = new List<float>();
        foreach (var texCoordsVector2 in texCoordsVector2s)
        {
            texCoords.Add(texCoordsVector2.X);
            texCoords.Add(texCoordsVector2.Y);
        }

        //dolny środek
        texCoords.Add(0);
        texCoords.Add(0);

        //górny środek
        texCoords.Add(0);
        texCoords.Add(1);

        _mesh = new Mesh(
            PrimitiveType.Triangles,
            indices.ToArray(),
            (vertices.ToArray(), 0, 3),
            (texCoords.ToArray(), 1, 2)
        );
    }

    public void Render()
    {
        _mesh.Render();
    }
}