using System.Text.Json;
using System.Text.Json.Serialization;
using FlowCare.Enums;
using FlowCare.Models;

namespace FlowCare.Seed
{
    // ROOT 
    public class SeedRoot
    {
        [JsonPropertyName("users")]
        public SeedUsers Users { get; set; } = null!;

        [JsonPropertyName("branches")]
        public List<SeedBranchDto> Branches { get; set; } = null!;

        [JsonPropertyName("service_types")]
        public List<SeedServiceTypeDto> ServiceTypes { get; set; } = null!;

        [JsonPropertyName("staff_service_types")]
        public List<SeedStaffServiceTypeDto> StaffServiceTypes { get; set; } = null!;

        [JsonPropertyName("slots")]
        public List<SeedSlotDto> Slots { get; set; } = null!;

        [JsonPropertyName("appointments")]
        public List<SeedAppointmentDto> Appointments { get; set; } = null!;

        [JsonPropertyName("audit_logs")]
        public List<SeedAuditLogDto> AuditLogs { get; set; } = null!;
    }

    //  USERS 
    public class SeedUsers
    {
        [JsonPropertyName("admin")]
        public List<SeedUserDto> Admin { get; set; } = null!;

        [JsonPropertyName("branch_managers")]
        public List<SeedUserDto> BranchManagers { get; set; } = null!;

        [JsonPropertyName("staff")]
        public List<SeedUserDto> Staff { get; set; } = null!;

        [JsonPropertyName("customers")]
        public List<SeedUserDto> Customers { get; set; } = null!;

        public IEnumerable<SeedUserDto> All() =>
            (Admin ?? [])
            .Concat(BranchManagers ?? [])
            .Concat(Staff ?? [])
            .Concat(Customers ?? []);
    }

    public class SeedUserDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        [JsonPropertyName("username")]
        public string Username { get; set; } = null!;

        [JsonPropertyName("password")]
        public string Password { get; set; } = null!;

        [JsonPropertyName("role")]
        public string Role { get; set; } = null!;

        [JsonPropertyName("full_name")]
        public string FullName { get; set; } = null!;

        [JsonPropertyName("email")]
        public string Email { get; set; } = null!;

        [JsonPropertyName("branch_id")]
        public string? BranchId { get; set; }

        [JsonPropertyName("phone")]
        public string? Phone { get; set; }

        [JsonPropertyName("is_active")]
        public bool IsActive { get; set; }

        private static UserRole ParseRole(string role) => role switch
        {
            "ADMIN" => UserRole.Admin,
            "BRANCH_MANAGER" => UserRole.BranchManager,
            "STAFF" => UserRole.Staff,
            "CUSTOMER" => UserRole.Customer,
            _ => throw new Exception($"Unknown role: {role}")
        };

        public User ToModel() => new User
        {
            Id = Id,
            Username = Username,
            Email = Email,
            FullName = FullName,
            BranchId = BranchId,
            IsActive = IsActive,
            Role = ParseRole(Role), 
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(Password)
        };
    }

    //  BRANCH 
    public class SeedBranchDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [JsonPropertyName("timezone")]
        public string? Timezone { get; set; }

        [JsonPropertyName("is_active")]
        public bool IsActive { get; set; }

        public Branch ToModel() => new Branch
        {
            Id = Id,
            Name = Name,
            City = City,
            Address = Address,
            Timezone = Timezone,
            IsActive = IsActive
        };
    }

    //  SERVICE TYPE 
    public class SeedServiceTypeDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        [JsonPropertyName("branch_id")]
        public string BranchId { get; set; } = null!;

        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("duration_minutes")]
        public int DurationMinutes { get; set; }

        [JsonPropertyName("is_active")]
        public bool IsActive { get; set; }

        public ServiceType ToModel() => new ServiceType
        {
            Id = Id,
            BranchId = BranchId,
            Name = Name,
            Description = Description,
            DurationMinutes = DurationMinutes,
            IsActive = IsActive
        };
    }

    //  STAFF SERVICE TYPE 
    public class SeedStaffServiceTypeDto
    {
        [JsonPropertyName("staff_id")]
        public string StaffId { get; set; } = null!;

        [JsonPropertyName("service_type_id")]
        public string ServiceTypeId { get; set; } = null!;
    }

    //  SLOT 
    public class SeedSlotDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        [JsonPropertyName("branch_id")]
        public string BranchId { get; set; } = null!;

        [JsonPropertyName("service_type_id")]
        public string ServiceTypeId { get; set; } = null!;

        [JsonPropertyName("staff_id")]
        public string? StaffId { get; set; }

        [JsonPropertyName("start_at")]
        public DateTimeOffset StartAt { get; set; }

        [JsonPropertyName("end_at")]
        public DateTimeOffset EndAt { get; set; }

        [JsonPropertyName("capacity")]
        public int Capacity { get; set; }

        [JsonPropertyName("is_active")]
        public bool IsActive { get; set; }

        public Slot ToModel() => new Slot
        {
            Id = Id,
            BranchId = BranchId,
            ServiceTypeId = ServiceTypeId,
            StaffId = StaffId,
            StartTime = StartAt.UtcDateTime,
            EndTime = EndAt.UtcDateTime,
            Capacity = Capacity,
            IsActive = IsActive,
            IsDeleted = false
        };
    }

    //  APPOINTMENT 
    public class SeedAppointmentDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        [JsonPropertyName("customer_id")]
        public string CustomerId { get; set; } = null!;

        [JsonPropertyName("branch_id")]
        public string BranchId { get; set; } = null!;

        [JsonPropertyName("service_type_id")]
        public string ServiceTypeId { get; set; } = null!;

        [JsonPropertyName("slot_id")]
        public string SlotId { get; set; } = null!;

        [JsonPropertyName("staff_id")]
        public string? StaffId { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = null!;

        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

       
        private static AppointmentStatus ParseStatus(string status) => status switch
        {
            "BOOKED" => AppointmentStatus.Booked,
            "CANCELLED" => AppointmentStatus.Cancelled,
            "COMPLETED" => AppointmentStatus.Completed,
            "NO_SHOW" => AppointmentStatus.NoShow,
            "CHECKED_IN" => AppointmentStatus.CheckedIn,
            _ => throw new Exception($"Unknown status: {status}")
        };

        public Appointment ToModel() => new Appointment
        {
            Id = Id,
            CustomerId = CustomerId,
            BranchId = BranchId,
            ServiceTypeId = ServiceTypeId,
            SlotId = SlotId,
            StaffId = StaffId,
            CreatedAt = CreatedAt.UtcDateTime,
            Status = ParseStatus(Status) 
        };
    }

    //  AUDIT LOG 
    public class SeedAuditLogDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        [JsonPropertyName("actor_id")]
        public string ActorId { get; set; } = null!;

        [JsonPropertyName("actor_role")]
        public string ActorRole { get; set; } = null!;

        [JsonPropertyName("action_type")]
        public string ActionType { get; set; } = null!;

        [JsonPropertyName("entity_type")]
        public string EntityType { get; set; } = null!;

        [JsonPropertyName("entity_id")]
        public string EntityId { get; set; } = null!;

        [JsonPropertyName("timestamp")]
        public DateTimeOffset Timestamp { get; set; }

        [JsonPropertyName("metadata")]
        public JsonElement? Metadata { get; set; }

        public AuditLog ToModel() => new AuditLog
        {
            Id = Id,
            UserId = ActorId,
            UserRole = ActorRole,
            ActionType = ActionType,
            TargetEntity = EntityType,
            TargetId = EntityId,
            CreatedAt = Timestamp.UtcDateTime,
            Metadata = Metadata?.ToString()
        };
    }
}