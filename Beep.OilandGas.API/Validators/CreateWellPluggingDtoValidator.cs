using FluentValidation;
using Beep.OilandGas.Models.DTOs;

namespace Beep.OilandGas.API.Validators
{
    /// <summary>
    /// Validator for CreateWellPluggingDto.
    /// </summary>
    public class CreateWellPluggingDtoValidator : AbstractValidator<CreateWellPluggingDto>
    {
        public CreateWellPluggingDtoValidator()
        {
            RuleFor(x => x.WellUWI)
                .NotEmpty().WithMessage("Well UWI is required.")
                .MaximumLength(50).WithMessage("Well UWI must not exceed 50 characters.");

            RuleFor(x => x.PluggingMethod)
                .MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.PluggingMethod))
                .WithMessage("Plugging method must not exceed 100 characters.");

            RuleFor(x => x.EstimatedCost)
                .GreaterThanOrEqualTo(0).When(x => x.EstimatedCost.HasValue)
                .WithMessage("Estimated cost must be greater than or equal to 0.");

            RuleFor(x => x.Currency)
                .MaximumLength(10).When(x => !string.IsNullOrWhiteSpace(x.Currency))
                .WithMessage("Currency code must not exceed 10 characters.");
        }
    }
}

