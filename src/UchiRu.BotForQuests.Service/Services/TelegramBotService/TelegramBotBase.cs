using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using UchiRu.BotForQuests.Service.Services.QuestsListOptions;

namespace UchiRu.BotForQuests.Service.Services.TelegramBotService;

public abstract class TelegramBotBase {
    static ITelegramBotClient _bot;

    public TelegramBotBase(IConfiguration configuration) {
        
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
    
    public async Task SendTextMessageAsync(long chatId, BotMessage message,
        CancellationToken cancellationToken) {
        
        InlineKeyboardMarkup? markup = null;
        
        if (message.Button != string.Empty) {
            markup = new(new[] {
                new[] {
                    InlineKeyboardButton.WithCallbackData(message.Button, message.ButtonCallback)
                }
            });
        } 
        await _bot.SendTextMessageAsync(
            chatId: chatId, 
            text: message.Text, 
            cancellationToken: cancellationToken, 
            replyMarkup: markup);
    }

    public async Task SendImageAsync(long chatId, BotMessage message, CancellationToken cancellationToken) {

        InlineKeyboardMarkup? markup = null;
        
        if (message.Button != string.Empty) {
            markup = new(new[] {
                new[] {
                    InlineKeyboardButton.WithCallbackData(message.Button, message.ButtonCallback)
                }
            });
        }
        var fs = System.IO.File.OpenRead(message.Image);
        await _bot.SendPhotoAsync( 
            chatId: chatId,
            photo: InputFile.FromStream(fs),
            caption: message.Text,
            replyMarkup: markup,
            cancellationToken: cancellationToken
        );
    }

    public abstract Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken);

    public abstract Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception,
        CancellationToken cancellationToken);
}