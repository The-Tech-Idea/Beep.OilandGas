using FluentValidation;
using Beep.OilandGas.Models.DTOs;

namespace Beep.OilandGas.API.Validators
{
    /// <summary>
    /// Validator for CreateEnhancedRecoveryOperationDto.
    /// </summary>
    public class CreateEnhancedRecoveryOperationDtoValidator : AbstractValidator<CreateEnhancedRecoveryOperationDto>
    {
        public CreateEnhancedRecoveryOperationDtoValidator()
        {
            RuleFor(x => x.FieldId)
                .NotEmpty().WithMessage("Field ID is required.")
                .MaximumLength(50).WithMessage("Field ID must not exceed 50 characters.");

            RuleFor(x => x.EORType)
                .NotEmpty().WithMessage("EOR type is required.")
                .Must(type => type == "WaterFlooding" || 
                             type == "GasInjection" || 
                             type == "CO2Injection" || 
                             type == "Chemical" || 
                             type == "Thermal" || 
                             type == "Other")
                .WithMessage("EOR type must be one of: WaterFlooding, GasInjection, CO2Injection, Chemical, Thermal, Other");

            RuleFor(x => x.PlannedInjectionRate)
                .GreaterThan(0).When(x => x.PlannedInjectionRate.HasValue)
                .WithMessage("Planned injection rate must be greater than 0.");

            RuleFor(x => x.InjectionRateUnit)
                .MaximumLength(20).When(x => !string.IsNullOrWhiteSpace(x.InjectionRateUnit))
                .WithMessage("Injection rate unit must not exceed 20 characters.");
        }
    }
}

