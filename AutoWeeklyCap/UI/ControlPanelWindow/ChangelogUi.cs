using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

using AutoWeeklyCap.UI.Helpers;

using Dalamud.Interface;

using Newtonsoft.Json;

// ReSharper disable InconsistentNaming

namespace AutoWeeklyCap.UI.ControlPanelWindow;

public static class ChangelogUi
{
    private const string ChangelogUrl = "https://dalamud-plugins.senither.com/changelog/Senither/AutoWeeklyCap.json";
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromMinutes(30);
    private static readonly HttpClient HttpClient = new();

    private static List<ChangelogEntry>? Entries;
    private static string? LoadError;
    private static DateTime? LastFetchUtc;
    private static bool IsFetching;

    private sealed record ChangelogEntry(
        [property: JsonProperty("version")] string Version,
        [property: JsonProperty("changelog")] string Changelog,
        [property: JsonProperty("created_at")] DateTime CreatedAt
    );

    internal static void Draw()
    {
        EnsureChangelogLoaded();

        if (IsFetching) {
            ImGuiEx.LineCentered(() => ImGui.TextDisabled("Loading..."));
        }

        if (Entries is { Count: > 0 }) {
            DrawChangelogEntries(Entries);
        } else {
            DrawEmptyState();
        }
    }

    private static void DrawEmptyState()
    {
        if (!string.IsNullOrWhiteSpace(LoadError)) {
            ImGui.TextColored(Theme.TextDanger, "Failed to load changelog.");
            ImGui.TextWrapped(LoadError);
            return;
        }

        ImGui.TextDisabled(IsFetching ? "Loading changelog..." : "Changelog is not available yet.");
    }

    private static void DrawChangelogEntries(List<ChangelogEntry> entries)
    {
        var IsFirst = true;
        foreach (var entry in entries) {
            Card.Draw($"{entry.Version} ({entry.CreatedAt:yyyy-MM-dd})", () =>
            {
                string? fullChangelogUrl = null;

                foreach (var line in ReadLines(entry.Changelog)) {
                    if (TryGetFullChangelogUrl(line, out var url)) {
                        fullChangelogUrl = url;
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(line)) {
                        ImGui.Spacing();
                        continue;
                    }

                    ImGui.TextWrapped(line);
                }

                if (!string.IsNullOrWhiteSpace(fullChangelogUrl)) {
                    ThemeButton.Draw(
                        FontAwesomeIcon.Code,
                        $"View Full Changelog###changelog-{entry.Version}",
                        fullChangelogUrl
                    );

                    ImGui.SameLine();
                }

                ThemeButton.Draw(
                    FontAwesomeIcon.Receipt,
                    $"View Release###release-{entry.Version}",
                    $"https://github.com/Senither/AutoWeeklyCap/releases/tag/{entry.Version}"
                );

                IsFirst = false;
            }, defaultOpen: IsFirst, id: entry.Version);
        }
    }

    private static void EnsureChangelogLoaded()
    {
        if (IsFetching) {
            return;
        }

        if (Entries != null && LastFetchUtc.HasValue && DateTime.UtcNow - LastFetchUtc.Value < RefreshInterval) {
            return;
        }

        _ = FetchChangelogAsync();
    }

    private static async Task FetchChangelogAsync()
    {
        if (IsFetching) {
            return;
        }

        IsFetching = true;
        LoadError = null;

        try {
            var json = await HttpClient.GetStringAsync(ChangelogUrl).ConfigureAwait(false);
            var entries = JsonConvert.DeserializeObject<List<ChangelogEntry>>(json) ?? [];

            Entries = entries;
            LastFetchUtc = DateTime.UtcNow;
        } catch (Exception ex) {
            LoadError = ex.Message;
        } finally {
            IsFetching = false;
        }
    }

    private static bool TryGetFullChangelogUrl(string line, out string? url)
    {
        const string Prefix = "**Full Changelog**:";
        var trimmedLine = line.Trim();

        if (trimmedLine.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase)) {
            var candidate = trimmedLine[Prefix.Length..].Trim();
            if (Uri.TryCreate(candidate, UriKind.Absolute, out _)) {
                url = candidate;
                return true;
            }
        }

        url = null;
        return false;
    }

    private static IEnumerable<string> ReadLines(string text)
    {
        using var reader = new StringReader(text);

        while (reader.ReadLine() is { } line) {
            yield return line;
        }
    }
}
