namespace UchiRu.BotForQuests.Service.Services.QuestsListOptions; 

public class OptionsService {
    private readonly QuestionOptions _questionOptions;

    public OptionsService(QuestionOptions questionOptions) {
        _questionOptions = questionOptions;
    }

    public int CountMessages => _questionOptions.QuestUnits.Count;
    public List<KeyboardText> Keyboard {
        get => _questionOptions.Keyboard;
    }

    public List<BotMessage> GetStartMessages() { 
        int count = _questionOptions.StartMessage.Count;
        var ret = _questionOptions.StartMessage;
        ret[count - 1].Button = _questionOptions.StartButton;
        ret[count - 1].ButtonCallback = _questionOptions.StartButtonCallback;
        return ret;
    }
    
    public BotMessage GetQuestByUserLevel(int level) {
        if (level >= CountMessages) {
            return _questionOptions.EndMessage;
        }

        if (level < 0) {
            return new BotMessage();
        }
        
        return _questionOptions.QuestUnits[level];
    }

    public bool IsTrueAnswer(string answer, int level) => 
        _questionOptions.QuestUnits[level].IsTrueAnswer(answer);
    
    public string ErrorMessage() => _questionOptions.ErrorMessage;
}