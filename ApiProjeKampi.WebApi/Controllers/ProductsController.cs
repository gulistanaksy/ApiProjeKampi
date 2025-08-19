using ApiProjeKampi.WebApi.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ApiProjeKampi.WebApi.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ApiProjeKampi.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IValidator<Product> _validator;
        private readonly ApiContext _context;
        public ProductsController(IValidator<Product> validator, ApiContext context)
        {
            _validator = validator;
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> ProductList()
        {
            var values = await _context.Products.ToListAsync();
            return Ok(values);
        }
        [HttpPost]
        public async Task<IActionResult> CreateProduct(Product product)
        {
            var validationResult = await _validator.ValidateAsync(product);
            if(!validationResult.IsValid)
                return BadRequest(validationResult.Errors.Select(x=> x.ErrorMessage));
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return Ok("Ürün eklendi.");
        }

    }
}
