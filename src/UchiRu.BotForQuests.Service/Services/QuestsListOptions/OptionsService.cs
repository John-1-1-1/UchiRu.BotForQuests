namespace UchiRu.BotForQuests.Service.Services.QuestsListOptions; 

public class OptionsService {
    private readonly QuestionOptions _questionOptions;

    public OptionsService(QuestionOptions questionOptions) {
        _questionOptions = questionOptions;
    }

    public int CountMessages => _questionOptions.QuestUnits.Count;
    
    public List<BotMessage> GetStartMessages() { 
        int count = _questionOptions.StartMessage.Count;
        var ret = _questionOptions.StartMessage;
        ret[count - 1].Button = _questionOptions.StartButton;
        ret[count - 1].ButtonCallback = _questionOptions.StartButtonCallback;
        return ret;
    }
    
    public TextQuestUnit GetQuestByUserLevel(int level) {
        if (level >= CountMessages)
        {
            return new TextQuestUnit() { Question = _questionOptions.EndMessage};
        }
        return _questionOptions.QuestUnits[level];
    }

    public bool IsTrueAnswer(string answer, int level) {
        return _questionOptions.QuestUnits[level].IsTrueAnswer(answer);
    }
}