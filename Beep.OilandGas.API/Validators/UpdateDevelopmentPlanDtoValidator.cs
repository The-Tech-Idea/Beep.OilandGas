using FluentValidation;
using Beep.OilandGas.Models.DTOs;

namespace Beep.OilandGas.API.Validators
{
    /// <summary>
    /// Validator for UpdateDevelopmentPlanDto.
    /// </summary>
    public class UpdateDevelopmentPlanDtoValidator : AbstractValidator<UpdateDevelopmentPlanDto>
    {
        public UpdateDevelopmentPlanDtoValidator()
        {
            RuleFor(x => x.PlanName)
                .MaximumLength(200).When(x => !string.IsNullOrWhiteSpace(x.PlanName))
                .WithMessage("Plan name must not exceed 200 characters.");

            RuleFor(x => x.Status)
                .Must(status => status == null || 
                    status == "Draft" || 
                    status == "Submitted" || 
                    status == "UnderReview" || 
                    status == "Approved" || 
                    status == "Rejected" || 
                    status == "InProgress" || 
                    status == "Completed")
                .When(x => !string.IsNullOrWhiteSpace(x.Status))
                .WithMessage("Status must be one of: Draft, Submitted, UnderReview, Approved, Rejected, InProgress, Completed");

            RuleFor(x => x.TargetStartDate)
                .LessThan(x => x.TargetCompletionDate).When(x => x.TargetStartDate.HasValue && x.TargetCompletionDate.HasValue)
                .WithMessage("Target start date must be before target completion date.");

            RuleFor(x => x.EstimatedCost)
                .GreaterThanOrEqualTo(0).When(x => x.EstimatedCost.HasValue)
                .WithMessage("Estimated cost must be greater than or equal to 0.");
        }
    }
}

