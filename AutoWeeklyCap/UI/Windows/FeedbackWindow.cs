using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

using AutoWeeklyCap.Helpers;

using Dalamud.Interface.Windowing;
using Dalamud.Utility;

namespace AutoWeeklyCap.UI.Windows;

public class FeedbackWindow : Window
{
    private static readonly List<string> FeedbackTypes =
    [
        "General Feedback",
        "Feature Request",
        "Bug Report"
    ];

    private bool _sentFeedback = false;
    private string _type = "-- select your feedback type --";
    private string _message = "";

    public FeedbackWindow() : base("Auto Weekly Tomestone Capper Feedback##feedback-window")
    {
        SizeConstraints = new WindowSizeConstraints { MinimumSize = new Vector2(580, 125), MaximumSize = new Vector2(580, 9999) };

        Flags = ImGuiWindowFlags.AlwaysAutoResize;
    }

    public override void Draw()
    {
        using (Theme.Push()) {
            if (_sentFeedback) {
                DrawFeedbackSentMessage();
            } else {
                DrawFeedbackForm();
            }
        }
    }

    private static void DrawFeedbackSentMessage()
    {
        ImGuiEx.TextCentered("Your feedback has been sent!");
        ImGuiEx.TextCentered("Thanks for contributing to the development of AWC");
    }

    private void DrawFeedbackForm()
    {
        ImGui.TextWrapped("Send your feedback about AWC directly to the developer, your message will be sent to a Discord webhook along with your character name.");
        ImGui.TextWrapped("If you're making a feature request for the plugin and would like to be credited if it's implemented, please include the name you want to show up in the credits within your message.");

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.Text("Feedback Type");

        if (ImGui.BeginCombo("###feedback-type", _type)) {
            foreach (var option in FeedbackTypes) {
                if (ImGui.Selectable(option, option == _type)) {
                    _type = option;
                }
            }

            ImGui.EndCombo();
        }

        ImGui.Text("Message");
        ImGuiEx.InputTextWrapMultilineExpanding("###feedback-message", ref _message, 1000, 6, 20);
        ImGui.Text($"{_message.Length} / 1000");

        if (!ImGui.Button("Send Feedback")) {
            return;
        }

        if (ValidateFormData()) {
            SendFeedback();
        }
    }

    private bool ValidateFormData()
    {
        if (!FeedbackTypes.Contains(_type)) {
            Notify.Warning("You must select a feedback type.");
            return false;
        }

        if (_message.Length < 30) {
            Notify.Warning("The message is too short, it must be at least 30 characters.");
            return false;
        }

        return true;
    }

    private void SendFeedback()
    {
        if (Secrets.DiscordWebhookUrl.IsNullOrWhitespace()) {
            Notify.Warning("Feedback URL is missing, unable to send feedback at this time.");
            return;
        }

        AWC.TaskManager.Enqueue(async void () =>
        {
            var character = PlayerHelper.GetFullCharacterName() ?? "<unknown>";
            var debugReport = DebugReportHelper.GenerateReport(prettyPrint: true) ?? "{}";

            // ReSharper disable once ShortLivedHttpClient
            using var client = new HttpClient();
            var payload = new { embeds = new[] { new { title = _type, description = $"**Version:**\nv{AWC.Version}\n\n**Character:**\n{character}\n\n**Message:**\n{_message}" } } };

            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"), "payload_json");

            var debugReportContent = new ByteArrayContent(Encoding.UTF8.GetBytes(debugReport));
            debugReportContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            content.Add(debugReportContent, "files[0]", "debug-details.json");

            await client.PostAsync(Secrets.DiscordWebhookUrl, content);
        });

        _sentFeedback = true;
    }
}
