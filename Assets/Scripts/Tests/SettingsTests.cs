using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class SettingsTests
{
    private GameObject _settingsGameObject;
    private ViewManager _viewManager;


    [SetUp]
    public void Setup()
    {
        _settingsGameObject = new GameObject();
        _viewManager = _settingsGameObject.AddComponent<ViewManager>();
    }
    [Test]
    public void ViewManagerControlPointSizeUpdate()
    {
        _viewManager.ControlPointSizeUpdated(2);
        Assert.AreEqual(_viewManager.GetControlPointSize(), 2);

        _viewManager.ControlPointSizeUpdated(2);
    }


    [UnityTest]
    public IEnumerator SettingsTestsWithEnumeratorPasses()
    {

        yield return null;
    }
}
