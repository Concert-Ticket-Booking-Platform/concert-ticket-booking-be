# 🎟️ Concert Ticket Booking Platform

> The system provides customer-facing booking APIs and internal operation APIs, with a focus on ticket inventory consistency, idempotent booking, voucher protection, and asynchronous background processing.

## 🏗️ Architecture

The system follows a Modular Monolith architecture using ASP.NET Core, PostgreSQL, Redis, and RabbitMQ.

Main components:

- API Layer: Handles customer and operation APIs
- Application Layer: Business use cases and application services
- Domain Layer: Entities, enums, and domain rules
- Infrastructure Layer: PostgreSQL, Redis, RabbitMQ
- Background Worker: Processes asynchronous tasks and booking expiration

## 📂 Project Structure

```text
├── src/
│ ├── ConcertTicket.API/ # Presentation Layer
│ ├── ConcertTicket.Application/ # Application Layer
│ ├── ConcertTicket.Domain/ # Domain Layer
│ └── ConcertTicket.Infrastructure/ # Infrastructure Layer
├── workers/
│ └── ConcertTicket.Worker/ # Background Worker
├── tests/
│ └── ConcertTicket.UnitTests/ # Unit Tests
├── migrations/ # EF Core Migrations / SQL Scripts
├── docs/ # Architecture, ERD & Technical Documents
├── docker-compose.yml # Local Infrastructure Setup
├── ConcertTicketPlatform.sln
├── gitignore
└── README.md
```

## 🛠️ Tech Stack

- ASP.NET Core 8
- C#
- PostgreSQL
- Entity Framework Core
- Redis
- RabbitMQ
- Docker / Docker Compose
- Swagger / OpenAPI
- xUnit

## 🚀 Local Setup

### 1. Clone Repository

```bash
git clone https://github.com/Concert-Ticket-Booking-Platform/concert-ticket-booking-be.git
cd ConcertBooking
```

## 📖 API Documentation

Swagger:

http://localhost:8080/swagger
