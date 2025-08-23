using ApiProjeKampi.WebApi.Context;
using ApiProjeKampi.WebApi.Dtos.MessageDtos;
using ApiProjeKampi.WebApi.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiProjeKampi.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ApiContext _context;   
        public MessagesController(IMapper mapper, ApiContext context)
        {
            _mapper = mapper;
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> MessageList()
        {
            var values = await _context.Messages.ToListAsync();
            var result = _mapper.Map<List<ResultMessageDto>>(values); // values'tan gelen değer ResultMessageDto'ya map ediliyor.
            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> CreateMessage(CreateMessageDto createMessageDto)
        {
            var value = _mapper.Map<Message>(createMessageDto); // createMessageDto'dan gelen değer Message'a map ediliyor.
            await _context.Messages.AddAsync(value);
            await _context.SaveChangesAsync();
            return Ok("Mesaj başarıyla eklendi.");
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var value = await _context.Messages.FindAsync(id);
            if (value == null)
                return NotFound("Mesaj bulunamadı.");
            _context.Messages.Remove(value);
            await _context.SaveChangesAsync();
            return Ok("Mesaj başarıyla silindi.");
        }
        [HttpGet("GetMessage")]
        public async Task<IActionResult> GetMessageById(int id)
        {
            var value = await _context.Messages.FindAsync(id);
            if (value == null)
                return NotFound("Mesaj bulunamadı.");
            var result = _mapper.Map<GetByIdMessageDto>(value); // value'dan gelen değer ResultMessageDto'ya map ediliyor.
            return Ok(result);
        }
        [HttpPut]
        public async Task<IActionResult> UpdateMessage(UpdateMessageDto updateMessageDto)
        {
            var value = _mapper.Map<Message>(updateMessageDto); // updateMessageDto'dan gelen değer Message'a map ediliyor.
            _context.Messages.Update(value);
            await _context.SaveChangesAsync();
            return Ok("Mesaj başarıyla güncellendi.");
        }
        [HttpGet("MessageListByIsReadFalse")]
        public async Task<IActionResult> MessageListByIsReadFalse()
        {
            var values = await _context.Messages.Where(x => x.IsRead == false).ToListAsync();
            return Ok(values);
        }
    }
}
