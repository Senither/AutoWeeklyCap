namespace AutoWeeklyCap.Runner.Actions.Safezones;

public class SafezoneAction : BaseAction
{
    protected override string Name => nameof(SafezoneAction);

    protected override bool Run(params object[] args)
    {
        foreach (var safezone in AWC.Config.GetSortedSafezones()) {
            if (safezone.IsOnLocation()) {
                return true;
            }

            if (safezone.Invoke()) {
                return true;
            }
        }

        return false;
    }
}
