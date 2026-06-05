using Domain.Exceptions;
using FluentValidation;
using Infrastructure.BankAdapters.Itau;
using Microsoft.AspNetCore.Mvc;

namespace ApiService.Middleware;

public sealed class ExceptionMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<ExceptionMiddleware> _logger;

	public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
	{
		_next = next;
		_logger = logger;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		try
		{
			await _next(context);
		}
		catch (Exception exception)
		{
			await HandleExceptionAsync(context, exception);
		}
	}

	private async Task HandleExceptionAsync(HttpContext context, Exception exception)
	{
		_logger.LogError(exception, "Unhandled exception");

		if (context.Response.HasStarted)
		{
			return;
		}

		var (statusCode, problemDetails) = CreateProblemDetails(context, exception);

		context.Response.Clear();
		context.Response.StatusCode = statusCode;
		context.Response.ContentType = "application/problem+json";

		await context.Response.WriteAsJsonAsync(problemDetails);
	}

	private static (int StatusCode, ProblemDetails ProblemDetails) CreateProblemDetails(HttpContext context, Exception exception)
	{
		return exception switch
		{
			ValidationException validationException => (
				StatusCodes.Status400BadRequest,
				new ValidationProblemDetails(
					validationException.Errors
						.GroupBy(error => error.PropertyName)
						.ToDictionary(
							group => group.Key,
							group => group.Select(error => error.ErrorMessage).ToArray()))
				{
					Title = "Validation failed",
					Status = StatusCodes.Status400BadRequest,
					Detail = "One or more validation errors occurred.",
					Instance = context.Request.Path
				}),

			DomainException or ArgumentException => (
				StatusCodes.Status400BadRequest,
				CreateProblem(context, StatusCodes.Status400BadRequest, "Bad request", exception.Message)),

			KeyNotFoundException => (
				StatusCodes.Status404NotFound,
				CreateProblem(context, StatusCodes.Status404NotFound, "Not found", exception.Message)),

			BankIntegrationException bankException => (
				ResolveBankStatus(bankException),
				CreateProblem(
					context,
					ResolveBankStatus(bankException),
					"Bank integration error",
					exception.Message)),

			NotImplementedException => (
				StatusCodes.Status501NotImplemented,
				CreateProblem(context, StatusCodes.Status501NotImplemented, "Not implemented", exception.Message)),

			_ => (
				StatusCodes.Status500InternalServerError,
				CreateProblem(context, StatusCodes.Status500InternalServerError, "Unexpected error", "An unexpected error occurred."))
		};
	}

	private static ProblemDetails CreateProblem(HttpContext context, int statusCode, string title, string detail)
	{
		return new ProblemDetails
		{
			Title = title,
			Status = statusCode,
			Detail = detail,
			Instance = context.Request.Path
		};
	}

	private static int ResolveBankStatus(BankIntegrationException exception)
	{
		return exception.StatusCode is >= 400 and <= 599
			? exception.StatusCode
			: StatusCodes.Status502BadGateway;
	}
}