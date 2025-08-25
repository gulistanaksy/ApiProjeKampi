using ApiProjeKampi.WebApi.Context;
using ApiProjeKampi.WebApi.Dtos.AboutDtos;
using ApiProjeKampi.WebApi.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiProjeKampi.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AboutsController : ControllerBase
    {
        readonly ApiContext _context;
        readonly IMapper _mapper;
        public AboutsController(ApiContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<IActionResult> AboutList()
        {
            var values = await _context.Abouts.ToListAsync();
            return Ok(values);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory(CreateAboutDto createAboutDto)
        {
            var value = _mapper.Map<About>(createAboutDto); // createFeatureDto'dan gelen değer Feature'a map ediliyor.
            await _context.Abouts.AddAsync(value);
            await _context.SaveChangesAsync();
            return (Ok("About eklendi."));
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteAbout(int id)
        {
            var value = await _context.Abouts.FindAsync(id);
            if (value == null)
                return NotFound("About bulunamadı");
            _context.Abouts.Remove(value);
            await _context.SaveChangesAsync();
            return Ok("About silindi.");
        }
        [HttpGet("getAbout")]
        public async Task<IActionResult> GetAbout(int id)
        {
            var value = await _context.Abouts.FindAsync(id);
            if (value == null)
                return NotFound("About bulunamadı.");
            return Ok(value);
        }
        [HttpPut]
        public async Task<IActionResult> UpdateAbout(UpdateAboutDto updateAboutDto)
        {
            var value = _mapper.Map<About>(updateAboutDto);
            _context.Abouts.Update(value);
            await _context.SaveChangesAsync();
            return Ok("About güncellendi.");
        }
    }
}
