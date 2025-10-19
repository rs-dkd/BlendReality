using UnityEngine;
struct IntVec2 : System.IEquatable<IntVec2>
{
    public Vector2 value;

    public float x { get { return value.x; } }
    public float y { get { return value.y; } }

    public IntVec2(Vector2 vector)
    {
        this.value = vector;
    }

    public override string ToString()
    {
        return string.Format("({0:F2}, {1:F2})", x, y);
    }

    public static bool operator ==(IntVec2 a, IntVec2 b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(IntVec2 a, IntVec2 b)
    {
        return !(a == b);
    }

    public bool Equals(IntVec2 p)
    {
        return round(x) == round(p.x) &&
            round(y) == round(p.y);
    }

    public bool Equals(Vector2 p)
    {
        return round(x) == round(p.x) &&
            round(y) == round(p.y);
    }

    public override bool Equals(System.Object b)
    {
        return (b is IntVec2 && (this.Equals((IntVec2)b))) ||
            (b is Vector2 && this.Equals((Vector2)b));
    }

    public override int GetHashCode()
    {
        return VectorHash.GetHashCode(value);
    }

    private static int round(float v)
    {
        return System.Convert.ToInt32(v * VectorHash.FltCompareResolution);
    }

    public static implicit operator Vector2(IntVec2 p)
    {
        return p.value;
    }

    public static implicit operator IntVec2(Vector2 p)
    {
        return new IntVec2(p);
    }
}

struct IntVec3 : System.IEquatable<IntVec3>
{
    public Vector3 value;

    public float x { get { return value.x; } }
    public float y { get { return value.y; } }
    public float z { get { return value.z; } }

    public IntVec3(Vector3 vector)
    {
        this.value = vector;
    }

    public override string ToString()
    {
        return string.Format("({0:F2}, {1:F2}, {2:F2})", x, y, z);
    }

    public static bool operator ==(IntVec3 a, IntVec3 b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(IntVec3 a, IntVec3 b)
    {
        return !(a == b);
    }

    public bool Equals(IntVec3 p)
    {
        return round(x) == round(p.x) &&
            round(y) == round(p.y) &&
            round(z) == round(p.z);
    }

    public bool Equals(Vector3 p)
    {
        return round(x) == round(p.x) &&
            round(y) == round(p.y) &&
            round(z) == round(p.z);
    }

    public override bool Equals(System.Object b)
    {
        return (b is IntVec3 && (this.Equals((IntVec3)b))) ||
            (b is Vector3 && this.Equals((Vector3)b));
    }

    public override int GetHashCode()
    {
        return VectorHash.GetHashCode(value);
    }

    private static int round(float v)
    {
        return System.Convert.ToInt32(v * VectorHash.FltCompareResolution);
    }

    public static implicit operator Vector3(IntVec3 p)
    {
        return p.value;
    }

    public static implicit operator IntVec3(Vector3 p)
    {
        return new IntVec3(p);
    }
}

static class VectorHash
{
    public const float FltCompareResolution = 1000f;

    static int HashFloat(float f)
    {
        ulong u = (ulong)(f * FltCompareResolution);
        return (int)(u % int.MaxValue);
    }

    /// <summary>
    /// Return the rounded hashcode for a vector2
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    public static int GetHashCode(Vector2 v)
    {
        // http://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode/263416#263416
        int hash = 27;

        unchecked
        {
            hash = hash * 29 + HashFloat(v.x);
            hash = hash * 29 + HashFloat(v.y);
        }

        return hash;
    }

    /// <summary>
    /// Return the hashcode for a vector3 without first converting it to pb_IntVec3.
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    public static int GetHashCode(Vector3 v)
    {
        // http://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode/263416#263416
        int hash = 27;

        unchecked
        {
            hash = hash * 29 + HashFloat(v.x);
            hash = hash * 29 + HashFloat(v.y);
            hash = hash * 29 + HashFloat(v.z);
        }

        return hash;
    }

    /// <summary>
    /// Return the hashcode for a vector3 without first converting it to pb_IntVec3.
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    public static int GetHashCode(Vector4 v)
    {
        // http://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode/263416#263416
        int hash = 27;

        unchecked
        {
            hash = hash * 29 + HashFloat(v.x);
            hash = hash * 29 + HashFloat(v.y);
            hash = hash * 29 + HashFloat(v.z);
            hash = hash * 29 + HashFloat(v.w);
        }

        return hash;
    }
}