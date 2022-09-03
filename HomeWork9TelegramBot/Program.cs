using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

var path = "database.file";
var token = "your token here";
var botClient = new TelegramBotClient($"{token}");
using var cts = new CancellationTokenSource();
// StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
var receiverOptions = new ReceiverOptions
{
    AllowedUpdates = { } // receive all update types
};
botClient.StartReceiving(
    HandleUpdatesAsync,
    HandleErrorAsync,
    receiverOptions,
    cancellationToken: cts.Token);

var me = await botClient.GetMeAsync();

Console.WriteLine($"Start listening for @{me.Username}");
Console.ReadLine();


// Send cancellation request to stop bot
cts.Cancel();
static async void DownloadFile(ITelegramBotClient botClient, string fileId, string path)
{
    try
    {
        var file = await botClient.GetFileAsync(fileId);

        using (var saveImageStream = new FileStream(path, FileMode.Create))
        {
            await botClient.DownloadFileAsync(file.FilePath, saveImageStream);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error downloading: " + ex.Message);
    }
}
async Task HandleUpdatesAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    if (update.Type == UpdateType.Message && update?.Message?.Text != null)
    {
        await HandleMessage(botClient, update.Message);
        return;
    }
    if(update.Type == UpdateType.CallbackQuery)
    {
        await HandleCallbackQuery(botClient, update.CallbackQuery);
        return;
    }
}

async Task HandleMessage(ITelegramBotClient botClient,Message message)
{
    if(message.Text == "/start")
    {
        await botClient.SendTextMessageAsync(message.Chat.Id, "Choose commands: /inline | /keyboard");
    }
    if (message.Text == "/keyboard")
    {
        ReplyKeyboardMarkup keyboard = new(new[]
        {
            new KeyboardButton[] { "Hello", "Search"}
        })
        {
            ResizeKeyboard = true
        };    
        await botClient.SendTextMessageAsync(message.Chat.Id, "Choose:", replyMarkup: keyboard);
        return;
    }
    if(message.Text == "Search")
    {
        await botClient.SendTextMessageAsync(message.Chat.Id, $"You are done");
        return;
    }
    await botClient.SendTextMessageAsync(message.Chat.Id, $"You said:{message.Text}");
}
async Task HandleCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery)
{

}
Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var ErrorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };

    Console.WriteLine(ErrorMessage);
    return Task.CompletedTask; 
}

