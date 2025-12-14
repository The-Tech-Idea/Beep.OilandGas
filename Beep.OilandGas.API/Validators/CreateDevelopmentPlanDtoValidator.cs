using FluentValidation;
using Beep.OilandGas.Models.DTOs;

namespace Beep.OilandGas.API.Validators
{
    /// <summary>
    /// Validator for CreateDevelopmentPlanDto.
    /// </summary>
    public class CreateDevelopmentPlanDtoValidator : AbstractValidator<CreateDevelopmentPlanDto>
    {
        public CreateDevelopmentPlanDtoValidator()
        {
            RuleFor(x => x.PlanName)
                .NotEmpty().WithMessage("Plan name is required.")
                .MaximumLength(200).WithMessage("Plan name must not exceed 200 characters.");

            RuleFor(x => x.FieldId)
                .NotEmpty().WithMessage("Field ID is required.");

            RuleFor(x => x.TargetStartDate)
                .LessThan(x => x.TargetCompletionDate).When(x => x.TargetStartDate.HasValue && x.TargetCompletionDate.HasValue)
                .WithMessage("Target start date must be before target completion date.");

            RuleFor(x => x.EstimatedCost)
                .GreaterThanOrEqualTo(0).When(x => x.EstimatedCost.HasValue)
                .WithMessage("Estimated cost must be greater than or equal to 0.");
        }
    }
}

