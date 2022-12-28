namespace TemplateProject;

public static class Vector3Extender
{
    public static System.Numerics.Vector3 ToNumerics(this OpenTK.Mathematics.Vector3 vector3)
    {
        return new System.Numerics.Vector3(vector3.X, vector3.Y, vector3.Z);
    }
    
    public static OpenTK.Mathematics.Vector3 ToOpenTK(this System.Numerics.Vector3  vector3)
    {
        return new OpenTK.Mathematics.Vector3(vector3.X, vector3.Y, vector3.Z);
    }
}