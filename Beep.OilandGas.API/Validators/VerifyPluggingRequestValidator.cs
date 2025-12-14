using FluentValidation;
using Beep.OilandGas.API.Controllers;

namespace Beep.OilandGas.API.Validators
{
    /// <summary>
    /// Validator for VerifyPluggingRequest.
    /// </summary>
    public class VerifyPluggingRequestValidator : AbstractValidator<VerifyPluggingRequest>
    {
        public VerifyPluggingRequestValidator()
        {
            RuleFor(x => x.VerifiedBy)
                .NotEmpty().WithMessage("Verified by is required.")
                .MaximumLength(100).WithMessage("Verified by must not exceed 100 characters.");
        }
    }
}

