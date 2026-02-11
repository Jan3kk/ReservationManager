namespace ReservationManager.Application.DTOs;

public record TableDto(Guid Id, string UniqueName, string Label, int Capacity);
