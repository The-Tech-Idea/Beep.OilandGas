namespace Beep.OilandGas.PermitsAndApplications.Validation;

public static class PermitFieldValueResolver
{
    public static bool TryResolve(PermitValidationRequest request, string key, out string? value) =>
        TheTechIdea.Data.OilGas.PermitApplicationSupportingRecords.TryResolve(request.Application,
            request.DrillingApplication, request.EnvironmentalApplication, request.InjectionApplication, key, out value)
        && !string.IsNullOrWhiteSpace(value);
}
