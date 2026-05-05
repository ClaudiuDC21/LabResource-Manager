using FluentValidation;
using LabResource.Application.DTOs.Users;

namespace LabResource.Application.Validators;

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(100).WithMessage("Full name cannot exceed 100 characters.");

        RuleFor(x => x.MatriculationNumber)
            .MaximumLength(20).WithMessage("Matriculation number cannot exceed 20 characters.")
            .When(x => !string.IsNullOrEmpty(x.MatriculationNumber));
    }
}