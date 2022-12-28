using OpenTK.Mathematics;
using MH = OpenTK.Mathematics.MathHelper;

namespace TemplateProject;

// ReSharper disable once InconsistentNaming
public class PUMA
{
    private readonly Cylinder _l1;
    private readonly Cylinder _q2;
    private readonly Cylinder _l3;
    private readonly Cylinder _l4;
    private readonly Cylinder _l5;


    public PUMA()
    {
        _l1 = new Cylinder(Direction.Z)
        {
            h = 3
        };
        _q2 = new Cylinder(Direction.X);
        _l3 = new Cylinder(Direction.NZ)
        {
            h = 1
        };
        _l4 = new Cylinder(Direction.X)
        {
            h = 1
        };
        R = 0.1f;
        
        _l5 = new Cylinder(Direction.X);
        _l5.r = 0.05f;
        _l5.h = 0.1f;

        RecalculateMatrices();
    }

    private void RecalculateMatrices()
    {
        _l1Matrix = Matrix4.CreateTranslation(0, 0, _l1.h);
        _q2Matrix = Matrix4.CreateTranslation(_q2.h, 0, 0);
        _l3Matrix = Matrix4.CreateTranslation(0, 0, -_l3.h);
        _l4Matrix = Matrix4.CreateTranslation(_l4.h, 0, 0);
        
        var a1 = MH.DegreesToRadians(_alpha1);
        _a1Matrix = Matrix4.CreateRotationZ(a1);
        
        var a2 = MH.DegreesToRadians(_alpha2);
        _a2Matrix = Matrix4.CreateRotationY(a2);
        
        var a3 = MH.DegreesToRadians(_alpha3);
        _a3Matrix = Matrix4.CreateRotationY(a3);
        
        var a4 = MH.DegreesToRadians(_alpha4);
        _a4Matrix = Matrix4.CreateRotationZ(a4);
        
        var a5 = MH.DegreesToRadians(_alpha5);
        _a5Matrix = Matrix4.CreateRotationX(a5);

        _f01 = _a1Matrix;
        _f02 = _a2Matrix * _l1Matrix * _f01;
        _f03 = _a3Matrix * _q2Matrix * _f02;
        _f04 = _a4Matrix * _l3Matrix * _f03;
        _f05 = _a5Matrix * _l4Matrix * _f04;
    }
    
    Matrix4 _f01 = Matrix4.Identity;
    Matrix4 _f02 = Matrix4.Identity;
    Matrix4 _f03 = Matrix4.Identity;
    Matrix4 _f04 = Matrix4.Identity;
    Matrix4 _f05 = Matrix4.Identity;

    public float L1
    {
        get => _l1.h;
        set
        {
            _l1.h = value;
            RecalculateMatrices();
        }
    }

    private Matrix4 _l1Matrix = Matrix4.Identity;

    public float Q2
    {
        get => _q2.h;
        set
        {
            _q2.h = value;
            RecalculateMatrices();
        }
    }

    private Matrix4 _q2Matrix = Matrix4.Identity;

    public float L3
    {
        get => _l3.h;
        set
        {
            _l3.h = value;
            RecalculateMatrices();
        }
    }

    private Matrix4 _l3Matrix = Matrix4.Identity;

    public float L4
    {
        get => _l4.h;
        set
        {
            _l4.h = value;
            RecalculateMatrices();
        }
    }

    private Matrix4 _l4Matrix = Matrix4.Identity;

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
            RecalculateMatrices();
        }
    }

    Matrix4 _a1Matrix = Matrix4.Identity;

    private float _alpha2 = 0;

    public float Alpha2
    {
        get => _alpha2;
        set
        {
            _alpha2 = value;
            RecalculateMatrices();
        }
    }

    Matrix4 _a2Matrix = Matrix4.Identity;

    private float _alpha3 = 0;

    public float Alpha3
    {
        get => _alpha3;
        set
        {
            _alpha3 = value;
            RecalculateMatrices();
        }
    }

    Matrix4 _a3Matrix = Matrix4.Identity;

    private float _alpha4 = 0;

    public float Alpha4
    {
        get => _alpha4;
        set
        {
            _alpha4 = value;
            RecalculateMatrices();
        }
    }

    Matrix4 _a4Matrix = Matrix4.Identity;

    private float _alpha5 = 0;

    public float Alpha5
    {
        get => _alpha5;
        set
        {
            _alpha5 = value;
            RecalculateMatrices();
        }
    }

    Matrix4 _a5Matrix = Matrix4.Identity;

    public void Render(Shader shader, Matrix4 projectionViewMatrix)
    {
        shader.LoadMatrix4("mvp", _f01 * projectionViewMatrix);
        _l1.Render();
        shader.LoadMatrix4("mvp", _f02 * projectionViewMatrix);
        _q2.Render();
        shader.LoadMatrix4("mvp", _f03 * projectionViewMatrix);
        _l3.Render();
        shader.LoadMatrix4("mvp", _f04 * projectionViewMatrix);
        _l4.Render();
        shader.LoadMatrix4("mvp", _f05 * projectionViewMatrix);
        _l5.Render();
    }

    public bool MoveToPoint(Vector3 pos, Vector3 rotInDeg)
    {
        var newConf = IKPUMASolver.SolveInverse(pos, (MH.DegreesToRadians(rotInDeg.X), MH.DegreesToRadians(rotInDeg.Y), MH.DegreesToRadians(rotInDeg.Z)), new PUMASettings(_l1.h, _l3.h, _l4.h));
        if (!newConf.IsNaNOrInf())
        {
            var newConfInDeg = newConf.InDegrees();
            _alpha1 = newConfInDeg.a1;
            _q2.h = newConfInDeg.q2;
            _alpha2 = newConfInDeg.a2;
            _alpha3 = newConfInDeg.a3;
            _alpha4 = newConfInDeg.a4;
            _alpha5 = newConfInDeg.a5;
            RecalculateMatrices();
            return true;
        }
        return false;
    }
}