using ApiProjeKampi.WebApi.Context;
using ApiProjeKampi.WebApi.Dtos.ContactDtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiProjeKampi.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactsController : ControllerBase
    {
        readonly private ApiContext _context;
        public ContactsController(ApiContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<List<ResultContactDto>> ContactList()
        {
            var values = await _context.Contacts.ToListAsync();
            return values.Select(c => new ResultContactDto
            {
                ContactId = c.ContactId,
                MapLocation = c.MapLocation,
                Address = c.Address,
                Phone = c.Phone,
                Email = c.Email,
                OpenHours = c.OpenHours
            }).ToList();
        }
        [HttpPost]
        public async Task<IActionResult> ContactAdd(CreateContactDto createContactDto)
        {
            var contact = new Entities.Contact
            {
                MapLocation = createContactDto.MapLocation,
                Address = createContactDto.Address,
                Phone = createContactDto.Phone,
                Email = createContactDto.Email,
                OpenHours = createContactDto.OpenHours
            };
            _context.Contacts.Add(contact);
            await _context.SaveChangesAsync();
            return Ok("Contact Oluşturuldu");
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteContact(int id)
        {
            var contact = await _context.Contacts.FindAsync(id);
            if (contact == null)
            {
                return NotFound("Contact not found");
            }
            _context.Contacts.Remove(contact);
            await _context.SaveChangesAsync();
            return Ok("Contact Silindi");
        }
        [HttpGet("GetContactById")]
        public async Task<GetByIdContactDto> GetContactById(int id)
        {
            var value = await _context.Contacts.FindAsync(id);
            if ( value==null)
                return null;
            
            return new GetByIdContactDto
            {
                ContactId = value.ContactId,
                MapLocation = value.MapLocation,
                Address = value.Address,
                Phone = value.Phone,
                Email = value.Email,
                OpenHours = value.OpenHours
            };
        }
        [HttpPut]
        public async Task<IActionResult> UpdateContact(UpdateContactDto updateContactDto)
        {
            var contact = await _context.Contacts.FindAsync(updateContactDto.ContactId);
            if (contact == null)
            {
                return NotFound("Contact not found");
            }
            contact.MapLocation = updateContactDto.MapLocation;
            contact.Address = updateContactDto.Address;
            contact.Phone = updateContactDto.Phone;
            contact.Email = updateContactDto.Email;
            contact.OpenHours = updateContactDto.OpenHours;
            _context.Contacts.Update(contact);
            await _context.SaveChangesAsync();
            return Ok("Contact Güncellendi");
        }
    }
}
