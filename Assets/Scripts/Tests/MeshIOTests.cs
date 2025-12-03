using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class MeshIOTests
{
    private string testPath;

    [SetUp]
    public void Setup()
    {
        testPath = Path.Combine(Application.persistentDataPath, "UnitTestExport.obj");
    }

    [TearDown]
    public void Cleanup()
    {
        if (File.Exists(testPath)) File.Delete(testPath);
    }

    //[Test]
    //public void MeshExporter_WritesOBJFile()
    //{
    //    // ARRANGE
    //    // Create a dummy cube
    //    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

    //    // Create a Mock ModelData (Defined at bottom of this answer)
    //    ModelData mockModel = cube.AddComponent<ModelData>();
    //    mockModel.SetupModel(cube.GetComponent<MeshFilter>(), "TestCube");

    //    List<ModelData> models = new List<ModelData> { mockModel };

    //    // ACT
    //    MeshExporter.MeshToFile(models, testPath, "TestFile", false);

    //    // ASSERT
    //    Assert.IsTrue(File.Exists(testPath));

    //    string content = File.ReadAllText(testPath);
    //    Assert.IsTrue(content.Contains("o TestCube"), "File should contain object name");
    //    Assert.IsTrue(content.Contains("v "), "File should contain vertices");
    //    Assert.IsTrue(content.Contains("f "), "File should contain faces");

    //    // Cleanup GO
    //    Object.DestroyImmediate(cube);
    //}
}