using System.ClientModel;
using System.Text.Json;
using Azure.AI.OpenAI;
using Newtonsoft.Json.Linq;
using OpenAI.Chat;
using Prism.Commands;
using Prism.Mvvm;

namespace Tools.ViewModels.Pages;

public class AIPlaygroundViewModel : BindableBase
{
    private string chatMessage;
    private ObservableCollection<string> chatResponse;
    private AzureOpenAITaskManager azureOpenAITaskManager;

    public string ChatMessage { get => chatMessage; set => SetProperty(ref chatMessage, value); }
    public ObservableCollection<string> ChatResponse { get => chatResponse; set => SetProperty(ref chatResponse, value); }

    public DelegateCommand ChatCommand { get; private set; }

    public AIPlaygroundViewModel()
    {
        ChatCommand = new DelegateCommand(ChatCommandMethod);

        _ = InitializeAsync();
    }

    public async Task InitializeAsync()
    {
        ChatResponse = new ObservableCollection<string>();

        azureOpenAITaskManager = new AzureOpenAITaskManager(
            "https://softtech-openai-kosvtk-gmy.openai.azure.com",
            "bec861eff9934fe29cca28016a91b736",
            "Analizgbt4o");
    }

    private async void ChatCommandMethod()
    {
        ChatResponse.Insert(0, ChatMessage);
        ChatResponse.Insert(0, await azureOpenAITaskManager.ProcessUserInput(ChatMessage));
    }
}

public class AzureOpenAITaskManager
{
    private readonly AzureOpenAIClient client;
    private readonly string deploymentName;

    public AzureOpenAITaskManager(string endpoint, string key, string deploymentName)
    {
        this.deploymentName = deploymentName;
        client = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(key));
    }

    public async Task<string> ProcessUserInput(string userInput)
    {
        var prompt = $" JSON object containing 'taskType', 'parameters', 'response': {userInput}";

        var response = await SendAzureOpenAIRequest(prompt);

        if (!string.IsNullOrEmpty(response))
        {
            try
            {
                var jsonResponse = JObject.Parse(response);
                var taskType = jsonResponse["taskType"].ToString();
                var parameters = jsonResponse["parameters"].ToString();

                switch (taskType)
                {
                    case "CreateBackupTask":
                        return await CreateBackupTask(parameters);

                    case "CreateCleanupTask":
                        return await CreateCleanupTask(parameters);

                    default:
                        return $"I'm not sure how to handle the task: {taskType}";
                }
            }
            catch (JsonException)
            {
                return "I couldn't understand the response. Please try rephrasing your request.";
            }
        }

        return "I'm sorry, I couldn't process your request at this time.";
    }

    private async Task<string> SendAzureOpenAIRequest(string prompt)
    {
        try
        {
            var chatCompletionsOptions = new ChatCompletionOptions()
            {
                ResponseFormat = ChatResponseFormat.JsonObject,

                //Messages =
                //    {
                //        new ChatMessage(ChatRole.System, "You are a helpful assistant that interprets user requests for task automation."),
                //        new ChatMessage(ChatRole.User, prompt)
                //    },
                MaxTokens = 100
            };

            ChatClient chatClient = client.GetChatClient(deploymentName);

            ChatCompletion completion = chatClient.CompleteChat(
            [
                // System messages represent instructions or other guidance about how the assistant should behave
                new SystemChatMessage("You are a helpful assistant that interprets user requests for task automation. options with arguments: CreateBackupTask[{daily,weekly,montly},{folder}], CreateCleanupTask[drive{c,d,e}]"),
                // User messages represent user input, whether historical or the most recen tinput
                new UserChatMessage(prompt)
            ], chatCompletionsOptions);

            return completion.Content[0].Text;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in SendAzureOpenAIRequest: {ex.Message}");
            return null;
        }
    }

    private async Task<string> CreateBackupTask(string parameters)
    {
        // Parse parameters and create backup task
        // This is a placeholder - you would implement actual task creation logic here
        return $"Backup task created with parameters: {parameters}";
    }

    private async Task<string> CreateCleanupTask(string parameters)
    {
        // Parse parameters and create cleanup task
        // This is a placeholder - you would implement actual task creation logic here
        return $"Cleanup task created with parameters: {parameters}";
    }
}

public class AutomatedTasks
{
    public static async Task<string> Execute(string taskName)
    {
        switch (taskName)
        {
            case "DailyBackup":
                return await PerformDailyBackup();

            case "WeeklySystemCleanup":
                return await PerformWeeklySystemCleanup();

            default:
                return $"Unknown task: {taskName}";

                /*
                 ```json
                {
                  "taskType": "scheduleBackup",
                  "parameters": {
                    "date": "tomorrow"
                  }
                }
                ```
                */
        }
    }

    private static async Task<string> PerformDailyBackup()
    {
        return "Daily backup";
    }

    private static async Task<string> PerformWeeklySystemCleanup()
    {
        return "Weekly system cleanup.";
    }
}