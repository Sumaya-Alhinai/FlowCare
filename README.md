# FlowCare API 🏥

Queue & Appointment Booking System for FlowCare — a growing network of service branches across Oman.

---

## 🚀 Tech Stack

- ASP.NET Core 9
- PostgreSQL 16
- Entity Framework Core 9
- JWT Authentication
- Basic Authentication
- Docker & Docker Compose

---

## 🏗 Architecture Overview

- Clean separation of Controllers, Services, and Data layers
- JWT-based authentication & role-based authorization
- Entity Framework Core (Code-First)
- Automatic database seeding
- Background cleanup service
- Soft delete implementation
- Audit logging system

---

## ⚙ Setup Instructions

### 🐳 Option 1 — Run with Docker (Recommended)

1. Install Docker Desktop  
2. Clone the repository:

```bash
git clone https://github.com/Sumaya-Alhinai/FlowCare.git
cd FlowCare
Run:

docker-compose up --build

API available at:

http://localhost:8080
💻 Option 2 — Run Locally
Requirements:

.NET 9 SDK

PostgreSQL 16

Steps:

Clone repository:

git clone https://github.com/Sumaya-Alhinai/FlowCare.git
cd FlowCare

Update appsettings.json:

{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5433;Database=FlowCareDb;Username=postgres;Password=1122"
  },
  "JwtSettings": {
    "Key": "THIS_IS_SUPER_SECRET_KEY_123456789",
    "Issuer": "FlowCareAPI",
    "Audience": "FlowCareClients",
    "DurationInMinutes": "60"
  }
}

Run migrations:

dotnet ef database update

Start the application:

dotnet run
🌍 Environment Variables
Variable	Description
ConnectionStrings__DefaultConnection	PostgreSQL connection string
JwtSettings__Key	JWT secret key (minimum 32 characters)
JwtSettings__Issuer	Token issuer
JwtSettings__Audience	Token audience
JwtSettings__DurationInMinutes	Token expiration time
🌱 Database Seeding

Automatically runs on startup

Idempotent (no duplicate data)

Includes:

2 branches (Muscat & Suhar)

Service types

Staff & Managers

14 slots

Default system configuration

👤 Default Users
Role	Email	Password
Admin	admin@flowcare.local
	Admin@123
Manager (Muscat)	aisha.b@flowcare.local
	Manager@123
Manager (Suhar)	hamad.h@flowcare.local
	Manager@123
Staff (Muscat)	salim.r@flowcare.local
	Staff@123
Staff (Suhar)	nasser.m@flowcare.local
	Staff@123
Customer	ahmed.h@example.com
	Customer@123
📡 API Endpoints Summary
Public
GET /api/branches
GET /api/branches/{id}/services
GET /api/branches/{id}/slots
GET /api/branches/{id}/queue
Authentication
POST /api/auth/register
POST /api/auth/login
Customer
POST /api/appointments/book
GET  /api/appointments
PUT  /api/appointments/{id}/reschedule
DELETE /api/appointments/{id}/cancel
Staff / Manager / Admin
GET /api/appointments
PUT /api/appointments/{id}/status
Manager / Admin
POST /api/slots
POST /api/slots/bulk
PUT  /api/slots/{id}
DELETE /api/slots/{id}
Admin Only
GET  /api/audit-logs
GET  /api/audit-logs/export
POST /api/admin/cleanup
✨ Key Features

Role-Based Access Control

Queue Management System

Appointment Booking with Attachments

Soft Delete for Slots

Audit Logging

Rate Limiting

Background Cleanup Service

Pagination Support

Search Support

Dockerized Deployment

🏆 Bonus Features Implemented

Pagination (page, size)

Search (term)

Queue Position Endpoint

Rate Limiting:

Max 3 bookings/day

Max 2 reschedules/day

Automatic background cleanup (every 24 hours)

Fully containerized with Docker

📦 Deployment Ready

The project supports:

Local development

Docker environment

Production-style architecture

Scalable database design
