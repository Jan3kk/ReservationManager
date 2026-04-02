using MediatR;

namespace ReservationManager.Application.Features.Availability.Queries.GetAvailableSlots;

public record GetAvailableSlotsQuery(DateTime Date, int PartySize, float DurationHours) : IRequest<List<TimeSpan>>;
