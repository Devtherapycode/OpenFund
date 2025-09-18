using Microsoft.AspNetCore.Mvc;
using MediatR;

namespace OpenFund.Api.Controllers;

/// <summary>
/// Base Controller
/// </summary>
[ApiController]
[Produces("application/json")]
public class ApiControllerBase : ControllerBase
{
    /// <summary>
    /// IMediator
    /// </summary>
    protected readonly IMediator Mediator;

    /// <summary>
    /// ApiControllerBase Constructor
    /// </summary>
    /// <param name="mediator"></param>
    public ApiControllerBase(IMediator mediator)
    {
        Mediator = mediator;
    }
}