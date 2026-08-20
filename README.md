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
│ ├── ConcertBooking.API/ # Presentation Layer
│ ├── ConcertBooking.Application/ # Application Layer
│ ├── ConcertBooking.Domain/ # Domain Layer
│ └── ConcertBooking.Infrastructure/ # Infrastructure Layer
├── workers/
│ └── ConcertBooking.Worker/ # Background Worker
├── tests/
│ └── ConcertBooking.UnitTests/ # Unit Tests
├── migrations/ # EF Core Migrations / SQL Scripts
├── docs/ # Architecture, ERD & Technical Documents
├── docker-compose.yml # Local Infrastructure Setup
├── ConcertBooking.sln
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

git clone https://github.com/Concert-Ticket-Booking-Platform/concert-ticket-booking-be.git
cd ConcertBooking

## 📖 API Documentation

Swagger:

http://localhost:5000/swagger
