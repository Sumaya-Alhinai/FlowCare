# FlowCare
# FlowCare API 🏥

Queue & Appointment Booking System for FlowCare — a growing network of service branches across Oman.

## Tech Stack
- ASP.NET Core 9
- PostgreSQL 16
- Entity Framework Core 9
- JWT + Basic Authentication
- Docker + Docker Compose

---

## Setup Instructions

### Option 1 — Run with Docker (Recommended)

1. Install [Docker Desktop](https://www.docker.com/products/docker-desktop)
2. Clone the repository:
```bash
git clone https://github.com/YOUR_USERNAME/flowcare-api.git
cd flowcare-api
```
3. Run:
```bash
docker-compose up --build
```
4. API available at: `http://localhost:8080`

---

### Option 2 — Run Locally

**Requirements:**
- .NET 9 SDK
- PostgreSQL 16

**Steps:**

1. Clone the repository:
```bash
git clone https://github.com/YOUR_USERNAME/flowcare-api.git
cd flowcare-api
```

2. Update `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=FlowCareDb;Username=postgres;Password=yourpassword"
  },
  "JwtSettings": {
    "Key": "FlowCareSecretKey2026MustBe32CharsLong!",
    "Issuer": "FlowCareAPI",
    "Audience": "FlowCareClients",
    "DurationInMinutes": "60"
  }
}
```

3. Run migrations:
```bash
dotnet ef database update
```

4. Run the app:
```bash
dotnet run
```

---

## Environment Variables

| Variable | Description | Example |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string | `Host=db;Port=5432;...` |
| `JwtSettings__Key` | JWT secret key (min 32 chars) | `FlowCareSecretKey...` |
| `JwtSettings__Issuer` | JWT issuer | `FlowCareAPI` |
| `JwtSettings__Audience` | JWT audience | `FlowCareClients` |
| `JwtSettings__DurationInMinutes` | Token expiry in minutes | `60` |

---

## Seeding Instructions

- Database is seeded **automatically on startup** from `Seed/example.json`
- Seeding is **idempotent** — running multiple times will not duplicate data
- Default seed data includes:
  - 2 branches (Muscat + Suhar)
  - 3+ service types per branch
  - 2+ staff per branch
  - 1+ branch manager per branch
  - 14 slots across multiple days

---

## Default Users

| Role | Email | Password |
|---|---|---|
| Admin | admin@flowcare.local | Admin@123 |
| Branch Manager (Muscat) | aisha.b@flowcare.local | Manager@123 |
| Branch Manager (Suhar) | hamad.h@flowcare.local | Manager@123 |
| Staff (Muscat) | salim.r@flowcare.local | Staff@123 |
| Staff (Suhar) | nasser.m@flowcare.local | Staff@123 |
| Customer | ahmed.h@example.com | Customer@123 |

---

## API Endpoints

### Public (No Authentication Required)
```
GET  /api/branches
GET  /api/branches/{id}/services
GET  /api/branches/{id}/slots
GET  /api/branches/{id}/slots?serviceTypeId={id}
GET  /api/branches/{id}/slots?date=2026-04-01
GET  /api/branches/{id}/queue?appointmentId={id}
```

### Authentication
```
POST /api/auth/register   → form-data + ID image (required)
POST /api/auth/login      → Basic Auth header
```

### Customer (Authenticated)
```
POST   /api/appointments/book              → form-data + optional attachment
GET    /api/appointments                   → list my appointments
GET    /api/appointments/{id}              → appointment details
GET    /api/appointments/{id}/attachment   → download attachment
DELETE /api/appointments/{id}/cancel
PUT    /api/appointments/{id}/reschedule
```

### Staff / Manager / Admin
```
GET /api/appointments              → role-based filtering
PUT /api/appointments/{id}/status  → checked-in, no-show, completed
GET /api/staff                     → Admin: all | Manager: branch-only
GET /api/customers
GET /api/customers/{id}
GET /api/customers/{id}/id-image   → Admin only
GET /api/audit-logs                → Admin: all | Manager: branch-only
```

### Manager / Admin
```
POST   /api/slots          → create single slot
POST   /api/slots/bulk     → create multiple slots
PUT    /api/slots/{id}     → update slot
DELETE /api/slots/{id}     → soft delete
POST   /api/staff/assign   → assign staff to service
```

### Admin Only
```
GET  /api/slots/deleted        → view soft-deleted slots
PUT  /api/admin/retention      → configure retention period
POST /api/admin/cleanup        → hard delete expired slots
GET  /api/audit-logs/export    → export as CSV
```

---

## Example API Usage (curl)

### Login as Admin
```bash
curl -X POST http://localhost:8080/api/auth/login \
  -H "Authorization: Basic YWRtaW5AZmxvd2NhcmUubG9jYWw6QWRtaW5AMTIz"
```

### Register Customer
```bash
curl -X POST http://localhost:8080/api/auth/register \
  -F "Name=John Doe" \
  -F "Email=john@example.com" \
  -F "Password=John@123" \
  -F "Phone=+96891234567" \
  -F "IdImage=@/path/to/image.jpg"
```

### Get Branches
```bash
curl http://localhost:8080/api/branches
```

### Book Appointment
```bash
curl -X POST http://localhost:8080/api/appointments/book \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -F "SlotId=slot_mus_002"
```

### Get Queue Position
```bash
curl "http://localhost:8080/api/branches/br_muscat_001/queue?appointmentId=appt_001"
```

### Export Audit Logs
```bash
curl http://localhost:8080/api/audit-logs/export \
  -H "Authorization: Bearer ADMIN_TOKEN" \
  -o audit_logs.csv
```

### Create Bulk Slots
```bash
curl -X POST http://localhost:8080/api/slots/bulk \
  -H "Authorization: Bearer ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '[
    {
      "BranchId": "br_muscat_001",
      "ServiceTypeId": "svc_mus_001",
      "StartTime": "2026-04-01T09:00:00Z",
      "EndTime": "2026-04-01T09:15:00Z"
    }
  ]'
```

---

## Database Schema

### Main Entities
- **Branch** — Service locations
- **ServiceType** — Types of services per branch
- **Slot** — Available time slots (supports soft delete)
- **Staff** — Employees assigned to branches
- **Customer** — Registered customers
- **Appointment** — Bookings linking customers to slots
- **AuditLog** — Tamper-evident action history
- **User** — Authentication entity (Admin/Manager/Staff/Customer)
- **Config** — System configuration (e.g., retention period)

---

## Bonus Features Implemented ✅
- Pagination (`page` + `size`) on all listing APIs
- Search (`term`) on all listing APIs
- Queue Position endpoint per branch
- Rate Limiting (max 3 bookings/day, max 2 reschedules/day)
- Background cleanup service (runs every 24 hours automatically)
- Docker + Docker Compose
