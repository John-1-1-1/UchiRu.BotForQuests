using UchiRu.BotForQuests.Service;
using UchiRu.BotForQuests.Service.Services.EfDataBaseService;
using UchiRu.BotForQuests.Service.Services.QuestsListOptions;
using UchiRu.BotForQuests.Service.Services.TelegramBotService;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddJsonFile("QuestList.json");
builder.Configuration.AddJsonFile("listCommands.json");

var questionOptions = new QuestionOptions();
builder.Configuration.Bind(questionOptions);

builder.Services.AddSingleton<UsersContext>();
builder.Services.AddSingleton<DataBaseService>();
builder.Services.AddSingleton<OptionsService>();
builder.Services.AddSingleton<QuestionOptions>(questionOptions);
builder.Services.AddSingleton<TelegramBot>();
builder.Services.AddHostedService<Worker>();
builder.Services.Configure<QuestionOptions>(
    builder.Configuration.GetSection("QuestionOption"));

var host = builder.Build();

host.Run();
