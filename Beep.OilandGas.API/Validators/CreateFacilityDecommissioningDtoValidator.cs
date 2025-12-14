using FluentValidation;
using Beep.OilandGas.Models.DTOs;

namespace Beep.OilandGas.API.Validators
{
    /// <summary>
    /// Validator for CreateFacilityDecommissioningDto.
    /// </summary>
    public class CreateFacilityDecommissioningDtoValidator : AbstractValidator<CreateFacilityDecommissioningDto>
    {
        public CreateFacilityDecommissioningDtoValidator()
        {
            RuleFor(x => x.FacilityId)
                .NotEmpty().WithMessage("Facility ID is required.")
                .MaximumLength(50).WithMessage("Facility ID must not exceed 50 characters.");

            RuleFor(x => x.DecommissioningMethod)
                .MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.DecommissioningMethod))
                .WithMessage("Decommissioning method must not exceed 100 characters.");

            RuleFor(x => x.EstimatedCost)
                .GreaterThanOrEqualTo(0).When(x => x.EstimatedCost.HasValue)
                .WithMessage("Estimated cost must be greater than or equal to 0.");

            RuleFor(x => x.Currency)
                .MaximumLength(10).When(x => !string.IsNullOrWhiteSpace(x.Currency))
                .WithMessage("Currency code must not exceed 10 characters.");
        }
    }
}

