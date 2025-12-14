using FluentValidation;
using Beep.OilandGas.ProspectIdentification.Services;

namespace Beep.OilandGas.API.Validators
{
    /// <summary>
    /// Validator for CreateSeismicSurveyDto.
    /// </summary>
    public class CreateSeismicSurveyDtoValidator : AbstractValidator<CreateSeismicSurveyDto>
    {
        public CreateSeismicSurveyDtoValidator()
        {
            RuleFor(x => x.SurveyName)
                .NotEmpty().WithMessage("Survey name is required.")
                .MaximumLength(200).WithMessage("Survey name must not exceed 200 characters.");

            RuleFor(x => x.SurveyType)
                .MaximumLength(50).When(x => !string.IsNullOrWhiteSpace(x.SurveyType))
                .WithMessage("Survey type must not exceed 50 characters.");

            RuleFor(x => x.AreaCovered)
                .GreaterThan(0).When(x => x.AreaCovered.HasValue)
                .WithMessage("Area covered must be greater than 0.");
        }
    }
}

