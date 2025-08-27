using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;
using PolyhedralNetSplines;
using System.IO;
using System.Linq;

public class PNSIntegration : MonoBehaviour
{
    [Header("PNS Settings")]
    public bool degreeRaise = true;
    public string exportFormat = "bv";

    [Header("Export Settings")]
    public string exportPath = "Assets/PNS_Exports/";

    private void Start()
    {
        if (!Directory.Exists(exportPath))
        {
            Directory.CreateDirectory(exportPath);
        }
    }

    public void ConvertProBuilderToPNS(ProBuilderMesh proBuilderMesh, string filename = "output")
    {
        try
        {
            // Convert ProBuilder mesh to PNS format
            PNSControlMesh pnsControlMesh = ConvertToPNSControlMesh(proBuilderMesh);

            string outputPath = Path.Combine(exportPath, $"{filename}.{exportFormat}");

            switch (exportFormat.ToLower())
            {
                case "bv":
                    PNS.CreateBV(pnsControlMesh, outputPath, degreeRaise);
                    break;
                case "igs":
                    PNS.CreateIGS(pnsControlMesh, outputPath, degreeRaise);
                    break;
                case "step":
                    PNS.CreateSTEP(pnsControlMesh, outputPath, degreeRaise);
                    break;
                default:
                    Debug.LogError($"Unsupported export format: {exportFormat}");
                    return;
            }

            Debug.Log($"Successfully exported PNS surface to: {outputPath}");
            pnsControlMesh.Dispose();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to convert mesh to PNS: {e.Message}");
        }
    }

    private PNSControlMesh ConvertToPNSControlMesh(ProBuilderMesh proBuilderMesh)
    {
        // Get mesh data from ProBuilder
        Vector3[] vertices = proBuilderMesh.positions.ToArray();
        Face[] faces = proBuilderMesh.faces.ToArray();

        // Convert vertices to PNS format
        float[,] pnsVertices = new float[vertices.Length, 3];
        for (int i = 0; i < vertices.Length; i++)
        {
            pnsVertices[i, 0] = vertices[i].x;
            pnsVertices[i, 1] = vertices[i].y;
            pnsVertices[i, 2] = vertices[i].z;
        }

        // Convert faces to PNS format
        int[][] pnsFaces = new int[faces.Length][];
        for (int i = 0; i < faces.Length; i++)
        {
            var face = faces[i];
            pnsFaces[i] = face.indexes.ToArray();
        }

        // Create and return PNS control mesh
        return PNSControlMesh.FromData(pnsVertices, pnsFaces);
    }

    public void ConvertBezierSurfaceToPNS(BezierSurface bezierSurface, string filename = "bezier_surface")
    {
        try
        {
            // Gen mesh from Bezier surface control points
            PNSControlMesh pnsControlMesh = ConvertBezierToPNSControlMesh(bezierSurface);

            // Export
            string outputPath = Path.Combine(exportPath, $"{filename}.{exportFormat}");

            switch (exportFormat.ToLower())
            {
                case "bv":
                    PNS.CreateBV(pnsControlMesh, outputPath, degreeRaise);
                    break;
                case "igs":
                    PNS.CreateIGS(pnsControlMesh, outputPath, degreeRaise);
                    break;
                case "step":
                    PNS.CreateSTEP(pnsControlMesh, outputPath, degreeRaise);
                    break;
            }

            Debug.Log($"Successfully exported Bezier surface to PNS: {outputPath}");
            pnsControlMesh.Dispose();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to convert Bezier surface to PNS: {e.Message}");
        }
    }

    private PNSControlMesh ConvertBezierToPNSControlMesh(BezierSurface bezierSurface)
    {
        int uCount = bezierSurface.controlPoints.GetLength(0);
        int vCount = bezierSurface.controlPoints.GetLength(1);

        // Get control points
        float[,] vertices = new float[uCount * vCount, 3];
        int vertexIndex = 0;

        for (int u = 0; u < uCount; u++)
        {
            for (int v = 0; v < vCount; v++)
            {
                Vector3 point = bezierSurface.controlPoints[u, v];
                vertices[vertexIndex, 0] = point.x;
                vertices[vertexIndex, 1] = point.y;
                vertices[vertexIndex, 2] = point.z;
                vertexIndex++;
            }
        }

        // Create quad faces connecting control points
        List<int[]> faces = new List<int[]>();

        for (int u = 0; u < uCount - 1; u++)
        {
            for (int v = 0; v < vCount - 1; v++)
            {
                // Create quad face indices
                int bottomLeft = u * vCount + v;
                int bottomRight = u * vCount + (v + 1);
                int topLeft = (u + 1) * vCount + v;
                int topRight = (u + 1) * vCount + (v + 1);

                // Add quad face (counter-clockwise winding)
                faces.Add(new int[] { bottomLeft, bottomRight, topRight, topLeft });
            }
        }

        return PNSControlMesh.FromData(vertices, faces.ToArray());
    }

    public void ConvertSelectedModel()
    {
        var selectedModels = SelectionManager.Instance.selectedModels;

        if (selectedModels.Count == 0)
        {
            Debug.LogWarning("No model selected for PNS conversion");
            return;
        }

        foreach (var model in selectedModels)
        {
            ProBuilderMesh proBuilderMesh = model.GetEditModel();
            if (proBuilderMesh != null)
            {
                string filename = $"model_{model.GetInstanceID()}_{System.DateTime.Now.Ticks}";
                ConvertProBuilderToPNS(proBuilderMesh, filename);
            }
        }
    }

    public void ConvertAllModels()
    {
        var allModels = ModelsManager.Instance.GetAllModelsInScene();

        if (allModels.Count == 0)
        {
            Debug.LogWarning("No models found in scene for conversion");
            return;
        }

        for (int i = 0; i < allModels.Count; i++)
        {
            ProBuilderMesh proBuilderMesh = allModels[i].GetEditModel();
            if (proBuilderMesh != null)
            {
                string filename = $"batch_model_{i}_{System.DateTime.Now.Ticks}";
                ConvertProBuilderToPNS(proBuilderMesh, filename);
            }
        }

        Debug.Log($"Batch converted {allModels.Count} models to PNS format");
    }

    public void AdvancedPNSConversion(ProBuilderMesh proBuilderMesh, string filename = "advanced_output")
    {
        try
        {
            PNSControlMesh controlMesh = ConvertToPNSControlMesh(proBuilderMesh);

            string outputPath = Path.Combine(exportPath, $"{filename}.{exportFormat}");

            if (exportFormat.ToLower() == "bv")
            {
                using (var writer = new BVWriter(outputPath))
                {
                    writer.Start();

                    // Get patch builders for manual processing
                    var patchBuilders = PNS.GetPatchBuilders(controlMesh);

                    foreach (var builder in patchBuilders)
                    {
                        if (degreeRaise)
                        {
                            builder.DegRaise();
                        }

                        // Build and process individual patches
                        var patches = builder.BuildPatches(controlMesh);
                        foreach (var patch in patches)
                        {
                            // Could add custom patch processing here
                            writer.Consume(patch);
                            patch.Dispose();
                        }

                        builder.Dispose();
                    }

                    writer.Stop();
                }
            }
            else
            {
                // Fallback
                PNS.ProcessMesh(controlMesh,
                    exportFormat.ToLower() == "igs" ?
                        (PatchConsumer)new IGSWriter(outputPath) :
                        new STEPWriter(outputPath),
                    degreeRaise);
            }

            Debug.Log($"Advanced PNS conversion completed: {outputPath}");
            controlMesh.Dispose();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Advanced PNS conversion failed: {e.Message}");
        }
    }

    public void SetDegreeRaise(bool value) => degreeRaise = value;
    public void SetExportFormat(string format) => exportFormat = format;
    public void SetExportPath(string path) => exportPath = path;

    public void TestPNSWithSimpleQuad()
    {
        try
        {
            Debug.Log("=== Starting PNS Simple Quad Test ===");

            // Create a simple quad manually
            float[,] vertices = new float[4, 3] {
                {-1, 0, -1},  // Bot left
                { 1, 0, -1},  // Bot right  
                { 1, 0,  1},  // Top right
                {-1, 0,  1}   // Top left
            };

            int[][] faces = new int[1][] {
                new int[] {0, 1, 2, 3}  // Single quad face
            };

            Debug.Log("Creating PNS control mesh from simple quad");
            var controlMesh = PNSControlMesh.FromData(vertices, faces);

            string testFilename = $"test_quad_{System.DateTime.Now.Ticks}";
            string outputPath = Path.Combine(exportPath, $"{testFilename}.{exportFormat}");

            Debug.Log($"Exporting to: {outputPath}");

            switch (exportFormat.ToLower())
            {
                case "bv":
                    PNS.CreateBV(controlMesh, outputPath, degreeRaise);
                    break;
                case "igs":
                    PNS.CreateIGS(controlMesh, outputPath, degreeRaise);
                    break;
                case "step":
                    PNS.CreateSTEP(controlMesh, outputPath, degreeRaise);
                    break;
            }

            Debug.Log($"Test quad exported successfully to: {outputPath}");
            controlMesh.Dispose();

            // Debug the file content
            DebugGeneratedFile($"{testFilename}.{exportFormat}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Test quad failed: {e.Message}");
            Debug.LogError($"Stack trace: {e.StackTrace}");
        }
    }

    public void TestPNSWithComplexQuadMesh()
    {
        try
        {
            Debug.Log("=== Testing PNS with 3x3 Quad Grid ===");

            // Create a 3x3 grid of vertices
            List<float[]> verticesList = new List<float[]>();
            for (int u = 0; u < 3; u++)
            {
                for (int v = 0; v < 3; v++)
                {
                    float x = (u - 1) * 1.0f;  // -1, 0, 1
                    float z = (v - 1) * 1.0f;  // -1, 0, 1
                    float y = Mathf.Sin(u * 0.5f) * Mathf.Cos(v * 0.5f) * 0.2f;
                    verticesList.Add(new float[] { x, y, z });
                }
            }

            // Convert to arr format
            float[,] vertices = new float[verticesList.Count, 3];
            for (int i = 0; i < verticesList.Count; i++)
            {
                vertices[i, 0] = verticesList[i][0];
                vertices[i, 1] = verticesList[i][1];
                vertices[i, 2] = verticesList[i][2];
            }

            // Create quad faces (2x2 grid of quads)
            List<int[]> facesList = new List<int[]>();
            for (int u = 0; u < 2; u++)
            {
                for (int v = 0; v < 2; v++)
                {
                    int bottomLeft = u * 3 + v;
                    int bottomRight = u * 3 + (v + 1);
                    int topLeft = (u + 1) * 3 + v;
                    int topRight = (u + 1) * 3 + (v + 1);

                    // Add quad with proper winding
                    facesList.Add(new int[] { bottomLeft, bottomRight, topRight, topLeft });
                }
            }

            var controlMesh = PNSControlMesh.FromData(vertices, facesList.ToArray());

            string testFilename = $"test_complex_quad_{System.DateTime.Now.Ticks}";
            string outputPath = Path.Combine(exportPath, $"{testFilename}.{exportFormat}");

            switch (exportFormat.ToLower())
            {
                case "bv":
                    PNS.CreateBV(controlMesh, outputPath, degreeRaise);
                    break;
                case "igs":
                    PNS.CreateIGS(controlMesh, outputPath, degreeRaise);
                    break;
                case "step":
                    PNS.CreateSTEP(controlMesh, outputPath, degreeRaise);
                    break;
            }

            Debug.Log($"Complex test exported to: {outputPath}");
            controlMesh.Dispose();

            // Debug the file content
            DebugGeneratedFile($"{testFilename}.{exportFormat}");

        }
        catch (System.Exception e)
        {
            Debug.LogError($"Complex test failed: {e.Message}");
        }
    }

    public void DebugGeneratedFile(string filename)
    {
        try
        {
            string filePath = Path.Combine(exportPath, filename);
            if (File.Exists(filePath))
            {
                string content = File.ReadAllText(filePath);
                Debug.Log($"File content length: {content.Length}");
                Debug.Log($"First 500 characters:\n{content.Substring(0, Mathf.Min(500, content.Length))}");
            }
            else
            {
                Debug.LogError($"File not found: {filePath}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error reading file: {e.Message}");
        }
    }

    public void TestPNSWithInterestingGeometry()
    {
        try
        {
            Debug.Log("=== Testing PNS with Interesting Geometry ===");

            // Create a 4x4 grid with more dramatic height variation
            List<float[]> verticesList = new List<float[]>();
            for (int u = 0; u < 4; u++)
            {
                for (int v = 0; v < 4; v++)
                {
                    float x = (u - 1.5f) * 1.5f;  // -2.25 to 2.25
                    float z = (v - 1.5f) * 1.5f;  // -2.25 to 2.25

                    // Create a more interesting surface
                    float y = Mathf.Sin(u * Mathf.PI * 0.5f) * Mathf.Cos(v * Mathf.PI * 0.5f) * 0.8f
                            + Mathf.Sin(u * Mathf.PI * 0.25f) * 0.3f
                            + Mathf.Cos(v * Mathf.PI * 0.3f) * 0.2f;

                    verticesList.Add(new float[] { x, y, z });
                }
            }

            // Convert to arr format
            float[,] vertices = new float[verticesList.Count, 3];
            for (int i = 0; i < verticesList.Count; i++)
            {
                vertices[i, 0] = verticesList[i][0];
                vertices[i, 1] = verticesList[i][1];
                vertices[i, 2] = verticesList[i][2];
            }

            // Create quad faces
            List<int[]> facesList = new List<int[]>();
            for (int u = 0; u < 3; u++)
            {
                for (int v = 0; v < 3; v++)
                {
                    int bottomLeft = u * 4 + v;
                    int bottomRight = u * 4 + (v + 1);
                    int topLeft = (u + 1) * 4 + v;
                    int topRight = (u + 1) * 4 + (v + 1);

                    // Add quad with proper winding
                    facesList.Add(new int[] { bottomLeft, bottomRight, topRight, topLeft });
                }
            }

            var controlMesh = PNSControlMesh.FromData(vertices, facesList.ToArray());

            string testFilename = $"test_interesting_{System.DateTime.Now.Ticks}";
            string outputPath = Path.Combine(exportPath, $"{testFilename}.{exportFormat}");

            switch (exportFormat.ToLower())
            {
                case "bv":
                    PNS.CreateBV(controlMesh, outputPath, degreeRaise);
                    break;
                case "igs":
                    PNS.CreateIGS(controlMesh, outputPath, degreeRaise);
                    break;
                case "step":
                    PNS.CreateSTEP(controlMesh, outputPath, degreeRaise);
                    break;
            }

            Debug.Log($"Interesting geometry exported to: {outputPath}");
            controlMesh.Dispose();

            DebugGeneratedFile($"{testFilename}.{exportFormat}");

        }
        catch (System.Exception e)
        {
            Debug.LogError($"Interesting geometry test failed: {e.Message}");
        }
    }

    public void TestPNSWithCylinderSurface()
    {
        try
        {
            Debug.Log("=== Testing PNS with Cylinder Surface ===");

            // Create a cylinder
            int uSteps = 6; // Around circumference
            int vSteps = 4; // Along height

            List<float[]> verticesList = new List<float[]>();
            for (int u = 0; u < uSteps; u++)
            {
                for (int v = 0; v < vSteps; v++)
                {
                    float angle = (float)u / (uSteps - 1) * Mathf.PI * 1.5f; // 270 deg
                    float height = (float)v / (vSteps - 1) * 2.0f - 1.0f; // -1 to 1

                    float radius = 1.0f + Mathf.Sin(height * Mathf.PI) * 0.3f;

                    float x = Mathf.Cos(angle) * radius;
                    float z = Mathf.Sin(angle) * radius;
                    float y = height;

                    verticesList.Add(new float[] { x, y, z });
                }
            }

            // Convert to arr format
            float[,] vertices = new float[verticesList.Count, 3];
            for (int i = 0; i < verticesList.Count; i++)
            {
                vertices[i, 0] = verticesList[i][0];
                vertices[i, 1] = verticesList[i][1];
                vertices[i, 2] = verticesList[i][2];
            }

            // Create quad faces
            List<int[]> facesList = new List<int[]>();
            for (int u = 0; u < uSteps - 1; u++)
            {
                for (int v = 0; v < vSteps - 1; v++)
                {
                    int bottomLeft = u * vSteps + v;
                    int bottomRight = u * vSteps + (v + 1);
                    int topLeft = (u + 1) * vSteps + v;
                    int topRight = (u + 1) * vSteps + (v + 1);

                    // Add quad with proper winding
                    facesList.Add(new int[] { bottomLeft, bottomRight, topRight, topLeft });
                }
            }

            var controlMesh = PNSControlMesh.FromData(vertices, facesList.ToArray());

            string testFilename = $"test_cylinder_{System.DateTime.Now.Ticks}";
            string outputPath = Path.Combine(exportPath, $"{testFilename}.{exportFormat}");

            switch (exportFormat.ToLower())
            {
                case "bv":
                    PNS.CreateBV(controlMesh, outputPath, degreeRaise);
                    break;
                case "igs":
                    PNS.CreateIGS(controlMesh, outputPath, degreeRaise);
                    break;
                case "step":
                    PNS.CreateSTEP(controlMesh, outputPath, degreeRaise);
                    break;
            }

            Debug.Log($"Cylinder surface exported to: {outputPath}");
            controlMesh.Dispose();

            DebugGeneratedFile($"{testFilename}.{exportFormat}");

        }
        catch (System.Exception e)
        {
            Debug.LogError($"Cylinder surface test failed: {e.Message}");
        }
    }


    public void TestPNSWithLowPolySphere()
    {
        try
        {
            Debug.Log("=== Testing PNS with Low-Poly Sphere ===");

            int rings = 6;
            int sectors = 8;
            List<float[]> verticesList = new List<float[]>();

            verticesList.Add(new float[] { 0, 1, 0 });

            for (int ring = 1; ring < rings; ring++)
            {
                float phi = Mathf.PI * ring / rings;
                float y = Mathf.Cos(phi);
                float ringRadius = Mathf.Sin(phi);

                for (int sector = 0; sector < sectors; sector++)
                {
                    float theta = 2.0f * Mathf.PI * sector / sectors;
                    float x = ringRadius * Mathf.Cos(theta);
                    float z = ringRadius * Mathf.Sin(theta);

                    verticesList.Add(new float[] { x, y, z });
                }
            }

            verticesList.Add(new float[] { 0, -1, 0 });

            float[,] vertices = new float[verticesList.Count, 3];
            for (int i = 0; i < verticesList.Count; i++)
            {
                vertices[i, 0] = verticesList[i][0];
                vertices[i, 1] = verticesList[i][1];
                vertices[i, 2] = verticesList[i][2];
            }

            List<int[]> facesList = new List<int[]>();

            for (int sector = 0; sector < sectors; sector++)
            {
                int current = 1 + sector;
                int next = 1 + (sector + 1) % sectors;
                facesList.Add(new int[] { 0, current, next });
            }

            for (int ring = 0; ring < rings - 2; ring++)
            {
                for (int sector = 0; sector < sectors; sector++)
                {
                    int current = 1 + ring * sectors + sector;
                    int next = 1 + ring * sectors + (sector + 1) % sectors;
                    int currentNext = 1 + (ring + 1) * sectors + sector;
                    int nextNext = 1 + (ring + 1) * sectors + (sector + 1) % sectors;

                    facesList.Add(new int[] { current, currentNext, nextNext, next });
                }
            }

            int bottomPole = verticesList.Count - 1;
            int lastRingStart = 1 + (rings - 2) * sectors;
            for (int sector = 0; sector < sectors; sector++)
            {
                int current = lastRingStart + sector;
                int next = lastRingStart + (sector + 1) % sectors;
                facesList.Add(new int[] { current, bottomPole, next });
            }

            var controlMesh = PNSControlMesh.FromData(vertices, facesList.ToArray());

            string testFilename = $"test_lowpoly_sphere_{System.DateTime.Now.Ticks}";
            string outputPath = Path.Combine(exportPath, $"{testFilename}.{exportFormat}");

            switch (exportFormat.ToLower())
            {
                case "bv":
                    PNS.CreateBV(controlMesh, outputPath, degreeRaise);
                    break;
                case "igs":
                    PNS.CreateIGS(controlMesh, outputPath, degreeRaise);
                    break;
                case "step":
                    PNS.CreateSTEP(controlMesh, outputPath, degreeRaise);
                    break;
            }

            Debug.Log($"Low-poly sphere exported to: {outputPath}");
            controlMesh.Dispose();

            DebugGeneratedFile($"{testFilename}.{exportFormat}");

        }
        catch (System.Exception e)
        {
            Debug.LogError($"Low-poly sphere test failed: {e.Message}");
        }
    }

    public void TestPNSWithFacetedTorus()
    {
        try
        {
            Debug.Log("=== Testing PNS with Faceted Torus ===");

            // Torus params
            float majorRadius = 1.0f;
            float minorRadius = 0.3f;
            int majorSteps = 8; // Around major circumference
            int minorSteps = 6; // Around minor circumference

            List<float[]> verticesList = new List<float[]>();

            // Generate torus vertices
            for (int i = 0; i < majorSteps; i++)
            {
                float majorAngle = (float)i / majorSteps * 2.0f * Mathf.PI;
                float majorX = Mathf.Cos(majorAngle);
                float majorZ = Mathf.Sin(majorAngle);

                for (int j = 0; j < minorSteps; j++)
                {
                    float minorAngle = (float)j / minorSteps * 2.0f * Mathf.PI;
                    float minorRadius_current = minorRadius + minorRadius * 0.3f * Mathf.Cos(minorAngle);

                    float x = (majorRadius + minorRadius_current * Mathf.Cos(minorAngle)) * majorX;
                    float y = minorRadius_current * Mathf.Sin(minorAngle);
                    float z = (majorRadius + minorRadius_current * Mathf.Cos(minorAngle)) * majorZ;

                    verticesList.Add(new float[] { x, y, z });
                }
            }

            // Convert to arr
            float[,] vertices = new float[verticesList.Count, 3];
            for (int i = 0; i < verticesList.Count; i++)
            {
                vertices[i, 0] = verticesList[i][0];
                vertices[i, 1] = verticesList[i][1];
                vertices[i, 2] = verticesList[i][2];
            }

            // Create quad faces
            List<int[]> facesList = new List<int[]>();
            for (int i = 0; i < majorSteps; i++)
            {
                for (int j = 0; j < minorSteps; j++)
                {
                    int current = i * minorSteps + j;
                    int nextMajor = ((i + 1) % majorSteps) * minorSteps + j;
                    int nextMinor = i * minorSteps + ((j + 1) % minorSteps);
                    int nextBoth = ((i + 1) % majorSteps) * minorSteps + ((j + 1) % minorSteps);

                    // Create quad face
                    facesList.Add(new int[] { current, nextMinor, nextBoth, nextMajor });
                }
            }

            var controlMesh = PNSControlMesh.FromData(vertices, facesList.ToArray());

            string testFilename = $"test_faceted_torus_{System.DateTime.Now.Ticks}";
            string outputPath = Path.Combine(exportPath, $"{testFilename}.{exportFormat}");

            switch (exportFormat.ToLower())
            {
                case "bv":
                    PNS.CreateBV(controlMesh, outputPath, degreeRaise);
                    break;
                case "igs":
                    PNS.CreateIGS(controlMesh, outputPath, degreeRaise);
                    break;
                case "step":
                    PNS.CreateSTEP(controlMesh, outputPath, degreeRaise);
                    break;
            }

            Debug.Log($"Faceted torus exported to: {outputPath}");
            controlMesh.Dispose();

            DebugGeneratedFile($"{testFilename}.{exportFormat}");

        }
        catch (System.Exception e)
        {
            Debug.LogError($"Faceted torus test failed: {e.Message}");
        }
    }

    public void TestPNSWithSimpleCube()
    {
        try
        {
            Debug.Log("=== Testing PNS with Simple Cube ===");

            float[,] vertices = new float[,]
            {
            { -1, -1, -1 },
            {  1, -1, -1 },
            {  1, -1,  1 },
            { -1, -1,  1 },
            { -1,  1, -1 },
            {  1,  1, -1 },
            {  1,  1,  1 },
            { -1,  1,  1 }
            };

            List<int[]> facesList = new List<int[]>
        {
            new int[] { 0, 1, 2, 3 }, // bottom
            new int[] { 4, 7, 6, 5 }, // top
            new int[] { 0, 4, 5, 1 }, // back
            new int[] { 2, 6, 7, 3 }, // front
            new int[] { 0, 3, 7, 4 }, // left
            new int[] { 1, 5, 6, 2 }  // right
        };

            var controlMesh = PNSControlMesh.FromData(vertices, facesList.ToArray());

            string testFilename = $"test_cube_{System.DateTime.Now.Ticks}";
            string outputPath = Path.Combine(exportPath, $"{testFilename}.{exportFormat}");

            Debug.Log($"Attempting to export cube with {vertices.GetLength(0)} vertices and {facesList.Count} faces");

            switch (exportFormat.ToLower())
            {
                case "bv":
                    Debug.Log("Creating BV file...");
                    PNS.CreateBV(controlMesh, outputPath, degreeRaise);
                    break;
                case "igs":
                    Debug.Log("Creating IGS file...");
                    PNS.CreateIGS(controlMesh, outputPath, degreeRaise);
                    break;
                case "step":
                    Debug.Log("Creating STEP file...");
                    PNS.CreateSTEP(controlMesh, outputPath, degreeRaise);
                    break;
            }

            Debug.Log($"Cube exported to: {outputPath}");
            controlMesh.Dispose();
            DebugGeneratedFile($"{testFilename}.{exportFormat}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Cube test failed: {e.Message}");
            Debug.LogError($"Stack trace: {e.StackTrace}");
        }
    }

    public void DebugProBuilderMesh(ProBuilderMesh mesh)
    {
        Debug.Log($"=== ProBuilder Mesh Debug ===");
        Debug.Log($"Vertices: {mesh.vertexCount}");
        Debug.Log($"Faces: {mesh.faceCount}");

        for (int i = 0; i < Mathf.Min(5, mesh.faces.Count); i++)
        {
            var face = mesh.faces[i];
            Debug.Log($"Face {i}: {face.indexes.Count} vertices - [{string.Join(", ", face.indexes)}]");
        }
        int quadCount = 0, triCount = 0, otherCount = 0;
        foreach (var face in mesh.faces)
        {
            if (face.indexes.Count == 3) triCount++;
            else if (face.indexes.Count == 4) quadCount++;
            else otherCount++;
        }

        Debug.Log($"Face topology: {triCount} triangles, {quadCount} quads, {otherCount} other");
    }
}