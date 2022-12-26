using OpenTK.Mathematics;
using MH = OpenTK.Mathematics.MathHelper;

namespace TemplateProject;

// ReSharper disable once InconsistentNaming
public class PUMA
{
    private readonly Cylinder _l1 = new Cylinder();
    private readonly Cylinder _q2 = new Cylinder();
    private readonly Cylinder _l3 = new Cylinder();
    private readonly Cylinder _l4 = new Cylinder();


    public float L1
    {
        get => _l1.h;
        set => _l1.h = value;
    }
    
    public float Q2
    {
        get => _q2.h;
        set => _q2.h = value;
    }
    
    public float L3
    {
        get => _l3.h;
        set => _l3.h = value;
    }
    
    public float L4
    {
        get => _l4.h;
        set => _l4.h = value;
    }

    private float _r = 1;

    public float R
    {
        get => _r;
        set
        {
            _r = value;
            _l1.r = _r;
            _q2.r = _r;
            _l3.r = _r;
            _l4.r = _r;
        }
    }

    private float _alpha1 = 0;

    public float Alpha1
    {
        get => _alpha1;
        set
        {
            _alpha1 = value;

            var a1 = MH.DegreesToRadians(_alpha1);
            _a1Matrix = Matrix4.CreateRotationZ(a1); 
        }
    }
    
    Matrix4 _a1Matrix = Matrix4.Identity;
    
    public void Render(Shader shader, Matrix4 cameraMatrix)
    {
        shader.LoadMatrix4("mvp", _a1Matrix * cameraMatrix);
        _l1.Render();
    }
}