using System.Collections.Generic;
using Newtonsoft.Json;

public class User
{
    public long id;
    public Permission permission;
    public string userName;
    public string fullName;
    public string password;
    public Role role;
    public bool passwordChanged;
    public string securityQuestion;
    
    [JsonIgnore]
    public List<TaskAssignment> taskAssignments = new();
    [JsonIgnore]
    public List<UserGroup> userGroups = new();
    [JsonIgnore]
    public List<TaskResult> taskResults = new();

    public User()
    {
        
    }

    public User(User user)
    {
        id = user.id;
        permission = user.permission;
        userName = user.userName;
        fullName = user.fullName;
        password = user.password;
        role = user.role;
        passwordChanged = user.passwordChanged;
        securityQuestion = user.securityQuestion;
        taskAssignments = user.taskAssignments.GetRange(0, user.taskAssignments.Count);
        userGroups = user.userGroups.GetRange(0, user.userGroups.Count); 
        taskResults = user.taskResults.GetRange(0, user.taskResults.Count);  
    }

    public enum Role
    {
        Learner,
        RestrictedTeacher,
        Teacher
    }
}
