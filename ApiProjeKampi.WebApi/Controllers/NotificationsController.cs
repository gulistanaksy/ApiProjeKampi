using ApiProjeKampi.WebApi.Context;
using ApiProjeKampi.WebApi.Dtos.NotificationDtos;
using ApiProjeKampi.WebApi.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiProjeKampi.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ApiContext _context;
        public NotificationsController(IMapper mapper, ApiContext context)
        {
            _mapper = mapper;
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> NotificationList()
        {
            var values = await _context.Notifications.ToListAsync();
            var result = _mapper.Map<List<ResultNotificationDto>>(values); // valuestan gelen değer ResultFeatureDto'ya map ediliyor.
            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> CreateNotification(CreateNotificationDto createNotification)
        {
            var value = _mapper.Map<Notification>(createNotification); // createFeatureDto'dan gelen değer Feature'a map ediliyor.
            await _context.Notifications.AddAsync(value);
            await _context.SaveChangesAsync();
            return Ok("Notification başarıyla eklendi.");
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var value = await _context.Notifications.FindAsync(id);
            if (value == null)
                return NotFound("Notification bulunamadı.");
            _context.Notifications.Remove(value);
            await _context.SaveChangesAsync();
            return Ok("Notification başarıyla silindi.");
        }
        [HttpGet("GetNotification")]
        public async Task<IActionResult> GetNotificationsById(int id)
        {
            var value = await _context.Notifications.FindAsync(id);
            var result = _mapper.Map<GetByIdNotificationDto>(value); // value'dan gelen değer ResultFeatureDto'ya map ediliyor.
            return Ok(result);
        }
        [HttpPut]
        public async Task<IActionResult> UpdateNotifications(UpdateNotificationDto updateNotificationDto)
        {
            var value = _mapper.Map<Notification>(updateNotificationDto); // updateFeatureDto'dan gelen değer Feature'a map ediliyor.
            _context.Notifications.Update(value);
            await _context.SaveChangesAsync();
            return Ok("Notification başarıyla güncellendi.");
        }
    }
}
