using Interview.Domain.Users;

namespace Interview.Domain.Repository;

public abstract class Entity
{
    public Entity()
    {
    }

    public Entity(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; set; }

    public DateTime CreateDate { get; internal set; }

    public DateTime UpdateDate { get; internal set; }

    public Guid? CreatedById { get; set; }

    public User? CreatedBy { get; set; }

    public void UpdateCreateDate(DateTime dateTime)
    {
        if (CreateDate == default)
        {
            CreateDate = dateTime;
        }

        if (UpdateDate == default)
        {
            UpdateDate = dateTime;
        }
    }

    public void UpdateUpdateDate(DateTime dateTime)
    {
        UpdateDate = dateTime;
    }
}
