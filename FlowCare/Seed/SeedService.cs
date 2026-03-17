using System.Text.Json;
using System.Text.Json.Serialization;
using FlowCare.Data;
using FlowCare.Models;
using Microsoft.EntityFrameworkCore;

namespace FlowCare.Seed
{
    public class SeedService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public SeedService(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task SeedAsync()
        {
            var filePath = Path.Combine(_env.ContentRootPath, "Seed", "example.json");
            if (!File.Exists(filePath)) return;

            var json = await File.ReadAllTextAsync(filePath);
            var data = JsonSerializer.Deserialize<SeedRoot>(json, _jsonOptions);
            if (data == null) return;

            await SeedBranchesAsync(data.Branches);
            await _context.SaveChangesAsync();

            await SeedUsersAsync(data.Users);
            

            await SeedServiceTypesAsync(data.ServiceTypes);
            await _context.SaveChangesAsync();

            await SeedStaffServiceTypesAsync(data.StaffServiceTypes);
            await _context.SaveChangesAsync();

            await SeedSlotsAsync(data.Slots);
            await _context.SaveChangesAsync();

            await SeedCustomersAsync(data.Users); 
            await _context.SaveChangesAsync();

            await SeedAppointmentsAsync(data.Appointments);
            await _context.SaveChangesAsync();

            await SeedAuditLogsAsync(data.AuditLogs);
            await _context.SaveChangesAsync();
        }

        private async Task SeedBranchesAsync(List<SeedBranchDto> branches)
        {
            if (await _context.Branches.AnyAsync()) return;
            await _context.Branches.AddRangeAsync(
                branches.Select(b => b.ToModel()));
        }

        private async Task SeedUsersAsync(SeedUsers users)
        {
            if (!await _context.Users.AnyAsync())
            {
                var userEntities = users.All()
                    .Select(u => u.ToModel())
                    .ToList();

                await _context.Users.AddRangeAsync(userEntities);
                await _context.SaveChangesAsync();
            }

            if (!await _context.Staff.AnyAsync())
            {
                var staffEntities = users.Staff
                    .Concat(users.BranchManagers ?? [])
                    .Where(u => u.BranchId != null)
                    .Select(u => new Staff
                    {
                        Id = u.Id,
                        FullName = u.FullName,
                        Email = u.Email,
                        BranchId = u.BranchId!
                    })
                    .ToList();

                await _context.Staff.AddRangeAsync(staffEntities);
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedCustomersAsync(SeedUsers users)
        {
            if (await _context.Customers.AnyAsync()) return;

            var customerEntities = users.Customers
                .Select(u => new Customer
                {
                    Id = u.Id,
                    Name = u.FullName,
                    Email = u.Email,
                    Phone = u.Phone,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(u.Password),
                    IdImagePath = "seed/placeholder.jpg"
                })
                .ToList();

            await _context.Customers.AddRangeAsync(customerEntities);
        }

        private async Task SeedServiceTypesAsync(List<SeedServiceTypeDto> items)
        {
            if (await _context.ServiceTypes.AnyAsync()) return;
            await _context.ServiceTypes.AddRangeAsync(
                items.Select(i => i.ToModel()));
        }

        private async Task SeedStaffServiceTypesAsync(List<SeedStaffServiceTypeDto> items)
        {
            if (await _context.StaffServiceTypes.AnyAsync()) return;
            var entities = items.Select(s => new StaffServiceType
            {
                StaffId = s.StaffId,
                ServiceTypeId = s.ServiceTypeId
            });
            await _context.StaffServiceTypes.AddRangeAsync(entities);
        }

        private async Task SeedSlotsAsync(List<SeedSlotDto> slots)
        {
            if (await _context.Slots.AnyAsync()) return;
            await _context.Slots.AddRangeAsync(
                slots.Select(s => s.ToModel()));
        }

        private async Task SeedAppointmentsAsync(List<SeedAppointmentDto> appointments)
        {
            if (await _context.Appointments.AnyAsync()) return;
            await _context.Appointments.AddRangeAsync(
                appointments.Select(a => a.ToModel()));
        }

        private async Task SeedAuditLogsAsync(List<SeedAuditLogDto> logs)
        {
            if (await _context.AuditLogs.AnyAsync()) return;
            await _context.AuditLogs.AddRangeAsync(
                logs.Select(l => l.ToModel()));
        }
    }
}