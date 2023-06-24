using Microsoft.Extensions.Options;
using UchiRu.BotForQuests.Service.Services;
using UchiRu.BotForQuests.Service.Services.EfDataBaseService;
using UchiRu.BotForQuests.Service.Services.QuestsListOptions;
using UchiRu.BotForQuests.Service.Services.TelegramBotService;

namespace UchiRu.BotForQuests.Service;

public class Worker : BackgroundService {
    private readonly ILogger<Worker> _logger;
    private readonly DataBaseService _dataBaseService;
    private readonly IConfiguration _configuration;
    private readonly OptionsService _optionsService;
    private TelegramBot _telegramBot;
    
    public Worker(ILogger<Worker> logger, 
        DataBaseService dataBaseService,
        IConfiguration configuration, 
        OptionsService optionsService,
        TelegramBot telegramBot) {
        _logger = logger;
        _dataBaseService = dataBaseService;
        _configuration = configuration;
        _optionsService = optionsService;
        _telegramBot = telegramBot;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        _telegramBot.Start(stoppingToken);
        
        while (!stoppingToken.IsCancellationRequested) {
            await Task.Delay(1000, stoppingToken);
        }
    }
}

