using FluentValidation;
using Beep.OilandGas.Models.DTOs;

namespace Beep.OilandGas.API.Validators
{
    /// <summary>
    /// Validator for UpdateLeaseDto.
    /// </summary>
    public class UpdateLeaseDtoValidator : AbstractValidator<UpdateLeaseDto>
    {
        public UpdateLeaseDtoValidator()
        {
            RuleFor(x => x.Status)
                .Must(status => status == null || 
                    status == "Active" || 
                    status == "Inactive" || 
                    status == "Expired" || 
                    status == "Terminated")
                .When(x => !string.IsNullOrWhiteSpace(x.Status))
                .WithMessage("Status must be one of: Active, Inactive, Expired, Terminated");

            RuleFor(x => x.AnnualRental)
                .GreaterThanOrEqualTo(0).When(x => x.AnnualRental.HasValue)
                .WithMessage("Annual rental must be greater than or equal to 0.");
        }
    }
}

