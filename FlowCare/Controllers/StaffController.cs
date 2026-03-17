using FlowCare.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using FlowCare.DTOs;

namespace FlowCare.Controllers
{
    [ApiController]
    [Route("api/staff")]
    public class StaffController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StaffController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,BranchManager")]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int size = 10,
            [FromQuery] string? term = null)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)!.Value;
            var branchId = User.FindFirst("BranchId")?.Value;

            var query = _context.Staff
                .Include(s => s.Branch)
                .Include(s => s.StaffServiceTypes)
                    .ThenInclude(sst => sst.ServiceType)
                .AsQueryable();

            if (userRole == "BranchManager" && branchId != null)
                query = query.Where(s => s.BranchId == branchId);

            if (!string.IsNullOrWhiteSpace(term))
                query = query.Where(s =>
                    s.FullName.Contains(term) ||
                    s.Email.Contains(term));

            var total = await query.CountAsync();

            var results = await query
                .Skip((page - 1) * size)
                .Take(size)
                .Select(s => new
                {
                    s.Id,
                    s.FullName,
                    s.Email,
                    Branch = new
                    {
                        s.Branch.Id,
                        s.Branch.Name,
                        s.Branch.City
                    },
                    Services = s.StaffServiceTypes.Select(sst => new
                    {
                        sst.ServiceType.Id,
                        sst.ServiceType.Name
                    })
                })
                .ToListAsync();

            return Ok(new { Total = total, Results = results });
        }


        [HttpPost("assign")]
        [Authorize(Roles = "Admin,BranchManager")]
        public async Task<IActionResult> Assign([FromBody] AssignStaffDto dto)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)!.Value;
            var branchId = User.FindFirst("BranchId")?.Value;

            var staff = await _context.Staff
                .FirstOrDefaultAsync(s => s.Id == dto.StaffId);

            if (staff == null)
                return NotFound(new { message = "Staff not found." });

            if (userRole == "BranchManager" && staff.BranchId != branchId)
                return Forbid();

            var serviceExists = await _context.ServiceTypes
                .AnyAsync(s => s.Id == dto.ServiceTypeId);

            if (!serviceExists)
                return NotFound(new { message = "Service type not found." });

            var alreadyAssigned = await _context.StaffServiceTypes
                .AnyAsync(sst =>
                    sst.StaffId == dto.StaffId &&
                    sst.ServiceTypeId == dto.ServiceTypeId);

            if (alreadyAssigned)
                return BadRequest(new { message = "Already assigned." });

            _context.StaffServiceTypes.Add(new Models.StaffServiceType
            {
                StaffId = dto.StaffId,
                ServiceTypeId = dto.ServiceTypeId
            });

            await _context.SaveChangesAsync();

            return Ok(new { message = "Staff assigned successfully." });
        }
    }
} 