using OpenTK.Mathematics;
using MH = OpenTK.Mathematics.MathHelper;
using Vector3 = OpenTK.Mathematics.Vector3;

// ReSharper disable InconsistentNaming


namespace TemplateProject;

// ReSharper disable once InconsistentNaming
public record PUMAConfiguration(double a1, double q2, double a2, double a3, double a4, double a5)
{
    public bool IsNaNOrInf()
    {
        return double.IsNaN(a1) || double.IsInfinity(a1)
                               || double.IsNaN(q2) || double.IsInfinity(q2)
                               || double.IsNaN(a2) || double.IsInfinity(a2)
                               || double.IsNaN(a3) || double.IsInfinity(a3)
                               || double.IsNaN(a4) || double.IsInfinity(a4)
                               || double.IsNaN(a5) || double.IsInfinity(a5);
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

public record PUMASettings(double l1, double l3, double l4);

public record AngleParam(double angle, double sine, double cosine)
{
    public double a => angle;
    public double s => sine;
    public double c => cosine;
}

public class Solution
{
    public AngleParam a1;
    public AngleParam a2;
    public AngleParam a3;
    public AngleParam a4;
    public AngleParam a5;
    public double q2;

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
    private static double EPS = 1e-6f;
    public static Solution[] SolveInverse(Vector3 targetPosition, Vector3 targetRotationInRad,
        PUMASettings settings, PUMAConfiguration lastConfiguration = null)
    {
        Matrix4 f05 = Matrix4.CreateRotationX(targetRotationInRad.X) *
                      Matrix4.CreateRotationY(targetRotationInRad.Y) * Matrix4.CreateRotationZ(targetRotationInRad.Z) *
                      Matrix4.CreateTranslation(targetPosition);
        f05.Transpose();
        
        
        var xx = (double)f05.M11;
        var xy = (double)f05.M21;
        var xz = (double)f05.M31;

        var yx = (double)f05.M12;
        var yy = (double)f05.M22;
        var yz = (double)f05.M32;

        var zx = (double)f05.M13;
        var zy = (double)f05.M23;
        var zz = (double)f05.M33;

        var px = (double)f05.M14;
        var py = (double)f05.M24;
        var pz = (double)f05.M34;

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
            solutions[i].a1 = CalculateA1(settings, xy, ref py, xx, ref px, i < solutions.Count / 2);
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
            if (solutions[i].q2 < 0)
            {
                var angle2 = solutions[i].a2.a + MH.Pi;
                solutions[i].a2 = new AngleParam(angle2, (double)MH.Sin(angle2), (double)MH.Cos(angle2));
                solutions[i].q2 = -solutions[i].q2;
            }
            //
            //Atan2 - 1 rozwiązanie
            solutions[i].a3 = CalculateA3(xz, xx, solutions[i].a1, solutions[i].a2, solutions[i].a4);
        }

        return solutions.ToArray();
    }

    private static double CalculateQ2(PUMASettings settings, double px, double xx, double xz, AngleParam a1, AngleParam a2, AngleParam a4)
    {
        return ((a4.c * (px - settings.l4 * xx) - a1.c * settings.l3 * xz) / (a1.c * a2.c * a4.c));
    }

    private static AngleParam CalculateA1(PUMASettings settings, double xy, ref double py, double xx, ref double px,
        bool firstSolution)
    {
        if(double.IsInfinity((xy * settings.l4 - py) / (xx * settings.l4 - px))
           ||double.IsNegativeInfinity((xy * settings.l4 - py) / (xx * settings.l4 - px)))
        {
            px += EPS; 
            py += EPS;
        }
        var a1 = (double) MH.Atan((xy * settings.l4 - py) / (xx * settings.l4 - px));
        if (double.IsNaN((xy * settings.l4 - py) / (xx * settings.l4 - px)))
        {
            a1 = 0;
        }
        if (!firstSolution)
        {
            a1 += MH.Pi;
        }
        var s1 = (double) MH.Sin(a1);
        var c1 = (double) MH.Cos(a1);
        return new AngleParam(a1, s1, c1);
    }

    private static AngleParam CalculateA2(PUMASettings settings, double pz, double xz, double xx, double px, AngleParam a1,
        AngleParam a4, bool firstSolution)
    {
        var a2 = (double) MH.Atan(
            -(a1.c * a4.c * (pz - settings.l4 * xz - settings.l1) + settings.l3 * (xx + a1.s * a4.s)) /
            (a4.c * (px - settings.l4 * xx) - a1.c * settings.l3 * xz));
        if (double.IsNaN(-(a1.c * a4.c * (pz - settings.l4 * xz - settings.l1) + settings.l3 * (xx + a1.s * a4.s)) /
                        (a4.c * (px - settings.l4 * xx) - a1.c * settings.l3 * xz)))
        {
            a2 = 0;
        }
        if (!firstSolution)
        {
            a2 += MH.Pi;
        }

        var s2 = (double) MH.Sin(a2);
        var c2 = (double) MH.Cos(a2);
        return new AngleParam(a2, s2, c2);
    }

    private static AngleParam CalculateA3(double xz, double xx, AngleParam a1,AngleParam a2, AngleParam a4)
    {
        var a3 = (double) MH.Atan2(-xz / a4.c, (xx + a1.s * a4.s) / (a1.c * a4.c)) - a2.a;
        var s3 = (double) MH.Sin(a3);
        var c3 = (double) MH.Cos(a3);

        return new AngleParam(a3, s3, c3);
    }

    private static AngleParam CalculateA4(double xy, double xx, AngleParam a1, bool firstSolution)
    {
        var a4 = (double) MH.Asin(a1.c * xy - a1.s * xx);
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

        var s4 = (double) MH.Sin(a4);
        var c4 = (double) MH.Cos(a4);
        return new AngleParam(a4, s4, c4);
    }

    private static AngleParam CalculateA5(double zx, double zy, double yy, double yx, AngleParam a1)
    {
        var sgnc4 = a1.c >= 0 ? 1 : -1;
        var a5 = (double) MH.Atan2(sgnc4 * (a1.s * zx - a1.c * zy), sgnc4 * (a1.c * yy - a1.s * yx));
        var s5 = (double) MH.Sin(a5);
        var c5 = (double) MH.Cos(a5);
        return new AngleParam(a5, s5, c5);
    }
}