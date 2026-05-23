namespace AutoWeeklyCap.Helpers;

public static class LevelingHelper
{
    public static string? GetCharacterToLevel()
    {
        var candidates = AWC.Config.LevelJobs.UseCharacterOrder
            ? AWC.Config.GetSortedCharacters()
            : [AWC.Config.LevelJobs.SelectedCharacter];

        foreach (var character in candidates) {
            var characterOptions = AWC.Config.GetOrRegisterCharacterOptions(character);
            if (characterOptions == null) {
                continue;
            }

            if (!AWC.Config.LevelJobs.CharacterJobs.TryGetValue(character, out var jobEntries)) {
                continue;
            }

            var hasLevelableJob = jobEntries.Any(entry =>
                entry.Enabled
                && entry.Job != PlayerJob.None
                && characterOptions.JobLevels.TryGetValue(entry.Job, out var level)
                && IsWithingLevelingThreshold(level)
            );

            if (hasLevelableJob) {
                return character;
            }
        }

        return null;
    }

    public static PlayerJob? GetJobToLevel(string character)
    {
        if (!AWC.Config.LevelJobs.CharacterJobs.TryGetValue(character, out var jobEntries)) {
            return null;
        }

        var characterOptions = AWC.Config.GetOrRegisterCharacterOptions(character);
        if (characterOptions == null) {
            return null;
        }

        foreach (var entry in jobEntries) {
            if (entry.Enabled
                && entry.Job != PlayerJob.None
                && characterOptions.JobLevels.TryGetValue(entry.Job, out var level)
                && IsWithingLevelingThreshold(level)) {
                return entry.Job;
            }
        }

        return null;
    }

    public static bool IsWithingLevelingThreshold(int level)
    {
        return level is >= 15 and < Constants.CurrentMaxLevel;
    }
}
