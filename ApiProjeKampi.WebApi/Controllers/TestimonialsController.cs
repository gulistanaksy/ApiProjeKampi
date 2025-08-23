using ApiProjeKampi.WebApi.Context;
using ApiProjeKampi.WebApi.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiProjeKampi.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestimonialsController : ControllerBase
    {
        private readonly ApiContext _context;
        public TestimonialsController(ApiContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult TestimonialList()
        {
            var values = _context.Testimonials.ToList();
            return Ok(values);
        }

        [HttpPost]
        public IActionResult CreateTestimonial(Testimonial Testimonial)
        {
            _context.Testimonials.Add(Testimonial);
            _context.SaveChanges();
            return Ok("Referans ekleme işlemi başarılı");
        }

        [HttpDelete]
        public IActionResult DeleteTestimonial(int id)
        {
            var value = _context.Testimonials.Find(id);
            _context.Testimonials.Remove(value);
            _context.SaveChanges();
            return Ok("Referans silme işlemi başarılı");
        }

        [HttpGet("GetTestimonial")]
        public IActionResult GetTestimonial(int id)
        {
            var value = _context.Testimonials.Find(id);
            return Ok(value);
        }

        [HttpPut]
        public IActionResult UpdateTestimonial(Testimonial Testimonial)
        {
            _context.Testimonials.Update(Testimonial);
            _context.SaveChanges();
            return Ok("Referans güncelleme işlemi başarılı");
        }
    }
}