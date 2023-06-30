using UchiRu.BotForQuests.Service.Services.TelegramBotService;

namespace UchiRu.BotForQuests.Service;

public class Worker : BackgroundService {
    private TelegramBot _telegramBot;
    
    public Worker(TelegramBot telegramBot) {
        _telegramBot = telegramBot;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        _telegramBot.Start(stoppingToken);
        
        while (!stoppingToken.IsCancellationRequested) {
            await Task.Delay(1000, stoppingToken);
        }
    }
}

