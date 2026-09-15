using UnityEngine;

public static class Helper
{
    // METHODS
    public static System.Numerics.Vector2 ToNumerics(this UnityEngine.Vector2 v)
    {
        return new System.Numerics.Vector2(v.x, v.y);
    }

    public static UnityEngine.Vector2 ToUnity(this System.Numerics.Vector2 v)
    {
        return new UnityEngine.Vector2(v.X, v.Y);
    }

    public static UnityEngine.Vector3 ToUnityWorld(this System.Numerics.Vector2 v, float height = 1f)
    {
        return new UnityEngine.Vector3(v.X, height, v.Y);
    }
}
