using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace OpenFund.Core.Behavior;

public class ValidationBehavior<TRequest, TResposne> : IPipelineBehavior<TRequest, TResposne> where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResposne> Handle(
        TRequest request,
        RequestHandlerDelegate<TResposne> next,
        CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            ICollection<IEnumerable<ValidationFailure>> errors = new List<IEnumerable<ValidationFailure>>();
            
            foreach (var validator in _validators)
            {
                var validationResult = await validator.ValidateAsync(context, cancellationToken);
                if (!validationResult.IsValid) errors.Add(validationResult.Errors);                
            }

            var flattenedErrors = errors.SelectMany(e => e);
            if (errors.Any()) throw new ValidationException(flattenedErrors);
        }

        return await next();
    }
}