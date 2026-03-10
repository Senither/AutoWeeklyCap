using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace AutoWeeklyCap.UI.Helpers;

public static class FileSelector
{
    internal const string AudioFilter = "Select audio track|*.3g2;*.3gp;*.3gp2;*.3gpp;*.asf;*.wma;*.wmv;*.aac;*.adts;*.avi;*.mp3;*.m4a;*.m4v;*.mov;*.mp4;*.sami;*.smi;*.wav;*.aiff";

    private static readonly ConcurrentDictionary<string, bool> OpenDialogs = new();
    private static readonly ConcurrentQueue<(string id, string? selectedPath)> Results = [];

    /// <summary>
    /// Draws a button that opens a native file selection dialog. When a file is selected,
    /// <paramref name="path"/> is updated with the selected full file path.
    /// </summary>
    /// <param name="id">Unique id for this selector instance (ImGui ID + dialog tracking).</param>
    /// <param name="path">Reference to the string to update when the user selects a file.</param>
    /// <param name="buttonLabel">Button text (visible).</param>
    /// <param name="dialogTitle">Dialog title.</param>
    /// <param name="filter">WinForms filter string, e.g. "JSON (*.json)|*.json|All files (*.*)|*.*".</param>
    /// <param name="defaultDirectory">Optional fallback initial directory if <paramref name="path"/> is empty.</param>
    /// <returns>True if <paramref name="path"/> changed this draw call.</returns>
    public static bool Draw(
        string id,
        ref string path,
        string buttonLabel = "Browse",
        string dialogTitle = "Select a file",
        string filter = "All files (*.*)|*.*",
        string? defaultDirectory = null)
    {
        var changed = false;

        // Apply any selection results queued from the dialog thread.
        while (Results.TryPeek(out var result)) {
            if (result.id != id) {
                break;
            }

            Results.TryDequeue(out result);
            if (!string.IsNullOrWhiteSpace(result.selectedPath) && !string.Equals(path, result.selectedPath, StringComparison.Ordinal)) {
                path = result.selectedPath;
                changed = true;
            }
        }

        if (ImGui.Button($"{buttonLabel}###awc-file-selector-{id}")) {
            OpenFileDialogAsync(id, dialogTitle, filter, path, defaultDirectory);
        }

        return changed;
    }

    private static void OpenFileDialogAsync(
        string id,
        string dialogTitle,
        string filter,
        string currentPath,
        string? defaultDirectory)
    {
        if (!OpenDialogs.TryAdd(id, true)) {
            return;
        }

        var thread = new Thread(() =>
        {
            try {
                using var dialog = new OpenFileDialog();

                dialog.Title = dialogTitle;
                dialog.Filter = filter;
                dialog.CheckFileExists = true;
                dialog.CheckPathExists = true;
                dialog.RestoreDirectory = true;

                var initialDirectory = GetInitialDirectory(currentPath, defaultDirectory);
                if (!string.IsNullOrWhiteSpace(initialDirectory)) {
                    dialog.InitialDirectory = initialDirectory;
                }

                Results.Enqueue(
                    dialog.ShowDialog() == DialogResult.OK
                        ? (id, dialog.FileName)
                        : (id, null)
                );
            } catch (Exception e) {
                try {
                    AWC.Log.Error(e, "FileSelectorHelper: failed to open file dialog");
                } catch {
                    // ignored
                }
            } finally {
                OpenDialogs.TryRemove(id, out _);
            }
        }) { IsBackground = true, Name = $"AWC-FileSelector-{id}" };

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
    }

    private static string? GetInitialDirectory(string currentPath, string? defaultDirectory)
    {
        try {
            if (!string.IsNullOrWhiteSpace(currentPath)) {
                if (File.Exists(currentPath)) {
                    return Path.GetDirectoryName(currentPath);
                }

                if (Directory.Exists(currentPath)) {
                    return currentPath;
                }

                var directory = Path.GetDirectoryName(currentPath);
                if (!string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory)) {
                    return directory;
                }
            }

            if (!string.IsNullOrWhiteSpace(defaultDirectory) && Directory.Exists(defaultDirectory)) {
                return defaultDirectory;
            }
        } catch {
            // ignored
        }

        return null;
    }
}
