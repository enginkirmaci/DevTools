using Microsoft.UI.Xaml.Controls;
using Tools.Library.Configuration;

namespace Tools.Views.Windows;

/// <summary>
/// Dialog for editing workspace settings.
/// </summary>
public sealed partial class WorkspaceSettingsDialog : ContentDialog
{
    /// <summary>
    /// Gets the edited workspace settings.
    /// </summary>
    public WorkspacesSettings Settings { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkspaceSettingsDialog"/> class.
    /// </summary>
    /// <param name="currentSettings">The current workspace settings to edit.</param>
    public WorkspaceSettingsDialog(WorkspacesSettings currentSettings)
    {
        InitializeComponent();
        Settings = currentSettings ?? new WorkspacesSettings();
        LoadSettings();
    }

    private void LoadSettings()
    {
        // Load workspace scan folders (array to multi-line text)
        if (Settings.WorkspaceScanFolders != null && Settings.WorkspaceScanFolders.Any())
        {
            WorkspaceScanFoldersTextBox.Text = string.Join(Environment.NewLine, Settings.WorkspaceScanFolders);
        }

        // Load excluded folders (array to multi-line text)
        if (Settings.ExcludedFolders != null && Settings.ExcludedFolders.Any())
        {
            ExcludedFoldersTextBox.Text = string.Join(Environment.NewLine, Settings.ExcludedFolders);
        }

        // Load simple string properties
        GitFolderPatternTextBox.Text = Settings.GitFolderPattern ?? "*.git";
        SolutionFilePatternTextBox.Text = Settings.SolutionFilePattern ?? "*.sln";
        PlatformFolderNameTextBox.Text = Settings.PlatformFolderName ?? "platform";
        VSCodeExecutableTextBox.Text = Settings.VSCodeExecutable ?? "code";
        TerminalExecutableTextBox.Text = Settings.TerminalExecutable ?? "wt";
    }

    /// <summary>
    /// Saves the edited settings back to the Settings property.
    /// </summary>
    public void SaveSettings()
    {
        // Convert multi-line text to array for workspace scan folders
        var scanFoldersText = WorkspaceScanFoldersTextBox.Text?.Trim();
        Settings.WorkspaceScanFolders = !string.IsNullOrWhiteSpace(scanFoldersText)
            ? scanFoldersText.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Select(f => f.Trim())
                .Where(f => !string.IsNullOrWhiteSpace(f))
                .ToArray()
            : Array.Empty<string>();

        // Convert multi-line text to array for excluded folders
        var excludedFoldersText = ExcludedFoldersTextBox.Text?.Trim();
        Settings.ExcludedFolders = !string.IsNullOrWhiteSpace(excludedFoldersText)
            ? excludedFoldersText.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Select(f => f.Trim())
                .Where(f => !string.IsNullOrWhiteSpace(f))
                .ToArray()
            : Array.Empty<string>();

        // Save simple string properties
        Settings.GitFolderPattern = string.IsNullOrWhiteSpace(GitFolderPatternTextBox.Text)
            ? "*.git"
            : GitFolderPatternTextBox.Text.Trim();

        Settings.SolutionFilePattern = string.IsNullOrWhiteSpace(SolutionFilePatternTextBox.Text)
            ? "*.sln"
            : SolutionFilePatternTextBox.Text.Trim();

        Settings.PlatformFolderName = string.IsNullOrWhiteSpace(PlatformFolderNameTextBox.Text)
            ? "platform"
            : PlatformFolderNameTextBox.Text.Trim();

        Settings.VSCodeExecutable = string.IsNullOrWhiteSpace(VSCodeExecutableTextBox.Text)
            ? "code"
            : VSCodeExecutableTextBox.Text.Trim();

        Settings.TerminalExecutable = string.IsNullOrWhiteSpace(TerminalExecutableTextBox.Text)
            ? "wt"
            : TerminalExecutableTextBox.Text.Trim();
    }
}
