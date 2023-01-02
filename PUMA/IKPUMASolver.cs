using OpenTK.Mathematics;
using MH = OpenTK.Mathematics.MathHelper;
using Vector3 = OpenTK.Mathematics.Vector3;

// ReSharper disable InconsistentNaming


namespace TemplateProject;

// ReSharper disable once InconsistentNaming
public record PUMAConfiguration(float a1, float q2, float a2, float a3, float a4, float a5)
{
    public bool IsNaNOrInf()
    {
        return float.IsNaN(a1) || float.IsInfinity(a1)
                               || float.IsNaN(q2) || float.IsInfinity(q2)
                               || float.IsNaN(a2) || float.IsInfinity(a2)
                               || float.IsNaN(a3) || float.IsInfinity(a3)
                               || float.IsNaN(a4) || float.IsInfinity(a4)
                               || float.IsNaN(a5) || float.IsInfinity(a5);
    }

    public PUMAConfiguration InDegrees()
    {
        return new PUMAConfiguration(
            MH.RadiansToDegrees(this.a1),
            this.q2,
            MH.RadiansToDegrees(this.a2),
            MH.RadiansToDegrees(this.a3),
            MH.RadiansToDegrees(this.a4),
            MH.RadiansToDegrees(this.a5));
    }
}

public record PUMASettings(float l1, float l3, float l4);

public record AngleParam(float angle, float sine, float cosine)
{
    public float a => angle;
    public float s => sine;
    public float c => cosine;
}

public class Solution
{
    public AngleParam a1;
    public AngleParam a2;
    public AngleParam a3;
    public AngleParam a4;
    public AngleParam a5;
    public float q2;

    public PUMAConfiguration ToConf()
    {
        return new PUMAConfiguration(
            a1.a,
            q2,
            a2.a,
            a3.a,
            a4.a,
            a5.a);
    }
}

public static class IKPUMASolver
{
    public static Solution[] SolveInverse(Vector3 targetPosition, Vector3 targetRotationInRad,
        PUMASettings settings, PUMAConfiguration lastConfiguration = null)
    {
        Matrix4 f05 = Matrix4.CreateRotationX(targetRotationInRad.X) *
                      Matrix4.CreateRotationY(targetRotationInRad.Y) * Matrix4.CreateRotationZ(targetRotationInRad.Z) *
                      Matrix4.CreateTranslation(targetPosition);
        f05.Transpose();
        
        
        var xx = f05.M11;
        var xy = f05.M21;
        var xz = f05.M31;

        var yx = f05.M12;
        var yy = f05.M22;
        var yz = f05.M32;

        var zx = f05.M13;
        var zy = f05.M23;
        var zz = f05.M33;

        var px = f05.M14;
        var py = f05.M24;
        var pz = f05.M34;

        var solutions = new List<Solution>
        {
            new Solution(),
            new Solution(),
            new Solution(),
            new Solution(),
            new Solution(),
            new Solution(),
            new Solution(),
            new Solution()
        };


        for (int i = 0; i < solutions.Count; i++)
        {
            //Atan - 2 rozwiązania
            //Alpha1
            solutions[i].a1 = CalculateA1(settings, xy, py, xx, px, i < solutions.Count / 2);
            //Asin - 2 rozwiązania
            //Alpha4
            solutions[i].a4 = CalculateA4(xy, xx, solutions[i].a1,
                i < solutions.Count / 4 || (i >= solutions.Count / 2  && i < solutions.Count/4* 3));

            //Atan2 - 1 rozwiązanie
            //Alpha5
            solutions[i].a5 = CalculateA5(zx, zy, yy, yx, solutions[i].a1);

            //Atan - 2 rozwiązania
            //Alpha2
            solutions[i].a2 = CalculateA2(settings, pz, xz, xx, px, solutions[i].a1, solutions[i].a4, i % 2 == 0);
            
            //Q2
            solutions[i].q2 = CalculateQ2(settings, px, xx, xz, solutions[i].a1, solutions[i].a2, solutions[i].a4);
            
            //Atan2 - 1 rozwiązanie
            solutions[i].a3 = CalculateA3(xz, xx, solutions[i].a1, solutions[i].a2, solutions[i].a4);
        }

        return solutions.ToArray();
    }

    private static float CalculateQ2(PUMASettings settings, float px, float xx, float xz, AngleParam a1, AngleParam a2, AngleParam a4)
    {
        return ((a4.c * (px - settings.l4 * xx) - a1.c * settings.l3 * xz) / (a1.c * a2.c * a4.c));
    }

    private static AngleParam CalculateA1(PUMASettings settings, float xy, float py, float xx, float px,
        bool firstSolution)
    {
        var a1 = (float) MH.Atan((xy * settings.l4 - py) / (xx * settings.l4 - px));
        if (a1 is Single.NaN)
        {
            a1 = 0;
        }
        if (!firstSolution)
        {
            a1 += MH.Pi;
        }

        var s1 = (float) MH.Sin(a1);
        var c1 = (float) MH.Cos(a1);
        return new AngleParam(a1, s1, c1);
    }

    private static AngleParam CalculateA2(PUMASettings settings, float pz, float xz, float xx, float px, AngleParam a1,
        AngleParam a4, bool firstSolution)
    {
        var a2 = (float) MH.Atan(
            -(a1.c * a4.c * (pz - settings.l4 * xz - settings.l1) + settings.l3 * (xx + a1.s * a4.s)) /
            (a4.c * (px - settings.l4 * xx) - a1.c * settings.l3 * xz));
        if (a2 is Single.NaN)
        {
            a2 = 0;
        }
        if (!firstSolution)
        {
            a2 += MH.Pi;
        }

        var s2 = (float) MH.Sin(a2);
        var c2 = (float) MH.Cos(a2);
        return new AngleParam(a2, s2, c2);
    }

    private static AngleParam CalculateA3(float xz, float xx, AngleParam a1,AngleParam a2, AngleParam a4)
    {
        var a3 = (float) MH.Atan2(-xz / a4.c, (xx + a1.s * a4.s) / (a1.c * a4.c)) - a2.a;
        var s3 = (float) MH.Sin(a3);
        var c3 = (float) MH.Cos(a3);

        return new AngleParam(a3, s3, c3);
    }

    private static AngleParam CalculateA4(float xy, float xx, AngleParam a1, bool firstSolution)
    {
        var a4 = (float) MH.Asin(a1.c * xy - a1.s * xx);
        if (!firstSolution)
        {
            if (a4 > 0)
            {
                a4 = MH.Pi - a4;
            }
            else
            {
                a4 = -MH.Pi - a4;
            }
        }

        var s4 = (float) MH.Sin(a4);
        var c4 = (float) MH.Cos(a4);
        return new AngleParam(a4, s4, c4);
    }

    private static AngleParam CalculateA5(float zx, float zy, float yy, float yx, AngleParam a1)
    {
        var a5 = (float) MH.Atan2(a1.s * zx - a1.c * zy, a1.c * yy - a1.s * yx);
        var s5 = (float) MH.Sin(a5);
        var c5 = (float) MH.Cos(a5);
        return new AngleParam(s5, s5, c5);
    }
}