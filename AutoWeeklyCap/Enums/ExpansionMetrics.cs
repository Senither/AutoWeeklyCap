namespace AutoWeeklyCap.Enums;

public enum ExpansionMetrics
{
    All,
    Dawntrail
}

public static class ExpansionMetricsExtensions
{
    extension(ExpansionMetrics action)
    {
        public string GetName()
        {
            return action switch
            {
                ExpansionMetrics.All => "Everything",
                ExpansionMetrics.Dawntrail => "Dawntrail",
                _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
            };
        }
    }
}
