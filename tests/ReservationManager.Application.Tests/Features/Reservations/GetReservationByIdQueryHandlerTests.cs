using FluentAssertions;
using NSubstitute;
using ReservationManager.Application.Abstractions.Repositories;
using ReservationManager.Application.Features.Reservations.Queries.GetReservationById;
using ReservationManager.Domain.Entities;

namespace ReservationManager.Application.Tests.Features.Reservations;

public class GetReservationByIdQueryHandlerTests
{
    private readonly IReservationRepository _reservationRepository;
    private readonly GetReservationByIdQueryHandler _sut;

    public GetReservationByIdQueryHandlerTests()
    {
        _reservationRepository = Substitute.For<IReservationRepository>();
        _sut = new GetReservationByIdQueryHandler(_reservationRepository);
    }

    [Fact]
    public async Task ReservationExists_ReturnsDto()
    {
        var reservation = new Reservation(
            Guid.NewGuid(), Guid.NewGuid(), "John", "john@test.com", "123",
            new DateTime(2025, 6, 15, 14, 0, 0), 1.5f, 2);

        _reservationRepository.GetByIdAsync(reservation.Id).Returns(reservation);

        var result = await _sut.Handle(new GetReservationByIdQuery(reservation.Id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(reservation.Id);
        result.TableId.Should().Be(reservation.TableId);
        result.CustomerName.Should().Be("John");
        result.CustomerEmail.Should().Be("john@test.com");
        result.CustomerPhone.Should().Be("123");
        result.DurationHours.Should().Be(1.5f);
        result.PartySize.Should().Be(2);
        result.Status.Should().Be(ReservationStatus.Pending);
    }

    [Fact]
    public async Task ReservationNotFound_ReturnsNull()
    {
        var id = Guid.NewGuid();
        _reservationRepository.GetByIdAsync(id).Returns((Reservation?)null);

        var result = await _sut.Handle(new GetReservationByIdQuery(id), CancellationToken.None);

        result.Should().BeNull();
    }
}
