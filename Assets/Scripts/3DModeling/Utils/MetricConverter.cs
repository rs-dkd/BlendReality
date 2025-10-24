using UnityEngine;


public enum GridUnitSystem
{
    Metric,
    Imperial
}
public enum ImperialDisplayMode
{
    DecimalFeet,
    Adaptive
}

public static class MetricConverter
{
    private const float InchesPerMeter = 39.3701f;
    private const int InchesPerFoot = 12;

    public static string ToFeetAndInches(float meters)
    {
        if (meters <= 0)
        {
            return "0\"";
        }

        float totalInches = meters * InchesPerMeter;

        if (totalInches < InchesPerFoot)
        {
            return $"{Mathf.RoundToInt(totalInches)}\"";
        }
        else
        {
            int feet = Mathf.FloorToInt(totalInches / InchesPerFoot);
            int inches = Mathf.RoundToInt(totalInches % InchesPerFoot);

            if (inches == InchesPerFoot)
            {
                feet++;
                inches = 0;
            }

            return $"{feet}' {inches}\"";
        }
    }
}