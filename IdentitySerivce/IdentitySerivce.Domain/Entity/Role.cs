namespace IdentitySerivce.Domain.Entity;

public class Role : IdentityRole<Guid>
{
    public Role()
    {
        Id = Guid.NewGuid();
    }
}