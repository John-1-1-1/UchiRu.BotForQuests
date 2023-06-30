using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using static Newtonsoft.Json.JsonConvert;
using UchiRu.BotForQuests.Service.Services.QuestsListOptions;

namespace UchiRu.BotForQuests.Service.Services.TelegramBotService;

public abstract class TelegramBotBase {
    private readonly OptionsService _optionsService;
    static ITelegramBotClient _bot = null!;
    private readonly ILogger<TelegramBot> _logger;
    protected readonly ListCommands? _listCommands;
        
    public TelegramBotBase(
        IConfiguration configuration,
        OptionsService optionsService,
        ILoggerFactory loggerFactory) {
        _optionsService = optionsService;
        _logger = loggerFactory.CreateLogger<TelegramBot>();
        _listCommands = configuration.GetSection("ListCommands").Get<ListCommands>();
        string? value = configuration.GetSection("Telegram:Token").Value;
        
        if (value == null) {
            throw new ArgumentNullException(value);
        }
        _bot = new TelegramBotClient(value);
    }
    
    public void Start(CancellationToken cancellationToken) {
        var receiverOptions = new ReceiverOptions {
            AllowedUpdates = { }, // receive all update types
        };

        _bot.StartReceiving(
            HandleUpdateAsync,
            (botClient, exception, cancellationToken1) => HandleErrorAsync(botClient, exception, cancellationToken1),
            receiverOptions,
            cancellationToken
        );
    }

    private IReplyMarkup? GetKeyboardMarkup(BotMessage message) {
        InlineKeyboardMarkup? markup = null;

        if (message.Button != string.Empty) {
            markup = new(new[] {
                new[] {
                    InlineKeyboardButton.WithCallbackData(message.Button, message.ButtonCallback)
                }
            });
            return markup;
        }

        var listButtons =
            _optionsService.Keyboard.Select(key =>
            new List<KeyboardButton>(){(new KeyboardButton(key.Text))}).ToList();
        
        var keyboard = new ReplyKeyboardMarkup(listButtons);
        keyboard.ResizeKeyboard = true;
        
        return  keyboard;
    }

    private async Task SendTextMessageAsync(long chatId, BotMessage message,
        CancellationToken cancellationToken) {

        await _bot.SendTextMessageAsync(
            chatId: chatId, 
            text: message.Text, 
            cancellationToken: cancellationToken, 
            replyMarkup: GetKeyboardMarkup(message));
    }

    private async Task SendImageAsync(long chatId, BotMessage message, CancellationToken cancellationToken) {
        
        var fs = System.IO.File.OpenRead(message.Image);
        await _bot.SendPhotoAsync( 
            chatId: chatId,
            photo: InputFile.FromStream(fs),
            caption: message.Text,
            replyMarkup: GetKeyboardMarkup(message),
            cancellationToken: cancellationToken
        );
    }

    public async Task AnswerCallbackQueryAsync(Update update) {
        var callbackQuery = update.CallbackQuery;
        if (callbackQuery == null) {
            _logger.LogWarning(nameof(callbackQuery) + " null");
        }

        try {
            await _bot.AnswerCallbackQueryAsync(callbackQuery!.Id, null);
        }
        catch (Exception e) {
            _logger.LogWarning(e.Message);
        }
    }

    private async Task SendFileAsync(long chatId, BotMessage message, CancellationToken cancellationToken) {

        var fs = System.IO.File.OpenRead(message.File);
        await _bot.SendDocumentAsync( 
            chatId: chatId,
            document: InputFile.FromStream(fs),
            caption: message.Text,
            cancellationToken: cancellationToken
        );
    }

    public async Task SendMessage(BotMessage message, long userId, CancellationToken cancellationToken) {
        if (message.Image != string.Empty) {
            await SendImageAsync(userId, message, cancellationToken);
            return;
        }
        if (message.File != string.Empty) {
            await SendFileAsync(userId, message, cancellationToken);
            return;
        }

        if (message.Text != string.Empty) {
            await SendTextMessageAsync(userId, message, cancellationToken);
            return;
        }
        
        _logger.LogWarning(SerializeObject(message));
    }

    public abstract Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken);

    public abstract Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception,
        CancellationToken cancellationToken);
}