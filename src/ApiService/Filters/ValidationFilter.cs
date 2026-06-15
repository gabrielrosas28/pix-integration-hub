using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ApiService.Filters;

/// <summary>
/// Action filter que executa automaticamente os validators do FluentValidation
/// registrados para cada argumento da action. Se houver falhas, lança
/// <see cref="ValidationException"/>, tratada pelo ExceptionMiddleware (HTTP 400).
/// </summary>
public sealed class ValidationFilter : IAsyncActionFilter
{
	private readonly IServiceProvider _serviceProvider;

	public ValidationFilter(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider;
	}

	public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
	{
		var failures = new List<ValidationFailure>();

		foreach (var argument in context.ActionArguments.Values)
		{
			if (argument is null)
			{
				continue;
			}

			var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
			var validator = _serviceProvider.GetService(validatorType) as IValidator;

			if (validator is null)
			{
				continue;
			}

			var validationContextType = typeof(ValidationContext<>).MakeGenericType(argument.GetType());
			var validationContext = (IValidationContext)Activator.CreateInstance(validationContextType, argument)!;
			var result = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);

			if (!result.IsValid)
			{
				failures.AddRange(result.Errors.Where(error => error is not null));
			}
		}

		if (failures.Count != 0)
		{
			throw new ValidationException(failures);
		}

		await next();
	}
}
