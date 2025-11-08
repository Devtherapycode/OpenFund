using MediatR;
using OpenFund.Core.Common;
using OpenFund.Core.DTOs;

namespace OpenFund.Core.CQS.Auth.Commands;

public record LoginUserCommand(UserLoginDto Model) : IRequest<Result<AuthTokenDto>>;