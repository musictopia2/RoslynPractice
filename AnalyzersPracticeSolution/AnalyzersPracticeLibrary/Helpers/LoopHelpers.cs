namespace AnalyzersPracticeLibrary.Helpers;
public static class LoopHelpers
{
    extension(int howMany)
    {
        public void Times(Action<int> action)
        {
            for (int i = 0; i < howMany; i++)
            {
                action?.Invoke(i + 1);
            }
        }
        
    }
}