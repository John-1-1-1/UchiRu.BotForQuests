using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model;

namespace UchiRu.BotForQuests.Service.Services.VkBotService; 

public class VkBot {
    private readonly IConfiguration _configuration;

    private VkApi api = new VkApi();
    
    public VkBot(IConfiguration configuration) {
        _configuration = configuration;
    }

    public void Authorize() {
        api.Authorize(new ApiAuthParams()
        {
            Login = _configuration.GetSection("VkBot:Login").Value, 
            Password = _configuration.GetSection("VkBot:Password").Value,
            ApplicationId = Convert.ToUInt64(
                _configuration.GetSection("VkBot:ApplicationId").Value),
            Settings = Settings.All
        });
        //api.Authorize(new ApiAuthParams(token));
    }
}