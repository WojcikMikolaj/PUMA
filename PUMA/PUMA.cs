﻿using System.ComponentModel.DataAnnotations;
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

    private Vector3 startPos;
    private Vector3 startRotInRad;
    private PUMAConfiguration startConf;
    
    private PUMAConfiguration currConf;
    
    private Vector3 endPos;
    private Vector3 endRotInRad;
    private PUMAConfiguration endConf;

    private Vector3 lastPos;
    private Vector3 lastRotInRad;
    private PUMAConfiguration lastConf;

    private readonly bool InterpolateConf; 

    public PUMA(bool interpolateConf)
    {
        InterpolateConf = interpolateConf;
        _l1 = new Cylinder(Direction.Z)
        {
            h = 3
        };
        _q2 = new Cylinder(Direction.X)
        {
            h = 3
        };
        _l3 = new Cylinder(Direction.NZ)
        {
            h = 3
        };
        _l4 = new Cylinder(Direction.X)
        {
            h = 3
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

        endPosition = (new Vector4(0, 0, 0, 1) * _f05).Xyz;
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

    private int _solutionNumber = 0;

    public int SolutionNumber
    {
        get => _solutionNumber;
        set
        {
            _solutionNumber = value;
            ChooseSolution();
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

    private Solution[] _solutions;
    public Vector3 endPosition = new Vector3();
    public void ChooseSolution()
    {
        if (_solutions != null)
        {
            var newConf = new PUMAConfiguration(
                _solutions[SolutionNumber].a1.a,
                _solutions[SolutionNumber].q2,
                _solutions[SolutionNumber].a2.a,
                _solutions[SolutionNumber].a3.a,
                _solutions[SolutionNumber].a4.a,
                _solutions[SolutionNumber].a5.a);

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
            }
        }
    }
    
    public bool MoveToPoint(Vector3 pos, Vector3 rotInDeg)
    {
        var solutions = IKPUMASolver.SolveInverse(pos,
            (MH.DegreesToRadians(rotInDeg.X), MH.DegreesToRadians(rotInDeg.Y), MH.DegreesToRadians(rotInDeg.Z)),
            new PUMASettings(_l1.h, _l3.h, _l4.h));
        _solutions = solutions;
        
        int bestSolution = 0;
        float distance = float.MaxValue;
        
        for (int i = 0; i < solutions.Length; i++)
        {
            var dist = CalculateDistance(pos, solutions[i]);
            if (dist < distance)
            {
                distance = dist;
                bestSolution = i;
            }
        }

        var newConf = new PUMAConfiguration(
            solutions[bestSolution].a1.a,
            solutions[bestSolution].q2,
            solutions[bestSolution].a2.a,
            solutions[bestSolution].a3.a,
            solutions[bestSolution].a4.a,
            solutions[bestSolution].a5.a);

        if (!newConf.IsNaNOrInf() && Math.Abs(distance - float.MaxValue) > 0.1)
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

    private float CalculateDistance(Vector3 targetPos, Solution solution)
    {
        var l1Matrix = Matrix4.CreateTranslation(0, 0, _l1.h);
        var q2Matrix = Matrix4.CreateTranslation(solution.q2, 0, 0);
        var l3Matrix = Matrix4.CreateTranslation(0, 0, -_l3.h);
        var l4Matrix = Matrix4.CreateTranslation(_l4.h, 0, 0);

        var a1Matrix = Matrix4.CreateRotationZ(solution.a1.a);
        var a2Matrix = Matrix4.CreateRotationY(solution.a2.a);
        var a3Matrix = Matrix4.CreateRotationY(solution.a3.a);
        var a4Matrix = Matrix4.CreateRotationZ(solution.a4.a);
        var a5Matrix = Matrix4.CreateRotationX(solution.a5.a);

        var f01 = a1Matrix;
        var f02 = a2Matrix * l1Matrix * f01;
        var f03 = a3Matrix * q2Matrix * f02;
        var f04 = a4Matrix * l3Matrix * f03;
        var f05 = a5Matrix * l4Matrix * f04;

        var newPos = new Vector4(0, 0, 0, 1) * f05;

        return Vector3.Distance(targetPos, newPos.Xyz);
    }

    public void SetStartPoint(Vector3 startingPointerPos, Vector3 startingPointerRot)
    {
        lastPos = startPos = startingPointerPos;
        lastRotInRad = startRotInRad = (MH.DegreesToRadians(startingPointerRot.X),
            MH.DegreesToRadians(startingPointerRot.Y),
            MH.DegreesToRadians(startingPointerRot.Z));
        
        lastConf = startConf = IKPUMASolver.SolveInverse(startPos, startRotInRad,
            new PUMASettings(_l1.h, _l3.h, _l4.h))[0].ToConf();
    }
    
    public void SetEndPoint(Vector3 endingPointerPos, Vector3 endingPointerRot)
    {
        endPos = endingPointerPos;
        endRotInRad = (MH.DegreesToRadians(endingPointerRot.X),
            MH.DegreesToRadians(endingPointerRot.Y),
                MH.DegreesToRadians(endingPointerRot.Z));
        
        endConf = IKPUMASolver.SolveInverse(endPos, endRotInRad,
            new PUMASettings(_l1.h, _l3.h, _l4.h))[0].ToConf();
    }

    public void CalculateCurrentConfiguration([Range(0,1)] float t)
    {
        if (t < 0)
        {
            t = 0;
        }
        
        if (t > 1)
        {
            t = 1;
        }
        
        
        if (InterpolateConf)
        {
            var a1 = (1.0f - t) * startConf.a1 + t * endConf.a1;
            var q2 = (1.0f - t) * startConf.q2 + t * endConf.q2;
            var a2 = (1.0f - t) * startConf.a2 + t * endConf.a2;
            var a3 = (1.0f - t) * startConf.a3 + t * endConf.a3;
            var a4 = (1.0f - t) * startConf.a4 + t * endConf.a4;
            var a5 = (1.0f - t) * startConf.a5 + t * endConf.a5;
            
            currConf = new PUMAConfiguration(a1, q2, a2, a3, a4, a5);
            _alpha1 = MH.RadiansToDegrees(a1);
            _alpha2 = MH.RadiansToDegrees(a2);
            _alpha3 = MH.RadiansToDegrees(a3);
            _alpha4 = MH.RadiansToDegrees(a4);
            _alpha5 = MH.RadiansToDegrees(a5);
            _q2.h = q2;
            RecalculateMatrices();
        }
        else
        {
            var currPos = (1.0f - t) * startPos + t * endPos;
            var currRotInRad = (1.0f - t) * startRotInRad + t * endRotInRad;
            
            var radX =0.0f;
            if (MH.Abs(startRotInRad.X - endRotInRad.X) < MH.Pi)
            {
                radX = (1.0f - t) * startRotInRad.X + t * endRotInRad.X;
            }
            else
            {
                radX = (1.0f - t) * startRotInRad.X + t * -(MH.TwoPi - endRotInRad.X);
            }

            var radY =0.0f;
            if (MH.Abs(startRotInRad.Y - endRotInRad.Y) < MH.Pi)
            {
                radY = (1.0f - t) * startRotInRad.Y + t * endRotInRad.Y;
            }
            else
            {
                radY = (1.0f - t) * startRotInRad.Y + t * -(MH.TwoPi - endRotInRad.Y);
            }

            var radZ =0.0f;
            if (MH.Abs(startRotInRad.Z - endRotInRad.Z) < MH.Pi)
            {
                radZ = (1.0f - t) * startRotInRad.Z + t * endRotInRad.Z;
            }
            else
            {
                radZ = (1.0f - t) * startRotInRad.Z + t * -(MH.TwoPi - endRotInRad.Z);
            }

            currRotInRad = (radX, radY, radZ);


            var solutions = IKPUMASolver.SolveInverse(currPos, currRotInRad,
                new PUMASettings(_l1.h, _l3.h, _l4.h));
            
            int bestSolution = 0;
            float distance = float.MaxValue;
            for (int i = 0; i < solutions.Length; i++)
            {
                var dist = CalculateDistance(lastConf, solutions[i]);
                if (dist < distance)
                {
                    distance = dist;
                    bestSolution = i;
                }
            }

            var newConf = new PUMAConfiguration(
                solutions[bestSolution].a1.a,
                solutions[bestSolution].q2,
                solutions[bestSolution].a2.a,
                solutions[bestSolution].a3.a,
                solutions[bestSolution].a4.a,
                solutions[bestSolution].a5.a);

            lastConf = newConf;
            
            if (!newConf.IsNaNOrInf() && distance != float.MaxValue)
            {
                var newConfInDeg = newConf.InDegrees();
                _alpha1 = newConfInDeg.a1;
                _q2.h = newConfInDeg.q2;
                _alpha2 = newConfInDeg.a2;
                _alpha3 = newConfInDeg.a3;
                _alpha4 = newConfInDeg.a4;
                _alpha5 = newConfInDeg.a5;
                RecalculateMatrices();
                lastPos = currPos;
                lastRotInRad = currRotInRad;
            }

    //        throw new ApplicationException();
        }
    }

    private float CalculateDistance(PUMAConfiguration lastConf, Solution solution)
    {
        var solutionConf = new PUMAConfiguration(
            solution.a1.a,
            solution.q2,
            solution.a2.a,
            solution.a3.a,
            solution.a4.a,
            solution.a5.a);

        
        float distance = 0.0f;
        distance = 0; //MathF.Abs(solutionConf.q2 - lastConf.q2);
        distance += MH.Min(MH.Abs(MH.ClampRadians(lastConf.a1) - MH.ClampRadians(solutionConf.a1)), MH.Abs(MH.TwoPi - MH.Abs(MH.ClampRadians(lastConf.a1) - MH.ClampRadians(solutionConf.a1))));
        distance += MH.Min(MH.Abs(MH.ClampRadians(lastConf.a2) - MH.ClampRadians(solutionConf.a2)), MH.Abs(MH.TwoPi - MH.Abs(MH.ClampRadians(lastConf.a2) - MH.ClampRadians(solutionConf.a2))));
        distance += MH.Min(MH.Abs(MH.ClampRadians(lastConf.a3) - MH.ClampRadians(solutionConf.a3)), MH.Abs(MH.TwoPi - MH.Abs(MH.ClampRadians(lastConf.a3) - MH.ClampRadians(solutionConf.a3))));
        distance += MH.Min(MH.Abs(MH.ClampRadians(lastConf.a4) - MH.ClampRadians(solutionConf.a4)), MH.Abs(MH.TwoPi - MH.Abs(MH.ClampRadians(lastConf.a4) - MH.ClampRadians(solutionConf.a4))));
        distance += MH.Min(MH.Abs(MH.ClampRadians(lastConf.a5) - MH.ClampRadians(solutionConf.a5)), MH.Abs(MH.TwoPi - MH.Abs(MH.ClampRadians(lastConf.a5) - MH.ClampRadians(solutionConf.a5))));
        return distance;
    }
}