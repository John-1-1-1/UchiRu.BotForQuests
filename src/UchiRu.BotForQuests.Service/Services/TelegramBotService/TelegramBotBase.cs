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
            _optionsService.Keyboard.
                Where(key => key.Visible != "false").
                Select(key =>
            new List<KeyboardButton>(){(new KeyboardButton(key.Text))}).ToList();
        
        var keyboard = new ReplyKeyboardMarkup(listButtons);
        keyboard.ResizeKeyboard = true;
        
        return  keyboard;
    }

    private async Task<BotMessage> SendTextMessageAsync(long chatId, BotMessage message,
        CancellationToken cancellationToken) {

        await _bot.SendTextMessageAsync(
            chatId: chatId, 
            text: message.Text, 
            cancellationToken: cancellationToken, 
            replyMarkup: GetKeyboardMarkup(message));
        message.Text = string.Empty;
        return message;
    }

    private async Task<BotMessage> SendImageAsync(long chatId, BotMessage message, CancellationToken cancellationToken) {
        
        var fs = System.IO.File.OpenRead(message.Image);
        await _bot.SendPhotoAsync( 
            chatId: chatId,
            photo: InputFile.FromStream(fs),
            caption: message.Text,
            replyMarkup: GetKeyboardMarkup(message),
            cancellationToken: cancellationToken
        );
        message.Text = string.Empty;
        message.Image = string.Empty;
        return message;
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

    private async Task<BotMessage> SendStickerAsync(long chatId, BotMessage message, CancellationToken cancellationToken) {
        InputFile n = InputFile.FromFileId(message.StickerId);
        await _bot.SendStickerAsync(chatId: chatId,
            sticker: n, cancellationToken: cancellationToken);
        message.StickerId = string.Empty;
        return message;
    }

    public async Task SendMessage(BotMessage message, long userId, CancellationToken cancellationToken) {

        message = message.Copy();
        
        if (message.Image != string.Empty) {
            message = await SendImageAsync(userId, message, cancellationToken);
        }
        if (message.StickerId != string.Empty) {
            message = await SendStickerAsync(userId, message, cancellationToken);
        }

        if (message.Text != string.Empty) {
            message = await SendTextMessageAsync(userId, message, cancellationToken);
        }
        
        _logger.LogWarning(SerializeObject(message));
    }

    protected abstract Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken);

    protected abstract Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception,
        CancellationToken cancellationToken);
}