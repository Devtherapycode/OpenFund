using MediatR;
using OpenFund.Core.DTOs;

namespace OpenFund.Core.CQS.Auth.Commands;

public record RegisterUserCommand(UserRegistrationDto Model) : IRequest; 
