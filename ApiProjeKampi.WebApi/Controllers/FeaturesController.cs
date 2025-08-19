using ApiProjeKampi.WebApi.Context;
using ApiProjeKampi.WebApi.Dtos.FeatureDtos;
using ApiProjeKampi.WebApi.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiProjeKampi.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeaturesController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ApiContext _context;
        public FeaturesController(IMapper mapper, ApiContext context)
        {
            _mapper = mapper;
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> FeatureList()
        {
            var values = await _context.Features.ToListAsync();
            var result = _mapper.Map<List<ResultFeatureDto>>(values); // valuestan gelen değer ResultFeatureDto'ya map ediliyor.
            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> CreateFeature(CreateFeatureDto createFeatureDto)
        {
            var value = _mapper.Map<Feature>(createFeatureDto); // createFeatureDto'dan gelen değer Feature'a map ediliyor.
            await _context.Features.AddAsync(value);
            await _context.SaveChangesAsync();
            return Ok("Feature başarıyla eklendi.");
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteFeature(int id)
        {
            var value = await _context.Features.FindAsync(id);
            if (value == null)
                return NotFound("Feature bulunamadı.");
            _context.Features.Remove(value);
            await _context.SaveChangesAsync();
            return Ok("Feature başarıyla silindi.");
        }
        [HttpGet("GetFeature")]
        public async Task<IActionResult> GetFeatureById(int id)
        {
            var value = await _context.Features.FindAsync(id);
            var result = _mapper.Map<ResultFeatureDto>(value); // value'dan gelen değer ResultFeatureDto'ya map ediliyor.
            return Ok(result);
        }
        [HttpPut]
        public async Task<IActionResult> UpdateFeature(UpdateFeatureDto updateFeatureDto)
        {
            var value = _mapper.Map<Feature>(updateFeatureDto); // updateFeatureDto'dan gelen değer Feature'a map ediliyor.
            _context.Features.Update(value);
            await _context.SaveChangesAsync();
            return Ok("Feature başarıyla güncellendi.");
        }
    }
}
