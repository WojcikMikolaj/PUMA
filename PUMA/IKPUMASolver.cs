using OpenTK.Mathematics;
using MH = OpenTK.Mathematics.MathHelper;
using Vector3 = OpenTK.Mathematics.Vector3;


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

public static class IKPUMASolver
{
    public static PUMAConfiguration SolveInverse(Vector3 targetPosition, Vector3 targetRotation,
        PUMASettings settings, PUMAConfiguration lastConfiguration = null)
    {
        Matrix4 f05 = Matrix4.CreateRotationX(targetRotation.X) *
                      Matrix4.CreateRotationY(targetRotation.Y) * Matrix4.CreateRotationZ(targetRotation.Z) * Matrix4.CreateTranslation(targetPosition);
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

        //Rozwiązanie A
        var a1 = (float) MH.Atan((xy * settings.l4 - py) / (xx * settings.l4 - px));
        var c1 = MH.Cos(a1);
        var s1 = MH.Sin(a1);

        var a4 = (float) MH.Asin(c1 * xy - s1 * xx);
        var c4 = MH.Cos(a4);
        var s4 = MH.Sin(a4);

        var a5 = (float) MH.Atan2(s1 * zx - c1 * zy, c1 * yy - s1 * yx);
        var c5 = MH.Cos(a5);
        var s5 = MH.Sin(a5);

        var a2 = (float) MH.Atan(-(c1 * c4 * (pz - settings.l4 * xz - settings.l1) + settings.l3 * (xx + s1 * s4)) /
                                 (c4 * (px - settings.l4 * xx) - c1 * settings.l3 * xz));
        var c2 = MH.Cos(a2);
        var s2 = MH.Sin(a2);

        var q2 = (float) ((c4 * (px - settings.l4 * xx) - c1 * settings.l3 * xz) / (c1 * c2 * c4));

        var a3 = (float) MH.Atan2(-xz / c4, (xx + s1 * s4) / (c1 * c4));

        return new PUMAConfiguration(a1, q2, a2, a3, a4, a5);
    }
}