using ApiProjeKampi.WebApi.Context;
using ApiProjeKampi.WebApi.Dtos.ReservationDtos;
using ApiProjeKampi.WebApi.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiProjeKampi.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationsController : ControllerBase
    {
        readonly ApiContext _context;
        readonly IMapper _mapper;
        public ReservationsController(ApiContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<IActionResult> ReservationList()
        {
            var values = await _context.Reservations.ToListAsync();
            return Ok(values);
        }

        [HttpPost]
        public async Task<IActionResult> CreateReservation(CreateReservationDto createReservationDto)
        {
            var value = _mapper.Map<Reservation>(createReservationDto);
            await _context.Reservations.AddAsync(value);
            await _context.SaveChangesAsync();
            return (Ok("Rezervasyon eklendi."));
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteReservation(int id)
        {
            var value = await _context.Reservations.FindAsync(id);
            if (value == null)
                return NotFound("Rezervasyon bulunamadı");
            _context.Reservations.Remove(value);
            await _context.SaveChangesAsync();
            return Ok("Rezervasyon silindi.");
        }
        [HttpGet("getReservation")]
        public async Task<IActionResult> GetReservation(int id)
        {
            var value = await _context.Reservations.FindAsync(id);
            if (value == null)
                return NotFound("Rezervasyon bulunamadı.");
            return Ok(value);
        }
        [HttpPut]
        public async Task<IActionResult> UpdateReservation(UpdateReservationDto updateReservationDto)
        {
            var value = _mapper.Map<Reservation>(updateReservationDto);
            _context.Reservations.Update(value);
            await _context.SaveChangesAsync();
            return Ok("Rezervasyon güncellendi.");
        }
        [HttpGet("GetTotalReservationCount")]
        public async Task<IActionResult> GetTotalReservationCount()
        {
            var value = await _context.Reservations.CountAsync();
            return Ok(value);
        }
        [HttpGet("GetTotalCustomerCount")]
        public async Task<IActionResult> GetTotalCustomerCount()
        {
            var value = await _context.Reservations.SumAsync(x => x.CountofPeople);
            return Ok(value);
        }
        [HttpGet("GetPendingReservations")]
        public async Task<IActionResult> GetPendingReservations()
        {
            var value = _context.Reservations.Where(x => x.ReservationStatus=="Onay Bekliyor").Count();
            return Ok(value);
        }
        [HttpGet("GetApprovedReservations")]
        public async Task<IActionResult> GetApprovedReservations()
        {
            var value = _context.Reservations.Where(x => x.ReservationStatus == "Onaylandı").Count();
            return Ok(value);
        }
    }
}
