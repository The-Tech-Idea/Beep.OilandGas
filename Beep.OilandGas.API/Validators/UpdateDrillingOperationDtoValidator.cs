using FluentValidation;
using Beep.OilandGas.DrillingAndConstruction.Services;

namespace Beep.OilandGas.API.Validators
{
    /// <summary>
    /// Validator for UpdateDrillingOperationDto.
    /// </summary>
    public class UpdateDrillingOperationDtoValidator : AbstractValidator<UpdateDrillingOperationDto>
    {
        public UpdateDrillingOperationDtoValidator()
        {
            RuleFor(x => x.Status)
                .Must(status => status == null || 
                    status == "Planned" || 
                    status == "InProgress" || 
                    status == "Suspended" || 
                    status == "Completed" || 
                    status == "Abandoned")
                .When(x => !string.IsNullOrWhiteSpace(x.Status))
                .WithMessage("Status must be one of: Planned, InProgress, Suspended, Completed, Abandoned");

            RuleFor(x => x.CurrentDepth)
                .GreaterThanOrEqualTo(0).When(x => x.CurrentDepth.HasValue)
                .WithMessage("Current depth must be greater than or equal to 0.");

            RuleFor(x => x.DailyCost)
                .GreaterThanOrEqualTo(0).When(x => x.DailyCost.HasValue)
                .WithMessage("Daily cost must be greater than or equal to 0.");

            RuleFor(x => x.CompletionDate)
                .GreaterThan(DateTime.MinValue).When(x => x.CompletionDate.HasValue)
                .WithMessage("Completion date must be a valid date.");
        }
    }
}

