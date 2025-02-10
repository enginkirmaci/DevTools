using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Prism.Commands;
using Prism.Mvvm;

namespace Tools.ViewModels.Pages
{
    public class CodeExecutePageViewModel : BindableBase
    {
        private string _immediateInput;
        private string _immediateOutput;

        public string ImmediateInput
        {
            get => _immediateInput;
            set => SetProperty(ref _immediateInput, value);
        }

        public string ImmediateOutput
        {
            get => _immediateOutput;
            set => SetProperty(ref _immediateOutput, value);
        }

        public ICommand ExecuteCommand { get; }

        public CodeExecutePageViewModel()
        {
            ExecuteCommand = new DelegateCommand(OnExecuteCommand);
        }

        private async void OnExecuteCommand()
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
}