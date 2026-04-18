namespace AutoWeeklyCap.Enums;

public enum RetainerTrigger
{
    CurrentCharacter = 0,
    AnyCharacter = 1,
    AllCharacters = 2
}

public static class RetainerTriggerExtensions
{
    extension(RetainerTrigger job)
    {
        public string GetName()
        {
            return job switch
            {
                RetainerTrigger.CurrentCharacter => "Current Character",
                RetainerTrigger.AnyCharacter => "Any Character",
                RetainerTrigger.AllCharacters => "All Characters",
                _ => throw new ArgumentOutOfRangeException(nameof(job), job, null)
            };
        }

        public bool IsWithinThreshold()
        {
            if (!AutoRetainerIPC.IsEnabled) {
                return false;
            }

            return job switch
            {
                RetainerTrigger.CurrentCharacter => IsCharacterWithinThreshold(Player.CID),
                RetainerTrigger.AnyCharacter => AWC.Config.Characters.Values.Any(o => IsCharacterWithinThreshold(o.ID)),
                RetainerTrigger.AllCharacters => AWC.Config.Characters.Values.All(o => IsCharacterWithinThreshold(o.ID)),
                _ => throw new ArgumentOutOfRangeException(nameof(job), job, null)
            };
        }
    }

    private static bool IsCharacterWithinThreshold(ulong cid)
    {
        var seconds = AutoRetainerIPC.GetClosestRetainerVentureSecondsRemaining(cid);
        if (!seconds.HasValue) {
            return false;
        }

        return seconds.Value <= AWC.Config.AutoRetainerThreshold;
    }
}
