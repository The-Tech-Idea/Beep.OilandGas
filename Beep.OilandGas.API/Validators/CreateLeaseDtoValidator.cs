using FluentValidation;
using Beep.OilandGas.Models.DTOs;

namespace Beep.OilandGas.API.Validators
{
    /// <summary>
    /// Validator for CreateLeaseDto.
    /// </summary>
    public class CreateLeaseDtoValidator : AbstractValidator<CreateLeaseDto>
    {
        public CreateLeaseDtoValidator()
        {
            RuleFor(x => x.LeaseNumber)
                .NotEmpty().WithMessage("Lease number is required.")
                .MaximumLength(100).WithMessage("Lease number must not exceed 100 characters.");

            RuleFor(x => x.LeaseDate)
                .NotEmpty().WithMessage("Lease date is required.");

            RuleFor(x => x.EffectiveDate)
                .NotEmpty().WithMessage("Effective date is required.");

            RuleFor(x => x.ExpirationDate)
                .GreaterThan(x => x.EffectiveDate).When(x => x.ExpirationDate.HasValue && x.EffectiveDate.HasValue)
                .WithMessage("Expiration date must be after effective date.");

            RuleFor(x => x.RoyaltyRate)
                .InclusiveBetween(0, 1).When(x => x.RoyaltyRate.HasValue)
                .WithMessage("Royalty rate must be between 0 and 1.");

            RuleFor(x => x.LeaseArea)
                .GreaterThan(0).When(x => x.LeaseArea.HasValue)
                .WithMessage("Lease area must be greater than 0.");
        }
    }
}

