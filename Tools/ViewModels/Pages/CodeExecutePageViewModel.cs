using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace Tools.ViewModels.Pages;

public partial class CodeExecutePageViewModel : ObservableObject
{
    [ObservableProperty]
    private string _immediateInput = string.Empty;

    [ObservableProperty]
    private string _immediateOutput = string.Empty;

    public IAsyncRelayCommand ExecuteCommand { get; }

    public CodeExecutePageViewModel()
    {
        ExecuteCommand = new AsyncRelayCommand(OnExecuteCommandAsync);
    }

    private async Task OnExecuteCommandAsync()
    {
        try
        {
            var result = await CSharpScript.EvaluateAsync(ImmediateInput, ScriptOptions.Default);
            ImmediateOutput = result?.ToString() ?? "Executed successfully with no result.";
        }
        catch (Exception ex)
        {
            ImmediateOutput = $"Error: {ex.Message}";
        }
    }
}