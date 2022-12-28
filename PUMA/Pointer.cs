using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using MH = OpenTK.Mathematics.MathHelper;

namespace TemplateProject;

public class Pointer
{
    private Mesh _mesh;

    private Matrix4 _modelMatrix = Matrix4.Identity;
    
    private Vector3 _pos;
    public Vector3 Pos
    {
        get => _pos;
        set
        {
            _pos = value;
            RecalculateModelMatrix();
        }
    }

    private Vector3 _rot;
    public Vector3 Rot
    {
        get => (MH.RadiansToDegrees(_rot.X), MH.RadiansToDegrees(_rot.Y), MH.RadiansToDegrees(_rot.Z));
        set
        {
            _rot = (MH.DegreesToRadians(value.X),MH.DegreesToRadians(value.Y), MH.DegreesToRadians(value.Z));
            RecalculateModelMatrix();
        }
    }

    public Pointer()
    {
        RecalculateMesh();
        RecalculateModelMatrix();
    }
    private void RecalculateMesh()
    {
        Vector3 x = (1, 0, 0);
        Vector3 y = (0, 1, 0);
        Vector3 z = (0, 0, 1);

        var indices = new int[]
        {
            0, 1,
            0, 2,
            0, 3
        };

        var vertices = new float[]
        {
            0, 0, 0,
            1, 0, 0,
            0, 1, 0,
            0, 0, 1
        };

        _mesh = new Mesh(PrimitiveType.Lines, indices, (vertices, 0, 3));
    }
    
    private void RecalculateModelMatrix()
    {
        _modelMatrix = Matrix4.CreateRotationX(_rot.X);
        _modelMatrix *= Matrix4.CreateRotationY(_rot.Y);
        _modelMatrix *= Matrix4.CreateRotationZ(_rot.Z);
        _modelMatrix *= Matrix4.CreateTranslation(_pos);
    }

    public void Render(Shader shader, Matrix4 projectionViewMatrix)
    {
        shader.LoadMatrix4("mvp", _modelMatrix * projectionViewMatrix);
        _mesh.Render();
    }
}