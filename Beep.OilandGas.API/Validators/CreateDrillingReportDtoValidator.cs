using FluentValidation;
using Beep.OilandGas.DrillingAndConstruction.Services;

namespace Beep.OilandGas.API.Validators
{
    /// <summary>
    /// Validator for CreateDrillingReportDto.
    /// </summary>
    public class CreateDrillingReportDtoValidator : AbstractValidator<CreateDrillingReportDto>
    {
        public CreateDrillingReportDtoValidator()
        {
            RuleFor(x => x.ReportDate)
                .NotEmpty().WithMessage("Report date is required.")
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Report date cannot be in the future.");

            RuleFor(x => x.Depth)
                .GreaterThanOrEqualTo(0).When(x => x.Depth.HasValue)
                .WithMessage("Depth must be greater than or equal to 0.");

            RuleFor(x => x.Hours)
                .GreaterThan(0).When(x => x.Hours.HasValue)
                .WithMessage("Hours must be greater than 0.")
                .LessThanOrEqualTo(24).When(x => x.Hours.HasValue)
                .WithMessage("Hours must not exceed 24.");

            RuleFor(x => x.Activity)
                .MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.Activity))
                .WithMessage("Activity description must not exceed 500 characters.");
        }
    }
}

