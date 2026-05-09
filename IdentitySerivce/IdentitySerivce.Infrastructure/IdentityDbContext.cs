using IdentitySerivce.Domain.Entity;
using Infrastructure.EFCORE;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace IdentitySerivce.Infrastructure;

public class IdentityDbContext : IdentityDbContext<User, Role, Guid>
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
        builder.EnableSoftDeletionGlobalFilter();
    }
}