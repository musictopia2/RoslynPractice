namespace AnalyzersPracticeLibrary.Helpers;
public static class ListExtensions
{
    extension<T>(IEnumerable<T> list)
    {
        public async Task ForCoditionalItemsAsync(Predicate<T> match, Func<T, Task> action)
        {
            foreach (var item in list)
            {
                if (match.Invoke(item))
                {
                    await action.Invoke(item);
                }
            }
        }
        public void ForCoditionalItems(Predicate<T> match, Action<T> action)
        {
            foreach (var item in list)
            {
                if (match.Invoke(item))
                {
                    action.Invoke(item);
                }
            }
        }
    }
}