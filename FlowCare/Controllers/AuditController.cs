using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlowCare.Data;
using FlowCare.Services;
using System.Security.Claims;

namespace FlowCare.Controllers
{
    [ApiController]
    [Route("api/audit-logs")]
    public class AuditController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly CsvService _csvService;

        public AuditController(AppDbContext context, CsvService csvService)
        {
            _context = context;
            _csvService = csvService;
        }

       
        [HttpGet]
        [Authorize(Roles = "Admin,BranchManager")]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int size = 20)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)!.Value;
            var branchId = User.FindFirst("BranchId")?.Value;

            var query = _context.AuditLogs
                .OrderByDescending(a => a.CreatedAt)
                .AsQueryable();

            if (userRole == "BranchManager" && branchId != null)
                query = query.Where(a => a.BranchId == branchId);

            var total = await query.CountAsync();

            var results = await query
                .Skip((page - 1) * size)
                .Take(size)
                .Select(a => new
                {
                    a.Id,
                    a.ActionType,
                    a.UserId,
                    a.UserRole,
                    a.TargetEntity,
                    a.TargetId,
                    a.CreatedAt,
                    a.Metadata,
                    a.BranchId
                })
                .ToListAsync();

            return Ok(new { Total = total, Results = results });
        }

        [HttpGet("export")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportCsv()
        {
            var logs = await _context.AuditLogs
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            var csvBytes = _csvService.ExportAuditLogs(logs);

            return File(csvBytes, "text/csv", "audit_logs.csv");
        }
    }
}