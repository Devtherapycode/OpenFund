using Microsoft.AspNetCore.Mvc;
using OpenFund.Core.Common;

namespace OpenFund.API.Infrastructure.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        object responseObject; 
        
        if (result.IsSuccess)
        {
            responseObject = new { data = result.Data };
        }
        else
        {
            responseObject = result.Errors != null
                ? new { message = result.Message, errors = result.Errors }
                : new { message = result.Message };

        }
        
        return new ObjectResult(responseObject) { StatusCode = result.StatusCode };
    }

    public static IActionResult ToActionResult(this Result result)
    {
        object responseObject;

        if (result.IsSuccess)
        {
            responseObject = null!;
        }
        else
        {
            responseObject = result.Errors != null
                ? new { message = result.Message, errors = result.Errors }
                : new { message = result.Message };
        }

        return new ObjectResult(responseObject) { StatusCode = result.StatusCode };
    }
}