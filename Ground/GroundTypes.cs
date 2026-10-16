public enum GroundType
{
    Dirt,
    Sand,
    Road,
    Ice,
    Bouncy,
    Slow
}

public static class GroundTypeExtensions
{
    public static float GetSpeedEffect(this GroundType groundType)
    {
        return groundType switch
        {
            GroundType.Dirt => 0.75f,
            GroundType.Sand => 0.5f,
            GroundType.Road => 1f,
            GroundType.Ice => 1f,
            GroundType.Bouncy => 1f,
            GroundType.Slow => 0.1f,
            _ => 1f, // Default speed effect for unknown ground types
        };
    }
}