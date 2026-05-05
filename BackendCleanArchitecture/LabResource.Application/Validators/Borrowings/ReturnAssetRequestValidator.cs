using FluentValidation;
using LabResource.Application.DTOs.Borrowings;

namespace LabResource.Application.Validators.Borrowings;

public class ReturnAssetRequestValidator : AbstractValidator<ReturnAssetRequest>
{
    public ReturnAssetRequestValidator()
    {
        RuleFor(x => x.LabAssetId)
            .NotEmpty().WithMessage("Asset ID is required.");

        RuleFor(x => x.Remarks)
            .MaximumLength(500).WithMessage("Remarks cannot exceed 500 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Remarks));
    }
}