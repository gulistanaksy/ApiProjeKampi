using ApiProjeKampi.WebApi.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ApiProjeKampi.WebApi.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ApiProjeKampi.WebApi.Dtos.ProductDtos;
using AutoMapper;

namespace ApiProjeKampi.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IValidator<Product> _validator;
        private readonly ApiContext _context;
        private readonly IMapper _mapper;
        public ProductsController(IValidator<Product> validator, ApiContext context, IMapper mapper)
        {
            _validator = validator;
            _context = context;
            _mapper = mapper;
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
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors.Select(x => x.ErrorMessage));
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return Ok("Ürün eklendi.");
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var value = await _context.Products.FindAsync(id);
            _context.Products.Remove(value);
            await _context.SaveChangesAsync();
            return Ok("Ürün silindi.");
        }
        [HttpGet("GetProduct")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var value = await _context.Products.FindAsync(id);
            return Ok(value);
        }
        [HttpPut]
        public async Task<IActionResult> UpdateProduct(Product product)
        {
            var validationResult = await _validator.ValidateAsync(product);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors.Select(x => x.ErrorMessage));
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            return Ok("Ürün Güncellendi.");
        }
        [HttpPost("CreateProductWithCategory")]
        public async Task<IActionResult> CreateProductWithCategory(CreateProductDto createProduct)
        {
            var value = _mapper.Map<Product>(createProduct);
            var validationResult = await _validator.ValidateAsync(value);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors.Select(x => x.ErrorMessage));
            await _context.Products.AddAsync(value);
            await _context.SaveChangesAsync();
            return Ok("Ürün eklendi.");
        }
        [HttpGet("ProductListWithCategory")]
        public async Task<IActionResult> ProductListWithCategory()
        {
            var values = await _context.Products.Include(x=> x.Category).ToListAsync();
            return Ok(_mapper.Map<List<ResultProductWithCategoryDto>>(values));
        }
    }
}
