using ReservationManager.Domain.Entities;

namespace ReservationManager.Application.DTOs;

public record ReservationDto(
    Guid Id,
    Guid TableId,
    string CustomerName,
    DateTime ReservationDate,
    float DurationHours,
    ReservationStatus Status
);
