using FluentResults;
using Microsoft.Extensions.Logging;

namespace Infrastructure.ResultBased;

public class ResultLogger(ILoggerFactory factory) : IResultLogger
{
    public void Log(string context, string content, ResultBase result, LogLevel logLevel)
    {
        if (result.HasError<ExceptionalError>(out var errors))
        {
            var exception = errors.Select(error => error.Exception).First();
            var logger = factory.CreateLogger(context);
            logger.LogError(exception, "{ExceptionMessage}" ,exception.Message);
        }
    }

    public void Log<TContext>(string content, ResultBase result, LogLevel logLevel)
    {
        if (result.HasError<ExceptionalError>(out var errors))
        {
            var exception = errors.Select(error => error.Exception).First();
            var logger = factory.CreateLogger<TContext>();
            logger.LogError(exception, "{ExceptionMessage}" ,exception.Message);
        }
    }
}