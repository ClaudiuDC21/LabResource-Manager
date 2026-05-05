using FluentValidation;
using LabResource.Application.DTOs.Borrowings;

namespace LabResource.Application.Validators.Borrowings;

public class BorrowAssetRequestValidator : AbstractValidator<BorrowAssetRequest>
{
    public BorrowAssetRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.LabAssetId)
            .NotEmpty().WithMessage("Asset ID is required.");

        RuleFor(x => x.RequestedStartDate)
            .NotEmpty().WithMessage("Start date is required.")
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date).WithMessage("Start date cannot be in the past.");

        RuleFor(x => x.RequestedEndDate)
            .NotEmpty().WithMessage("End date is required.")
            .GreaterThan(x => x.RequestedStartDate).WithMessage("End date must be after the start date.");
    }
}