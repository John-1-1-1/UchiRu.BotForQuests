namespace UchiRu.BotForQuests.Service.Services.EfDataBaseService; 

public class DataBaseService {
    private readonly UsersContext _usersContext;

    public DataBaseService(UsersContext usersContext) {
        _usersContext = usersContext;
    }


    public string AddUser(long fromId, int level) {
        var user = _usersContext.User.FirstOrDefault(u => u.UserId == fromId);
        
        if (user == null) {
            _usersContext.User.Add(new User() 
                {  UserId = fromId, Level = level});
        }
        else { 
            return "Вы уже проходите квест!";
        }
        _usersContext.SaveChanges();
        return string.Empty;
    }

    public void UpdateUser(long fromId, int level) {
        var user = _usersContext.User.FirstOrDefault(u => u.UserId == fromId);

         if (user.Level <= level) { 
             user.Level = level; 
             _usersContext.User.Update(user);
             _usersContext.SaveChanges();
         }
        
    }
    
    public int GetUserLevel(long fromId) {
        var user = _usersContext.User!.
            FirstOrDefault(u => u.UserId == fromId);
        if (user == null) {
            return -1;
        } 
        return user.Level;
    }

    public void DeleteUser(long id) {
        var user = _usersContext.User!.FirstOrDefault(u => u.UserId == id);
        if (user != null) {
            _usersContext.User.Remove(user);
            _usersContext.SaveChanges();
        }
    }
}
