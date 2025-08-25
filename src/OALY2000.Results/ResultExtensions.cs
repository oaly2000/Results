namespace OALY2000.Results;

public static class ResultExtensions
{
    public static Result<TReturn> Switch<T, TReturn>(this Result<T> source, Func<T, Result<TReturn>> onSuccess, Func<Exception, Result<TReturn>> onFailure)
    {
        if (source.IsSuccess) return onSuccess(source.Value);
        else return onFailure(source.Error);
    }

    public static Result<TResult> Select<TFrom, TResult>(this Result<TFrom> source, Func<TFrom, TResult> selector)
    {
        return source.Switch(r => selector(r), Result<TResult>.Fail);
    }

    public static Result<TResult> SelectMany<TSource, TMiddle, TResult>(
        this Result<TSource> source,
        Func<TSource, Result<TMiddle>> collectionSelector,
        Func<TSource, TMiddle, TResult> resultSelector)
    {
        if (source.IsSuccess) return collectionSelector(source.Value).Select(v => resultSelector(source.Value!, v));
        else return source.Error;
    }

    public static async Task<Result<TResult>> Select<TFrom, TResult>(this Task<Result<TFrom>> source, Func<TFrom, TResult> selector)
        => (await source).Select(selector);

    public static async Task<Result<TResult>> SelectMany<TSource, TMiddle, TResult>(
        this Task<Result<TSource>> source,
        Func<TSource, Task<Result<TMiddle>>> collectionSelector,
        Func<TSource, TMiddle, TResult> resultSelector)
    {
        var result = await source;

        if (result.IsSuccess) return (await collectionSelector(result.Value)).Select(v => resultSelector(result.Value!, v));
        else return result.Error;
    }
}
