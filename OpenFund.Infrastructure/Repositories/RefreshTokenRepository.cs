using Microsoft.EntityFrameworkCore;
using OpenFund.Core.Entities;
using OpenFund.Core.Interfaces.Repositories;
using OpenFund.Infrastructure.Context;

namespace OpenFund.Infrastructure.Repositories;

public class RefreshTokenRepository : BaseRepository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(AppDbContext dbContext) : base(dbContext) { }
}