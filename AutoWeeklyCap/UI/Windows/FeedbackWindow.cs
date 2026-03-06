using System.Net.Http;
using System.Text;
using System.Text.Json;

using Dalamud.Interface.Windowing;

namespace AutoWeeklyCap.UI.Windows;

public class FeedbackWindow : Window
{
    private const string WebhookUrl = "https://discord.com/api/webhooks/1474382411331534868/LBXYJt_hB7xc7OZuXJ3wci5O1EbBSJzS3A-qKzj6UkPCySlNk5Fp7e4BKq8ZCosTQDhM";

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
        if (_sentFeedback) {
            DrawFeedbackSentMessage();
        } else {
            DrawFeedbackForm();
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
        AWC.TaskManager.Enqueue(async void () =>
        {
            var character = PlayerHelper.GetFullCharacterName() ?? "<unknown>";

            using var client = new HttpClient();
            var payload = new { embeds = new[] { new { title = _type, description = $"**Version:**\nv{AWC.Version}\n\n**Character:**\n{character}\n\n**Message:**\n{_message}" } } };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            await client.PostAsync(WebhookUrl, content);
        });

        _sentFeedback = true;
    }
}
