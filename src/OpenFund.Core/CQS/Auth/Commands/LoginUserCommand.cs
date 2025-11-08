using MediatR;
using OpenFund.Core.DTOs;

namespace OpenFund.Core.CQS.Auth.Commands;

public record LoginUserCommand(UserLoginDto Model) : IRequest<AuthTokenDto>;