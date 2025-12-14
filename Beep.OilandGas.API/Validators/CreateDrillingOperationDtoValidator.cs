using FluentValidation;
using Beep.OilandGas.Models.DTOs;

namespace Beep.OilandGas.API.Validators
{
    /// <summary>
    /// Validator for CreateDrillingOperationDto.
    /// </summary>
    public class CreateDrillingOperationDtoValidator : AbstractValidator<CreateDrillingOperationDto>
    {
        public CreateDrillingOperationDtoValidator()
        {
            RuleFor(x => x.WellUWI)
                .NotEmpty().WithMessage("Well UWI is required.")
                .MaximumLength(50).WithMessage("Well UWI must not exceed 50 characters.");

            RuleFor(x => x.TargetDepth)
                .GreaterThan(0).When(x => x.TargetDepth.HasValue)
                .WithMessage("Target depth must be greater than 0.");

            RuleFor(x => x.EstimatedDailyCost)
                .GreaterThanOrEqualTo(0).When(x => x.EstimatedDailyCost.HasValue)
                .WithMessage("Estimated daily cost must be greater than or equal to 0.");
        }
    }
}

