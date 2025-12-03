using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class ManagerTests
{
    private GameObject _managerGO;

    [TearDown]
    public void Teardown()
    {
        if (_managerGO != null) Object.Destroy(_managerGO);
    }

    // --- VIEW MANAGER TESTS ---

    [UnityTest]
    public IEnumerator ViewManager_Singleton_Initializes()
    {
        _managerGO = new GameObject("ViewManager");
        // Add dependencies needed for Awake
        var sm = _managerGO.AddComponent<ViewManager>();

        // Inject dependencies using Reflection because fields are private
        InjectDependency(sm, "shadingToggleGroup", _managerGO.AddComponent<ToggleGroupUI>());
        InjectDependency(sm, "unitSystemToggleGroup", _managerGO.AddComponent<ToggleGroupUI>());
        InjectDependency(sm, "controlPointSizeSlider", _managerGO.AddComponent<SliderUI>());
        InjectDependency(sm, "backfaceToggle", _managerGO.AddComponent<Toggle>());

        // Wait for Awake
        yield return null;

        Assert.IsNotNull(ViewManager.Instance);
        Assert.AreEqual(sm, ViewManager.Instance);
    }

    [UnityTest]
    public IEnumerator ViewManager_ControlPointSize_Updates()
    {
        _managerGO = new GameObject("ViewManager");
        var vm = _managerGO.AddComponent<ViewManager>();

        // Setup Mocks
        InjectDependency(vm, "shadingToggleGroup", _managerGO.AddComponent<ToggleGroupUI>());
        InjectDependency(vm, "unitSystemToggleGroup", _managerGO.AddComponent<ToggleGroupUI>());
        InjectDependency(vm, "controlPointSizeSlider", _managerGO.AddComponent<SliderUI>());
        InjectDependency(vm, "backfaceToggle", _managerGO.AddComponent<Toggle>());

        yield return null; // Allow Awake

        // Act
        float testSize = 0.5f;
        vm.ControlPointSizeUpdated(testSize);

        // Assert
        Assert.AreEqual(testSize, vm.GetControlPointSize());
    }

    // --- PANEL MANAGER TESTS ---

    [UnityTest]
    public IEnumerator PanelManager_OpensCorrectPanel_AndClosesOthers()
    {
        _managerGO = new GameObject("PanelManager");
        var pm = _managerGO.AddComponent<PanelManager>();

        // Create dummy panels
        var panel1GO = new GameObject("LeftPanel1");
        var p1 = panel1GO.AddComponent<UIPanel>();
        InjectDependency(p1, "panel", new GameObject("P1_Content"));

        var panel2GO = new GameObject("LeftPanel2");
        var p2 = panel2GO.AddComponent<UIPanel>();
        InjectDependency(p2, "panel", new GameObject("P2_Content"));

        // Inject array into Manager
        SetPrivateField(pm, "leftSidePanels", new UIPanel[] { p1, p2 });
        SetPrivateField(pm, "rightSidePanels", new UIPanel[0]);
        SetPrivateField(pm, "centerSidePanels", new UIPanel[0]);

        yield return null; // Run Start() to set DockSides

        // ACT: Open Panel 1
        pm.OpenPanel(p1);

        // Wait for logic
        yield return null;

        // ASSERT
        // We check if the internal "panel" GameObject is active
        GameObject p1Content = (GameObject)GetPrivateField(p1, "panel");
        GameObject p2Content = (GameObject)GetPrivateField(p2, "panel");

        Assert.IsTrue(p1Content.activeSelf, "Panel 1 should be open");
        Assert.IsFalse(p2Content.activeSelf, "Panel 2 should be closed");
    }

    // --- HELPER FUNCTIONS FOR REFLECTION ---
    private void InjectDependency(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null) field.SetValue(target, value);
    }

    private void SetPrivateField(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null) field.SetValue(target, value);
    }

    private object GetPrivateField(object target, string fieldName)
    {
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        return field.GetValue(target);
    }
}