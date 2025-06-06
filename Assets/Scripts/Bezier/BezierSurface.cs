using UnityEngine;

[System.Serializable]
public class BezierSurface
{
    [Header("Surface Definition")]
    public Vector3[,] controlPoints; //Control point grid
    public int uResolution = 15; //Surface horizontal smoothness
    public int vResolution = 15; //Surface vertical smoothness

    [Header("Identification")]
    public int surfaceID;
    public bool isDirty = true; //Does surface need to be regenerated?

    public BezierSurface(int uControlPoints = 4, int vControlPoints = 4)
    {
        controlPoints = new Vector3[uControlPoints, vControlPoints];
        InitializeDefaultControlPoints();
    }

    //Create initial control points for flat grid
    private void InitializeDefaultControlPoints()
    {
        int uCount = controlPoints.GetLength(0);
        int vCount = controlPoints.GetLength(1);

        for (int u = 0; u < uCount; u++)
        {
            for (int v = 0; v < vCount; v++)
            {
                controlPoints[u, v] = new Vector3(
                    (u - (uCount - 1) * 0.5f) * 0.5f,
                    0f,
                    (v - (vCount - 1) * 0.5f) * 0.5f
                );
            }
        }
    }

    //Get point on surface using uv coords
    public Vector3 EvaluateSurface(float u, float v)
    {
        int uCount = controlPoints.GetLength(0);
        int vCount = controlPoints.GetLength(1);

        //Bezier math for 4x4 surfaces
        if (uCount == 4 && vCount == 4)
        {
            return EvaluateBicubicBezier(u, v);
        }

        //Simpler interpolation for everything else
        return EvaluateBilinear(u, v);
    }

    //Bezier surface evaluation for 4x4 control grids
    private Vector3 EvaluateBicubicBezier(float u, float v)
    {
        Vector3 result = Vector3.zero;
        //Bernstein basis functions for cubic Bezier
        float[] Bu = new float[4];
        float[] Bv = new float[4];
        //Calc Bernstein polynomials for u direction
        Bu[0] = (1 - u) * (1 - u) * (1 - u);           //(1-u)^3
        Bu[1] = 3 * u * (1 - u) * (1 - u);             // 3u(1-u)^2
        Bu[2] = 3 * u * u * (1 - u);                   // 3u^2(1-u)
        Bu[3] = u * u * u;                             // u^3
        //Calc Bernstein polynomials for v direction
        Bv[0] = (1 - v) * (1 - v) * (1 - v);           // (1-v)^3
        Bv[1] = 3 * v * (1 - v) * (1 - v);             // 3v(1-v)^2
        Bv[2] = 3 * v * v * (1 - v);                   // 3v^2(1-v)
        Bv[3] = v * v * v;                             // v^3
        //Get all contributions from control points
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                result += Bu[i] * Bv[j] * controlPoints[i, j];
            }
        }

        return result;
    }

    //Interpolation for non-4x4 grids
    private Vector3 EvaluateBilinear(float u, float v)
    {
        int uCount = controlPoints.GetLength(0);
        int vCount = controlPoints.GetLength(1);
        //Get control points to interpolate between
        float uIndex = u * (uCount - 1);
        float vIndex = v * (vCount - 1);
        int u0 = Mathf.FloorToInt(uIndex);
        int v0 = Mathf.FloorToInt(vIndex);
        int u1 = Mathf.Min(u0 + 1, uCount - 1);
        int v1 = Mathf.Min(v0 + 1, vCount - 1);
        float uFrac = uIndex - u0;
        float vFrac = vIndex - v0;
        //Get the four corner points
        Vector3 p00 = controlPoints[u0, v0];
        Vector3 p10 = controlPoints[u1, v0];
        Vector3 p01 = controlPoints[u0, v1];
        Vector3 p11 = controlPoints[u1, v1];
        //Bilinear interpolation
        Vector3 p0 = Vector3.Lerp(p00, p10, uFrac);
        Vector3 p1 = Vector3.Lerp(p01, p11, uFrac);

        return Vector3.Lerp(p0, p1, vFrac);
    }
}