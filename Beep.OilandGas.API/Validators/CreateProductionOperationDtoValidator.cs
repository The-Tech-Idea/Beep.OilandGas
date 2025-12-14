using FluentValidation;
using Beep.OilandGas.Models.DTOs;

namespace Beep.OilandGas.API.Validators
{
    /// <summary>
    /// Validator for CreateProductionOperationDto.
    /// </summary>
    public class CreateProductionOperationDtoValidator : AbstractValidator<CreateProductionOperationDto>
    {
        public CreateProductionOperationDtoValidator()
        {
            RuleFor(x => x.WellUWI)
                .NotEmpty().WithMessage("Well UWI is required.")
                .MaximumLength(50).WithMessage("Well UWI must not exceed 50 characters.");

            RuleFor(x => x.OperationDate)
                .NotEmpty().WithMessage("Operation date is required.")
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Operation date cannot be in the future.");

            RuleFor(x => x.OilProduction)
                .GreaterThanOrEqualTo(0).When(x => x.OilProduction.HasValue)
                .WithMessage("Oil production must be greater than or equal to 0.");

            RuleFor(x => x.GasProduction)
                .GreaterThanOrEqualTo(0).When(x => x.GasProduction.HasValue)
                .WithMessage("Gas production must be greater than or equal to 0.");

            RuleFor(x => x.WaterProduction)
                .GreaterThanOrEqualTo(0).When(x => x.WaterProduction.HasValue)
                .WithMessage("Water production must be greater than or equal to 0.");
        }
    }
}

