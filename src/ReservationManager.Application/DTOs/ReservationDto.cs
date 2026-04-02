using ReservationManager.Domain.Entities;

namespace ReservationManager.Application.DTOs;

public record ReservationDto(
    Guid Id,
    Guid TableId,
    string CustomerName,
    string CustomerEmail,
    string CustomerPhone,
    DateTime ReservationDate,
    float DurationHours,
    int PartySize,
    ReservationStatus Status);
