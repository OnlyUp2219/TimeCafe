namespace BuildingBlocks.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators, ILogger<ValidationBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators = validators;
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger = logger;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);
        var failures = new List<FluentValidation.Results.ValidationFailure>();

        foreach (var validator in _validators)
        {
            var result = await validator.ValidateAsync(context, cancellationToken).ConfigureAwait(false);
            if (!result.IsValid)
                failures.AddRange(result.Errors);
        }

        if (failures.Count != 0)
        {
            _logger.LogWarning("Validation failed for {RequestType}. Errors: {Errors}", typeof(TRequest).Name, failures.Select(f => f.ErrorMessage));
            throw new ValidationException(failures);
        }

        return await next();
    }
}