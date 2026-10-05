namespace Beep.OilandGas.ChokeAnalysis.Validation
{
    /// <summary>
    /// A problem that keeps a choke calculation from running: which value is out of range (null when the problem is
    /// between values), and a sentence for the person who sent it.
    /// </summary>
    /// <param name="ParameterName">The property out of range, or null when the properties disagree with each other.</param>
    /// <param name="Sentence">What is wrong, written for the person.</param>
    public sealed record ChokeValidationProblem(string? ParameterName, string Sentence);
}
