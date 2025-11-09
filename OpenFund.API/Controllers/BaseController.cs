using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace OpenFund.API.Controllers;

public class BaseController : ControllerBase
{
    protected readonly ILogger<BaseController> _logger;
    protected readonly IMediator _mediator;

    public BaseController(ILogger<BaseController> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }
}