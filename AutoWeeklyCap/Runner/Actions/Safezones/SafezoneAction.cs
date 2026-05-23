namespace AutoWeeklyCap.Runner.Actions.Safezones;

public class SafezoneAction : BaseAction
{
    protected override string Name => nameof(SafezoneAction);

    protected override bool Run(params object[] args)
    {
        if (args.Length <= 0 || args[0] is not string character) {
            return AWC.Config.GetSortedSafezones().Any(IsWithinOrGoingToSafezone);
        }

        var options = AWC.Config.GetOrRegisterCharacterOptions(character);
        if (options != null && IsWithinOrGoingToSafezone(options.PreferredSafezone)) {
            return true;
        }

        return AWC.Config.GetSortedSafezones().Any(IsWithinOrGoingToSafezone);
    }

    private static bool IsWithinOrGoingToSafezone(Safezone? safezone)
    {
        return safezone.HasValue && IsWithinOrGoingToSafezone(safezone.Value);
    }

    private static bool IsWithinOrGoingToSafezone(Safezone safezone)
    {
        return safezone.IsOnLocation() || safezone.Invoke();
    }
}
