using System;

public class Permission
{
    public long id;
    public string createdByFullName;
    public DateTime createdDate;
    public string lastEditedByFullName;
    public DateTime lastEditedDate;
    public bool editable;

    public Permission()
    {
    }

    public Permission(long id, string createdByFullName, DateTime createdDate, string lastEditedByFullName, DateTime lastEditedDate,
        bool editable)
    {
        this.id = id;
        this.createdByFullName = createdByFullName;
        this.createdDate = createdDate;
        this.lastEditedByFullName = lastEditedByFullName;
        this.lastEditedDate = lastEditedDate;
        this.editable = editable;
    }
}