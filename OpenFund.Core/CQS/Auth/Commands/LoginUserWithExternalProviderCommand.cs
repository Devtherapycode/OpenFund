using MediatR;
using OpenFund.Core.Common;
using OpenFund.Core.DTOs;

namespace OpenFund.Core.CQS.Auth.Commands;

public record LoginUserWithExternalProviderCommand(string RedirectUri, string Code) : IRequest<Result<AuthTokenDto>>;