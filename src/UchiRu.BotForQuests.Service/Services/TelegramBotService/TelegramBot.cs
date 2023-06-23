using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using UchiRu.BotForQuests.Service.Services.EfDataBaseService;
using UchiRu.BotForQuests.Service.Services.QuestsListOptions;

namespace UchiRu.BotForQuests.Service.Services.TelegramBotService; 

public class TelegramBot :TelegramBotBase{
    private readonly OptionsService _optionsService;

    private readonly DataBaseService _dataBaseService;

    public TelegramBot(
        OptionsService optionsService,
        DataBaseService dataBaseService,
        IConfiguration configuration) : base(configuration)
    {
        _optionsService = optionsService;
        _dataBaseService = dataBaseService;
    }
    
    private async Task CheckResult(string message, int level, long userId, 
        CancellationToken cancellationToken) {
        if (level < 0) {
            return;
        }

        if (level >= _optionsService.CountMessages) {
            return;
        }

        if (_optionsService.IsTrueAnswer(message, level))
        {
            level += 1;
            _dataBaseService.UpdateUser(userId, level);
            var newTextQuestUnit = _optionsService.GetQuestByUserLevel(level);
            
            await SendMessage(
                new BotMessage() { Text = newTextQuestUnit.Question, 
                    Image = newTextQuestUnit.Image 
                }, userId, cancellationToken); 
        }
        else {
            await SendMessage(
                new BotMessage() { Text = "Неверно", 
                }, userId, cancellationToken); 
        }
    }

    public async Task SendMessage(BotMessage message, long userId, CancellationToken cancellationToken) {
        if (message.Image != string.Empty) {
            await SendImageAsync(userId, message, cancellationToken);
        }
        else {
            await SendTextMessageAsync(userId, message, cancellationToken);
        }
    }
    
    public override async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken) {

        Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
        
        if (update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery) {
            var message = _dataBaseService.AddUser(update.CallbackQuery!.From.Id, 0);
            if (message != string.Empty) {
                await SendMessage(new BotMessage() {Text = message},
                    update.CallbackQuery.From.Id, cancellationToken);
            }

            await SendMessage(new BotMessage() {Text = _optionsService.GetQuestByUserLevel(0).Question},
                update.CallbackQuery.From.Id, cancellationToken);
        }
        
        if(update.Type == Telegram.Bot.Types.Enums.UpdateType.Message) {

            var userLevel = _dataBaseService.GetUserLevel(update.Message.From.Id);
            var message = update.Message;
            
            if (message.Text == null) {
                return;
            }
            
            if (message.Text == "/deleteMe") {
                _dataBaseService.DeleteUser(update.Message.From.Id);
            }
            
            if (message.Text.ToLower() == "/start" && userLevel == -1) {
                await SendStartImages(message, cancellationToken);
            }
            else {
                await CheckResult(message.Text.ToLower(), 
                    userLevel, message.Chat.Id, cancellationToken);
            }
        }
    }

    private async Task SendStartImages(Message message, CancellationToken cancellationToken) {
        foreach (var mess in _optionsService.GetStartMessages()) {
            await SendMessage(new BotMessage() {
                Text = mess.Text, Image = mess.Image, Button = mess.Button,
                ButtonCallback = mess.ButtonCallback
            }, message.Chat.Id, cancellationToken);
        }
    }
    
    public override async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception,
        CancellationToken cancellationToken) {
    }
}