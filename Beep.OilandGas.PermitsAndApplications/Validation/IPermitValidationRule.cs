namespace Beep.OilandGas.PermitsAndApplications.Validation
{
    public interface IPermitValidationRule
    {
        string Name { get; }
        PermitValidationRuleResult Evaluate(PermitValidationRequest request);
    }
}
