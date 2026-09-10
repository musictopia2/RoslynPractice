namespace AnalyzersPracticeLibrary.Helpers;
public static class FlagExtensions
{
    extension<TEnum>(TEnum first)
    {
        public TEnum CombineFlags(params List<TEnum> others)
        {
            ulong flags = Convert.ToUInt64(first);

            foreach (TEnum item in others)
            {
                flags |= Convert.ToUInt64(item);
            }

            return (TEnum)Enum.ToObject(typeof(TEnum), flags);
        }
    }
}
