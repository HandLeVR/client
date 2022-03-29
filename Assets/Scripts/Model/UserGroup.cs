using System.Collections.Generic;
using Newtonsoft.Json;

public class UserGroup
{
    public long id;
    public Permission permission;
    public string name;
    public List<User> users = new List<User>();
    [JsonIgnore] 
    public List<UserGroupTaskAssignment> userGroupTaskAssignments = new List<UserGroupTaskAssignment>();

    // We define alternate setter for userGroupTaskAssignments to avoid serialization.
    // Otherwise we can get an recursion error when sending the user group to the server.
    [JsonProperty("userGroupTaskAssignments")]
    private List<UserGroupTaskAssignment> userGroupTaskAssignmentsAlternateSetter
    {
        // get is intentionally omitted here
        set => userGroupTaskAssignments = value;
    }

    public UserGroup()
    {
    }
}