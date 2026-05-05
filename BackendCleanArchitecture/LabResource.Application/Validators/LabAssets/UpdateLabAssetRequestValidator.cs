using FluentValidation;
using LabResource.Application.DTOs.LabAssets;

namespace LabResource.Application.Validators.LabAssets;

public class UpdateLabAssetRequestValidator : AbstractValidator<UpdateLabAssetRequest>
{
    public UpdateLabAssetRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Asset name is required.")
            .MaximumLength(150).WithMessage("Asset name cannot exceed 150 characters.");

        RuleFor(x => x.SerialNumber)
            .MaximumLength(50).WithMessage("Serial number cannot exceed 50 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.SerialNumber));

        RuleFor(x => x.Location)
            .MaximumLength(100).WithMessage("Location cannot exceed 100 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Location));
    }
}