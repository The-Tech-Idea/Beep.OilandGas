using FluentValidation;
using Beep.OilandGas.Models.DTOs;

namespace Beep.OilandGas.API.Validators
{
    /// <summary>
    /// Validator for CreateProspectDto.
    /// </summary>
    public class CreateProspectDtoValidator : AbstractValidator<CreateProspectDto>
    {
        public CreateProspectDtoValidator()
        {
            RuleFor(x => x.ProspectName)
                .NotEmpty().WithMessage("Prospect name is required.")
                .MaximumLength(200).WithMessage("Prospect name must not exceed 200 characters.");

            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90, 90).When(x => x.Latitude.HasValue)
                .WithMessage("Latitude must be between -90 and 90.");

            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180, 180).When(x => x.Longitude.HasValue)
                .WithMessage("Longitude must be between -180 and 180.");
        }
    }
}

