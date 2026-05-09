using IdentitySerivce.Domain.Entity;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentitySerivce.Infrastructure.EntityConfig;

public class RoleConfig : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("T_Roles");
    }
}