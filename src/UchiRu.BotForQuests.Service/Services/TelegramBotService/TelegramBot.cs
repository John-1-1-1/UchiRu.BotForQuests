using Telegram.Bot;
using Telegram.Bot.Types;
using UchiRu.BotForQuests.Service.Services.EfDataBaseService;
using UchiRu.BotForQuests.Service.Services.QuestsListOptions;
using static Newtonsoft.Json.JsonConvert;

namespace UchiRu.BotForQuests.Service.Services.TelegramBotService; 

public class TelegramBot: TelegramBotBase{
    private readonly OptionsService _optionsService;
    private readonly DataBaseService _dataBaseService;
    private readonly ILogger<TelegramBot> _logger;
    public TelegramBot(
        OptionsService optionsService, DataBaseService dataBaseService,
        IConfiguration configuration, ILoggerFactory loggerFactory) :
        base(configuration, optionsService, loggerFactory) {
        _optionsService = optionsService;
        _dataBaseService = dataBaseService;
        _logger = loggerFactory.CreateLogger<TelegramBot>();
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
                new BotMessage() { 
                    Text = newTextQuestUnit.Text, 
                    Image = newTextQuestUnit.Image
                }, userId, cancellationToken); 
        }
        else {
            await SendMessage(
                new BotMessage() { Text = _optionsService.ErrorMessage(), 
                }, userId, cancellationToken); 
        }
    }

    public override async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken) {
        _logger.LogInformation(SerializeObject(update));
        
        if (update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery) {
            var userLevel = _dataBaseService.GetUserLevel(update.CallbackQuery.From.Id);
            if (userLevel != -1) {
                return;
            }
            var message = _dataBaseService.AddUser(update.CallbackQuery!.From.Id, 0);
            if (message != string.Empty) {
                await SendMessage(new BotMessage() {Text = message},
                    update.CallbackQuery.From.Id, cancellationToken);
            }

            await SendMessage(new BotMessage() {Text = _optionsService.GetQuestByUserLevel(0).Text},
                update.CallbackQuery.From.Id, cancellationToken);
            
            await AnswerCallbackQueryAsync(update);
        }
        
        if(update.Type == Telegram.Bot.Types.Enums.UpdateType.Message) {
            var userLevel = _dataBaseService.GetUserLevel(update.Message.From.Id);
            var message = update.Message;
            
            if (message.Text == null) {
                return;
            }
    
            
            foreach (var keyboardText in _optionsService.Keyboard) {
                if (keyboardText.IsCommand(message.Text)) {
                    message.Text = keyboardText.Command;
                    await CommandsHandler(message, userLevel, cancellationToken);
                    return;
                }
            }
            
            await CheckResult(message.Text.ToLower(), 
                    userLevel, message.Chat.Id, cancellationToken);
        }
    }

    private async Task CommandsHandler(Message message, int userLevel, CancellationToken cancellationToken) {
        if (message.Text == _listCommands.RestartCommand) {
            _dataBaseService.DeleteUser(message.From.Id);
            await SendStartMassages(message, cancellationToken);
        }

        if (message.Text == _listCommands.ListResultsCommand) {
            var result = _dataBaseService.GetResults();
            var resultInString = string.Join("\n", result.Keys.Select(key =>
                string.Concat("level","[",key.ToString(), "]", "=", result[key].ToString())));
            await SendMessage(new BotMessage() {
                Text = resultInString }, message.Chat.Id, cancellationToken);
        }
            
        if (message.Text == _listCommands.StartCommand && userLevel == -1) {
            await SendStartMassages(message, cancellationToken);
        }

        if (message.Text == _listCommands.ReplayQuestion) {
            var newTextQuestUnit = _optionsService.GetQuestByUserLevel(userLevel);
            
            await SendMessage(
                new BotMessage() { 
                    Text = newTextQuestUnit.Text, 
                    Image = newTextQuestUnit.Image
                }, message.From.Id, cancellationToken); 
        }
    }
    
    private async Task SendStartMassages(Message message, CancellationToken cancellationToken) {
        foreach (var mess in _optionsService.GetStartMessages()) {
            await SendMessage(mess, message.Chat.Id, cancellationToken);
        }
    }
    
    public override Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception,
        CancellationToken cancellationToken) {
        _logger.LogError(exception.Message);
        return Task.CompletedTask;
    }
}