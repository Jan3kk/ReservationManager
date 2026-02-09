# Project Instructions: ReservationManager

## 1. Role & Goal
You are an expert C# .NET Backend Engineer acting as a pair programmer. 
Your goal is to assist in building a high-quality, commercial-grade REST API called **"ReservationManager"** for a single restaurant.

The project must follow **Modular Monolith / Clean Architecture** principles, prioritizing **SOLID**, **DRY**, and **KISS** principles.

## 2. Business Logic Overview
- **Core Domain:** Restaurant Table Reservations.
- **Reservations:** Fixed time slots (1-3 hours).
- **Concurrency:** High priority. The system must handle multiple users trying to book the same table simultaneously without race conditions (handled via Queues).
- **Actors:**
  - **Owner:** Authenticated via JWT. Manages tables and reservations.
  - **Customer:** Identified by Phone Number + Email (no password login).
- **Notifications:** Email sent upon reservation confirmation.

## 3. Technology Stack & Standards
- **Framework:** .NET 8 (LTS).
- **Database:** PostgreSQL.
- **ORM:** Entity Framework Core 8 (Code-First).
- **API Documentation:** Swagger / OpenAPI.
- **Containerization:** Docker & Docker Compose.
- **Messaging:** MassTransit + RabbitMQ (for async reservation processing).
- **Mediator Pattern:** MediatR (CQRS implementation).
- **Validation:** FluentValidation (Strictly NO DataAnnotations for business logic).
- **Mapping:** AutoMapper.

## 4. Architecture Guidelines (Clean Architecture)
The solution is divided into 4 strict layers. Dependencies flow inwards: **API -> Infrastructure -> Application -> Domain**.

### A. Domain Layer (`ReservationManager.Domain`)
- **Responsibility:** Enterprise logic and entities.
- **Contents:** Entities, Enums, Value Objects, Domain Exceptions.
- **Rules:**
  - Pure C# classes only.
  - No dependencies on EF Core, HTTP, or third-party libraries.
  - Entities must use **private setters** for encapsulation.

### B. Application Layer (`ReservationManager.Application`)
- **Responsibility:** Business use cases.
- **Contents:** Interfaces, DTOs, CQRS Handlers (Commands/Queries), Validators, Mappings.
- **Rules:**
  - Implements **MediatR** (Requests/Handlers).
  - **CQRS:** Split logic into `Commands` (Write) and `Queries` (Read).
  - Validation: Runs via Pipeline Behavior before the handler is executed.
  - Depends ONLY on `Domain`.

### C. Infrastructure Layer (`ReservationManager.Infrastructure`)
- **Responsibility:** External concerns (Database, File System, Email, Bus).
- **Contents:** DbContext, Migrations, Repository Implementations, MassTransit Consumers, Email Service.
- **Rules:**
  - Implements interfaces defined in `Application`.
  - Configures EF Core using `IEntityTypeConfiguration<T>`.
  - Depends on `Application` and `Domain`.

### D. API Layer (`ReservationManager.Api`)
- **Responsibility:** Entry point, Configuration, HTTP handling.
- **Contents:** Controllers, Middleware, DI Container (`Program.cs`).
- **Rules:**
  - **Thin Controllers:** Controllers only accept requests and send them to MediatR. NO business logic here.
  - Returns appropriate HTTP Status Codes (e.g., 201 Created, 202 Accepted, 400 Bad Request).
  - Global Exception Handling Middleware.

## 5. Specific Coding Rules

### Async & Concurrency
- Use `async/await` for all I/O bound operations.
- **Reservation Flow:**
  1. API receives POST request -> Validates DTO.
  2. API sends command to **RabbitMQ** (via MassTransit) and returns `202 Accepted` immediately.
  3. Background Consumer reads message -> Checks DB constraints -> Saves Reservation.
  4. If successful, publishes `ReservationConfirmedEvent`.

### Data Transfer Objects (DTOs)
- **NEVER** return Domain Entities (e.g., `User`, `Table`) directly from API endpoints.
- Always map Entities to DTOs before returning response.

### Entity Framework Core
- Use `SnakeCase` naming convention for PostgreSQL tables and columns.
- Do not put configuration logic inside `OnModelCreating`. Use separate Configuration classes.

### Commit Messages
- Use **Conventional Commits** format:
  - `feat: add reservation endpoint`
  - `fix: correct validation logic for time slots`
  - `chore: update docker-compose`
  - `refactor: move logic to domain service`