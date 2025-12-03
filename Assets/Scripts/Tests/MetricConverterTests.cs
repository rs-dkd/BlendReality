using NUnit.Framework;
using UnityEngine;

public class MetricConverterTests
{
    [Test]
    public void MetersToFeetAndInches_ConvertsCorrectly()
    {
        // 1 meter is approx 3' 3"
        string result = MetricConverter.ToFeetAndInches(1f);
        Assert.AreEqual("3' 3\"", result);
    }

    [Test]
    public void MetersToFeetAndInches_HandlesZero()
    {
        string result = MetricConverter.ToFeetAndInches(0f);
        Assert.AreEqual("0\"", result);
    }

    [Test]
    public void MetersToFeetAndInches_HandlesNegatives()
    {
        // -1 meter
        string result = MetricConverter.ToFeetAndInches(-1f);
        Assert.AreEqual("-3' 3\"", result);
    }

    [Test]
    public void ImperialToMeters_ParsesStandardFormat()
    {
        // 3 feet 3 inches
        float result = MetricConverter.ToMeters("3' 3\"");
        // Allow a small delta for floating point imprecision
        Assert.AreEqual(0.9906f, result, 0.001f);
    }

    [Test]
    public void ImperialToMeters_ParsesJustInches()
    {
        float result = MetricConverter.ToMeters("12\"");
        Assert.AreEqual(0.3048f, result, 0.001f);
    }

    [Test]
    public void ImperialToMeters_HandlesGarbageInput()
    {
        float result = MetricConverter.ToMeters("Not A Number");
        Assert.AreEqual(0f, result);
    }
}