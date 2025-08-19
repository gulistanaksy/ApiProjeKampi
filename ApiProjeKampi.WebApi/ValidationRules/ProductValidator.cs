using ApiProjeKampi.WebApi.Entities;
using FluentValidation;

namespace ApiProjeKampi.WebApi.ValidationRules
{
    public class ProductValidator:AbstractValidator<Product>  // <ilgili entity>
    {
        public ProductValidator()
        {
            RuleFor(x => x.ProductName).NotEmpty().WithMessage("Ürün adı boş olamaz.");
            RuleFor(x=> x.ProductName).MinimumLength(2).WithMessage("Ürün adı en az 2 karakter olmalıdır.");
            RuleFor(x=> x.ProductName).MaximumLength(50).WithMessage("Ürün adı en fazla 50 karakter olmalıdır.");

            RuleFor(x=> x.Price).NotEmpty().WithMessage("Fiyat boş olamaz.").GreaterThan(0).
                WithMessage("Ürün fiyatı negatif olamaz!").LessThan(1000).WithMessage("Ürün fiyatı 1000 TL'den yüksek olamaz!");
            
            RuleFor(x=> x.ProductDescription).NotEmpty().WithMessage("Ürün açıklaması boş olamaz.");

        }
    }
}
