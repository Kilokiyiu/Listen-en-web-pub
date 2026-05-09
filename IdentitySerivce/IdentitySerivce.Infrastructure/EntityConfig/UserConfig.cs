using IdentitySerivce.Domain.Entity;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentitySerivce.Infrastructure.EntityConfig;

public class UserConfig : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("T_Users");
    }
}