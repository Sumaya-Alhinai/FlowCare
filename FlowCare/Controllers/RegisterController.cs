using FlowCare.Data;
using FlowCare.Enums;
using FlowCare.Models;
using FlowCare.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlowCare.DTOs;        


namespace FlowCare.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class RegisterController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly FileService _fileService;

        public RegisterController(AppDbContext context, FileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromForm] RegisterDto dto)
        {
            var exists = await _context.Customers
                .AnyAsync(c => c.Email == dto.Email);

            if (exists)
                return BadRequest(new { message = "Email already registered." });

            if (dto.IdImage == null || dto.IdImage.Length == 0)
                return BadRequest(new { message = "ID image is required." });

            string imagePath;
            try
            {
                imagePath = await _fileService.SaveCustomerId(dto.IdImage);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

            var customer = new Customer
            {
                Id = Guid.NewGuid().ToString(),
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                IdImagePath = imagePath
            };

            var user = new User
            {
                Id = customer.Id,
                Username = dto.Email,
                Email = dto.Email,
                FullName = dto.Name,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = UserRole.Customer,
                IsActive = true
            };

            _context.Customers.Add(customer);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                customer.Id,
                customer.Name,
                customer.Email,
                customer.Phone,
                message = "Registration successful."
            });
        }
    }
}