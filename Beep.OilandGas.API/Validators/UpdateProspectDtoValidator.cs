using FluentValidation;
using Beep.OilandGas.Models.DTOs;

namespace Beep.OilandGas.API.Validators
{
    /// <summary>
    /// Validator for UpdateProspectDto.
    /// </summary>
    public class UpdateProspectDtoValidator : AbstractValidator<UpdateProspectDto>
    {
        public UpdateProspectDtoValidator()
        {
            RuleFor(x => x.ProspectName)
                .MaximumLength(200).When(x => !string.IsNullOrWhiteSpace(x.ProspectName))
                .WithMessage("Prospect name must not exceed 200 characters.");

            RuleFor(x => x.EstimatedResources)
                .GreaterThanOrEqualTo(0).When(x => x.EstimatedResources.HasValue)
                .WithMessage("Estimated resources must be greater than or equal to 0.");

            RuleFor(x => x.Status)
                .Must(status => status == null || 
                    status == "Active" || 
                    status == "Inactive" || 
                    status == "Evaluating" || 
                    status == "Approved" || 
                    status == "Rejected")
                .When(x => !string.IsNullOrWhiteSpace(x.Status))
                .WithMessage("Status must be one of: Active, Inactive, Evaluating, Approved, Rejected");
        }
    }
}

