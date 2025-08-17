using ApiProjeKampi.WebApi.Context;
using ApiProjeKampi.WebApi.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ApiProjeKampi.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        readonly ApiContext _context;
        public CategoriesController(ApiContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> CategoryList()
        {
            var values = await _context.Categories.ToListAsync();
            return Ok(values);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory(Category category)
        {
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
            return (Ok("Kategory eklendi."));
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound("kategori bulunamadı");
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return Ok("Kategori silindi.");
        }
        [HttpGet("getCategory")]
        public async Task<IActionResult> GetCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound("Kategori bulunamadı.");
            return Ok(category);
        }
        [HttpPut]
        public async Task<IActionResult> UpdateCategory(Category category)
        {
            var value = await _context.Categories.FindAsync(category.CategoryId);
            if (value == null)
                return NotFound("Kategori bulunamadı.");
            value.CategoryName = category.CategoryName;
            await _context.SaveChangesAsync();
            return Ok("Kategori güncellendi.");
        }
    }
}
