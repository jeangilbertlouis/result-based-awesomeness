using FluentResults;

namespace ResultBasedApplication;

public static class ResultsExtensions
{
    private static void ThrowExceptionalErrorsException(this Result result)
    {
        if (result.HasError<ExceptionalError>(out var errors))
            throw errors.Select(err => err.Exception).First();
    }

    public static void ThrowOnError<T>(this Result<T> result)
        => result.ToResult().ThrowOnError();

    public static void ThrowOnError(this Result result) =>
        result.ThrowOnSpecificError<IError>();

    private static void ThrowOnSpecificError<TError>(this Result result) where TError : IError
    {
        if (result.IsSuccess) return;
        if (!result.HasError<TError>()) return;

        result.ThrowExceptionalErrorsException();
        throw new Exception(string.Concat(result.Errors.Select(e => e.Message)));
    }
}