namespace AnalyzersPracticeLibrary.Helpers;
public static class DictionaryHelpers
{
    extension(string? value)
    {
        public ImmutableDictionary<string, string?> CreateImmmutableDictionary(string firstKey, string firstValue, string secondKey)
        {
            ImmutableDictionary<string, string?> output =
                ImmutableDictionary<string, string?>.Empty
                    .Add(firstKey, firstValue)
                    .Add(secondKey, value);
            return output;
        }
        //public ImmutableDictionary<string, string?> CreateImmmutableDictionary(string firstKey, string firstValue, string secondKey)
        //{
        //    ImmutableDictionary<string, string?> output =
        //        ImmutableDictionary<string, string?>.Empty
        //            .Add(firstKey, firstValue)
        //            .Add(secondKey, value);
        //    return output;
        //}
    }

    

}