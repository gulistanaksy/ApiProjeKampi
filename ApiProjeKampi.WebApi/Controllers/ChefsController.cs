using ApiProjeKampi.WebApi.Context;
using ApiProjeKampi.WebApi.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiProjeKampi.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChefsController : ControllerBase
    {
        public readonly ApiContext _context;
        public ChefsController(ApiContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> ChefList()
        {
            var values = await _context.Chefs.ToListAsync();
            if (values == null || !values.Any())
            {
                return NotFound("No chefs found.");
            }
            return Ok(values);
        }
        [HttpPost]
        public async Task<IActionResult> CreateChef(Chef chef)
        {
            if (chef == null)
            {
                return BadRequest("Chef data is null.");
            }
            await _context.Chefs.AddAsync(chef);
            await _context.SaveChangesAsync();
            return Ok("Chef created successfully.");
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteChef(int id)
        {
            var chef = await _context.Chefs.FindAsync(id);
            if (chef == null)
            {
                return NotFound("Chef not found.");
            }
            _context.Chefs.Remove(chef);
            await _context.SaveChangesAsync();
            return Ok("Chef deleted successfully.");
        }
        [HttpGet("GetChef")]
        public async Task<IActionResult> GetChef(int id)
        {
            var chef = await _context.Chefs.FindAsync(id);
            if (chef == null)
            {
                return NotFound("Chef not found.");
            }
            return Ok(chef);
        }
        [HttpPut]
        public async Task<IActionResult> UpdateChef(Chef chef)
        {
            var value = await _context.Chefs.FindAsync(chef.ChefId);
            if (value == null)
                return NotFound("Chef not found.");
            value.NameSurname = chef.NameSurname;
            value.Title = chef.Title;
            value.Description = chef.Description;
            value.ImageUrl = chef.ImageUrl;
            _context.Chefs.Update(value);
            await _context.SaveChangesAsync();
            return Ok("Chef updated successfully.");

        }
    }
}
