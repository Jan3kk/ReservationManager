using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using ReservationManager.Application.Abstractions.Repositories;
using ReservationManager.Application.Features.Availability.Queries.GetAvailableSlots;
using ReservationManager.Domain.Entities;
using ReservationManager.Domain.Services;
using ReservationManager.Domain.Settings;

namespace ReservationManager.Application.Tests.Features.Availability;

public class GetAvailableSlotsQueryHandlerTests
{
    private readonly ITableRepository _tableRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly AvailabilityService _availabilityService;
    private readonly GetAvailableSlotsQueryHandler _sut;
    private readonly DateTime _testDate = new(2025, 6, 15);

    public GetAvailableSlotsQueryHandlerTests()
    {
        _tableRepository = Substitute.For<ITableRepository>();
        _reservationRepository = Substitute.For<IReservationRepository>();

        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2025, 6, 15, 8, 0, 0, TimeSpan.Zero));
        _availabilityService = new AvailabilityService(timeProvider);

        _sut = new GetAvailableSlotsQueryHandler(_tableRepository, _reservationRepository, _availabilityService);
    }

    private RestaurantTable MakeTable(int capacity = 4) =>
        new(Guid.NewGuid(), $"T{Guid.NewGuid():N}"[..8], "Table", capacity);

    [Fact]
    public async Task NoTables_ReturnsEmptyList()
    {
        _tableRepository.GetByCapacityAsync(2).Returns(new List<RestaurantTable>());

        var query = new GetAvailableSlotsQuery(_testDate, 2, 1.5f);

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task OneTable_NoReservations_ReturnsSlots()
    {
        var table = MakeTable();
        _tableRepository.GetByCapacityAsync(2).Returns([table]);
        _reservationRepository.GetByTableIdsAndDateAsync(Arg.Any<List<Guid>>(), _testDate)
            .Returns(new List<Reservation>());

        var query = new GetAvailableSlotsQuery(_testDate, 2, 1.5f);

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().NotBeEmpty();
        result.First().Should().Be(RestaurantSettings.OpenTime);
    }

    [Fact]
    public async Task TwoTables_ReturnsUniqueSortedStartTimes()
    {
        var table1 = MakeTable();
        var table2 = MakeTable();
        _tableRepository.GetByCapacityAsync(2).Returns([table1, table2]);

        var reservation1 = new Reservation(
            Guid.NewGuid(), table1.Id, "C1", "c1@test.com", "111",
            _testDate + RestaurantSettings.OpenTime, 1.5f, 2);

        _reservationRepository.GetByTableIdsAndDateAsync(Arg.Any<List<Guid>>(), _testDate)
            .Returns(new List<Reservation> { reservation1 });

        var query = new GetAvailableSlotsQuery(_testDate, 2, 1.5f);

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().NotBeEmpty();
        result.Should().BeInAscendingOrder();
        result.Should().OnlyHaveUniqueItems();
        result.Should().Contain(RestaurantSettings.OpenTime,
            "table2 has no reservations and offers the OpenTime slot");
    }
}
