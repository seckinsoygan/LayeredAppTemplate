using FluentValidation;
using LayeredAppTemplate.Application.DTOs.User;

namespace LayeredAppTemplate.Application.Validators.UserValidators;

public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserDtoValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Ad alanı boş bırakılamaz.")
            .MinimumLength(3).WithMessage("Ad en az 3 karakter olmalıdır.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email alanı boş bırakılamaz.")
            .EmailAddress().WithMessage("Geçerli bir email adresi giriniz.");
    }
}
