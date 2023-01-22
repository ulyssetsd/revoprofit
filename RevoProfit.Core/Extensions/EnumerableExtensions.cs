namespace RevoProfit.Core.Extensions;

public static class AsyncEnumerableExtensions
{
    public static async Task<IEnumerable<T>> ToEnumerableAsync<T>(this IAsyncEnumerable<T> asyncEnumerable)
    {
        var list = new List<T>();
        await foreach (var item in asyncEnumerable)
        {
            list.Add(item);
        }

        return list;
    }
}
