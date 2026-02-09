using ReservationManager.Domain.Entities;

namespace ReservationManager.Application.DTOs;

public record ReservationDto(
    Guid Id,
    Guid TableId,
    string CustomerName,
    DateTime Date,
    double Duration,
    ReservationStatus Status);
