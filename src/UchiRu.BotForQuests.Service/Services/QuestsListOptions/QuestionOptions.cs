namespace UchiRu.BotForQuests.Service.Services.QuestsListOptions; 


public class QuestionOptions {
    public List<BotMessage> StartMessage { get; set; } = new List<BotMessage>();
    public string StartButton { get; set; } = string.Empty;
    public string StartButtonCallback { get; set; } = string.Empty;
    public List<BotMessage> QuestUnits { get; set; } = new List<BotMessage>();
    public BotMessage EndMessage { get; set; } = new BotMessage();
    public List<KeyboardText> Keyboard { get; set; } = new List<KeyboardText>();
    public string ErrorMessage { get; set; } = string.Empty;
}

public class KeyboardText {
    public string Text { get; set; } = string.Empty;
    public bool IsCommand(string command) => 
        command.ToLower() == Text.ToLower() || Command == command.ToLower();
    public string Command { get; set; } = string.Empty;
}

public class BotMessage {
    public string Text { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public string Button { get; set; } = string.Empty;
    public string File { get; set; } = string.Empty;
    public string ButtonCallback { get; set; } = string.Empty;
    public List<string> TrueAnswers { get; set; } = new List<string>();
    public bool IsTrueAnswer(string answer) {
        return TrueAnswers.Contains(answer);
    }
}

