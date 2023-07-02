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
    
    private async Task CheckResult(string message, int userLevel, long userId, 
        CancellationToken cancellationToken) {
        if (userLevel < 0) {
            return;
        }

        if (userLevel >= _optionsService.CountMessages) {
            return;
        }

        if (_optionsService.IsTrueAnswer(message, userLevel))
        {
            userLevel += 1;
            _dataBaseService.UpdateUser(userId, userLevel);
            
            await SendListMessagesByUserLevel(userLevel, userId, cancellationToken);
        }
        else {
            await SendMessage(
                new BotMessage() { Text = _optionsService.ErrorMessage(), 
                }, userId, cancellationToken); 
        }
    }

    protected override async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken) {
        _logger.LogInformation(SerializeObject(update));
        
        if (update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery) {

            var callbackQuery = update.CallbackQuery;
            if (callbackQuery == null) {
                throw new ArgumentNullException(nameof(callbackQuery));
            }

            var userId = callbackQuery.From.Id;

            var userLevel = _dataBaseService.GetUserLevel(userId);
            
            await AnswerCallbackQueryAsync(update);
            
            if (userLevel != -1) {
                return;
            }
            
            _dataBaseService.AddUser(userId, 0);
            userLevel = _dataBaseService.GetUserLevel(userId);
            await SendListMessagesByUserLevel(userLevel ,userId, cancellationToken);
        }
        
        if(update.Type == Telegram.Bot.Types.Enums.UpdateType.Message) {
            var message = update.Message;
            if (message == null) {
                throw new ArgumentNullException(nameof(message));
            }

            var userId = message.Chat.Id;
            var userLevel = _dataBaseService.GetUserLevel(userId);

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
                    userLevel, userId, cancellationToken);
        }
    }

    private async Task CommandsHandler(Message message, int userLevel, CancellationToken cancellationToken) {
        var userId = message.Chat.Id;
        if (message.Text == _listCommands.RestartCommand) {
            _dataBaseService.DeleteUser(userId);
            await SendListMessagesByUserLevel(_dataBaseService.GetUserLevel(userId)
                , userId, cancellationToken);
        }

        if (message.Text == _listCommands.ListResultsCommand) {
            var result = _dataBaseService.GetResults();
            var resultInString = string.Join("\n", result.Keys.Select(key =>
                string.Concat("level","[",key.ToString(), "]", "=", result[key].ToString())));
            await SendMessage(new BotMessage() {
                Text = resultInString }, message.Chat.Id, cancellationToken);
        }
            
        if (message.Text == _listCommands.StartCommand && userLevel == -1) {
            await SendListMessagesByUserLevel(userLevel, userId, cancellationToken);
        }

        if (message.Text == _listCommands.ReplayQuestion) {
            await SendListMessagesByUserLevel(userLevel, userId, cancellationToken);
        }
    }

    private async Task SendListMessagesByUserLevel(int userLevel, long userId,
        CancellationToken cancellationToken) {
        foreach (var botMessage in _optionsService.GetQuestByUserLevel(userLevel)) {
            await SendMessage(botMessage, userId, cancellationToken); 
        }
    }

    protected override Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception,
        CancellationToken cancellationToken) {
        _logger.LogError(exception.Message);
        return Task.CompletedTask;
    }
}