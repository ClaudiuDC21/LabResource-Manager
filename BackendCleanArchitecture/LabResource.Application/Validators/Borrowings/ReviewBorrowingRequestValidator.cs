using FluentValidation;
using LabResource.Application.DTOs.Borrowings;

namespace LabResource.Application.Validators.Borrowings;

public class ReviewBorrowingRequestValidator : AbstractValidator<ReviewBorrowingRequest>
{
    public ReviewBorrowingRequestValidator()
    {
        RuleFor(x => x.TeacherNotes)
            .MaximumLength(500).WithMessage("Teacher notes cannot exceed 500 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.TeacherNotes));
    }
}