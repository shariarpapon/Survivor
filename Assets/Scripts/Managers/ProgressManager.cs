public static class ProgressManager
{
    public static int totalOperations;
    public static int operationsComplete;

    public static void LogOperations(int operations) 
    {
        totalOperations += operations;
    }

    public static void UpdateProgress() 
    {
        operationsComplete++;
    }

    public static float Progress 
    {
        get { return (float)operationsComplete / (float)totalOperations; }
    }
}
