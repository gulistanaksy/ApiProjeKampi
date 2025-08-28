using ApiProjeKampi.WebApi.Context;
using ApiProjeKampi.WebApi.Dtos.ImageDtos;
using ApiProjeKampi.WebApi.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiProjeKampi.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        readonly ApiContext _context;
        readonly IMapper _mapper;
        public ImagesController(ApiContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<IActionResult> ImageList()
        {
            var values = await _context.Images.ToListAsync();
            return Ok(values);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory(CreateImageDto createImageDto)
        {
            var value = _mapper.Map<Image>(createImageDto); 
            await _context.Images.AddAsync(value);
            await _context.SaveChangesAsync();
            return (Ok("Resim eklendi."));
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteImage(int id)
        {
            var value = await _context.Images.FindAsync(id);
            if (value == null)
                return NotFound("Resim bulunamadı");
            _context.Images.Remove(value);
            await _context.SaveChangesAsync();
            return Ok("Resim silindi.");
        }
        [HttpGet("GetImage")]
        public async Task<IActionResult> GetImage(int id)
        {
            var value = await _context.Images.FindAsync(id);
            if (value == null)
                return NotFound("Resim bulunamadı.");
            return Ok(value);
        }
        [HttpPut]
        public async Task<IActionResult> UpdateImage(UpdateImageDto updateImageDto)
        {
            var value = _mapper.Map<Image>(updateImageDto);
            _context.Images.Update(value);
            await _context.SaveChangesAsync();
            return Ok("Resim güncellendi.");
        }
    }
}
