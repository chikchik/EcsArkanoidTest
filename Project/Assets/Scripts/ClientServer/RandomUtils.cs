using System;

public static class RandomUtils
{
    public static float Range(this Random random, float min, float max)
    {
        var range = max - min;
        var randomValue = random.NextDouble();
        var rangedValue = randomValue * range + min;
        return (float)rangedValue;
    }
}