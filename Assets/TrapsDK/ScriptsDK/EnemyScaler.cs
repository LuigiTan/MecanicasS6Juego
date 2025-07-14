public static class EnemyScaler
{
    public static int HordeCount => HordeManager.Instance?.HordeCount ?? 0;

    public static float GetHealthMultiplier() => 1f + HordeCount * 0.25f;
    public static float GetDamageMultiplier() => 1f + HordeCount * 0.2f;
    public static float GetSpeedMultiplier() => 1f + HordeCount * 0.1f;
}
