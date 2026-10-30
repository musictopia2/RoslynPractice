namespace AnalyzersPracticeLibrary.Helpers;
public static class ListExtensions
{

    extension (IEnumerable<string> source)
    {
        public string TextList => string.Join(", ", source);
    }

    extension<T>(IEnumerable<T> source)
    {

        

        public async Task ForCoditionalItemsAsync(Predicate<T> match, Func<T, Task> action)
        {
            foreach (var item in source)
            {
                if (match.Invoke(item))
                {
                    await action.Invoke(item);
                }
            }
        }
        public void ForCoditionalItems(Predicate<T> match, Action<T> action)
        {
            foreach (var item in source)
            {
                if (match.Invoke(item))
                {
                    action.Invoke(item);
                }
            }
        }


        public bool IsOrderedBy(Func<T, int> getOrder)
        {
            bool hasPrevious = false;
            int previous = 0;

            foreach (T item in source)
            {
                int current = getOrder(item);

                if (hasPrevious && current < previous)
                {
                    return false;
                }

                previous = current;
                hasPrevious = true;
            }

            return true;
        }

    }
}