namespace StrykerReportMerger
{
    internal enum MutantStatus
    {
        NotRun = 0,
        Timeout = 1,
        CompileError = 2,
        Ignored = 3,
        NoCoverage = 4,
        Survived = 5,
        Killed = 6,
    }
}
