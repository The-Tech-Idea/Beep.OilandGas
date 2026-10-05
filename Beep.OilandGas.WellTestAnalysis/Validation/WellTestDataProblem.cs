namespace Beep.OilandGas.WellTestAnalysis.Validation
{
    /// <summary>
    /// A problem that keeps well test data from being analysed: which value, and a sentence for the person who sent it.
    /// </summary>
    /// <param name="ParameterName">The well test data member at fault.</param>
    /// <param name="Sentence">What is wrong with it, written for the person.</param>
    public sealed record WellTestDataProblem(string ParameterName, string Sentence);
}
