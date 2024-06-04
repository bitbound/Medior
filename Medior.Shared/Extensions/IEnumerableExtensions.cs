namespace Medior.Shared.Extensions;

public static class IEnumerableExtensions
{
    public static string StringJoin(this IEnumerable<object> strings, string separator = "")
    {
        return string.Join(separator, strings);
    }

    public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<T> enumerable)
    {
        foreach (var item in enumerable)
        {
            yield return item;
        }
        await Task.CompletedTask;
    }
}
