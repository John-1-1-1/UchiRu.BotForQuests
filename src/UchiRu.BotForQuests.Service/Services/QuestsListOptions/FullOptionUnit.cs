namespace UchiRu.BotForQuests.Service.Services.QuestsListOptions; 



public class QuestionOptions {

    public List<BotMessage>? StartMessage{ get; set; }
    public string StartButton { get; set; } = string.Empty;
    public string StartButtonCallback { get; set; } = string.Empty;
    public List<TextQuestUnit>? QuestUnits { get; set; }
    
    public string EndMessage { get; set; } = string.Empty;
}

public class BotMessage {

    public string Text { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public string Button { get; set; } = string.Empty;
    public string ButtonCallback { get; set; } = string.Empty;
}



public class TextQuestUnit {
    
    public string Question { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public List<string> TrueAnswers { get; set; } = new List<string>();
    public bool IsTrueAnswer(string answer) {
        return TrueAnswers.Contains(answer);
    }
}
