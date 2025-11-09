using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenFund.Core.Entities;

namespace OpenFund.Infrastructure.EntityConfigurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(t => t.Token);

        builder.Property(t => t.Token)
            .IsRequired();

        builder.Property(t => t.Expiration)
            .IsRequired();
    }
}