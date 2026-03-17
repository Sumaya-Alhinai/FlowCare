using FlowCare.Data;
using FlowCare.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlowCare.Controllers
{
    [ApiController]
    [Route("api/customers")]
    [Authorize(Roles = "Admin,BranchManager")]
    public class CustomersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly FileService _fileService;

        public CustomersController(AppDbContext context, FileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int size = 10,
            [FromQuery] string? term = null)
        {
            var query = _context.Customers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(term))
                query = query.Where(c =>
                    c.Name.Contains(term) ||
                    c.Email.Contains(term) ||
                    (c.Phone != null && c.Phone.Contains(term)));

            var total = await query.CountAsync();

            var results = await query
                .Skip((page - 1) * size)
                .Take(size)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Email,
                    c.Phone
                })
                .ToListAsync();

            return Ok(new { Total = total, Results = results });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null)
                return NotFound(new { message = "Customer not found." });

            return Ok(new
            {
                customer.Id,
                customer.Name,
                customer.Email,
                customer.Phone,
                customer.IdImagePath
            });
        }

        [HttpGet("{id}/id-image")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetIdImage(string id)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null)
                return NotFound(new { message = "Customer not found." });

            var folder = Path.Combine(
                Directory.GetCurrentDirectory(), "wwwroot", "uploads", "customer_ids");
            var filePath = Path.Combine(folder, customer.IdImagePath);

            if (!System.IO.File.Exists(filePath))
                return NotFound(new { message = "Image not found." });

            var ext = Path.GetExtension(customer.IdImagePath).ToLower();
            var contentType = ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };

            var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(bytes, contentType);
        }
    }
}
