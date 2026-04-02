using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using ReservationManager.Application.Abstractions.Repositories;
using ReservationManager.Application.Features.Reservations.Commands.CreateReservation;
using ReservationManager.Domain.Entities;
using ReservationManager.Domain.Services;
using ReservationManager.Domain.Settings;

namespace ReservationManager.Application.Tests.Features.Reservations;

public class CreateReservationCommandHandlerTests
{
    private readonly ITableRepository _tableRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly AvailabilityService _availabilityService;
    private readonly CreateReservationCommandHandler _sut;
    private readonly DateTime _testDate = new(2025, 6, 15);

    public CreateReservationCommandHandlerTests()
    {
        _tableRepository = Substitute.For<ITableRepository>();
        _reservationRepository = Substitute.For<IReservationRepository>();

        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2025, 6, 15, 8, 0, 0, TimeSpan.Zero));
        _availabilityService = new AvailabilityService(timeProvider);

        _sut = new CreateReservationCommandHandler(_tableRepository, _reservationRepository, _availabilityService);

        _reservationRepository.AddAsync(Arg.Any<Reservation>())
            .Returns(ci => ci.Arg<Reservation>().Id);
    }

    private CreateReservationCommand MakeCommand(TimeSpan? startTime = null) => new(
        Date: _testDate,
        StartTime: startTime ?? RestaurantSettings.OpenTime,
        DurationHours: 1.5f,
        PartySize: 2,
        CustomerName: "John",
        CustomerEmail: "john@test.com",
        CustomerPhone: "123456789");

    private RestaurantTable MakeTable(int capacity = 4) =>
        new(Guid.NewGuid(), $"T{Guid.NewGuid():N}"[..8], "Table", capacity);

    [Fact]
    public async Task HappyPath_SingleTable_SlotAvailable_ReturnsGuid()
    {
        var table = MakeTable();
        _tableRepository.GetByCapacityAsync(2).Returns([table]);
        _reservationRepository.GetByTableIdsAndDateAsync(Arg.Any<List<Guid>>(), _testDate)
            .Returns(new List<Reservation>());

        var command = MakeCommand();

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().NotBeEmpty();
        await _reservationRepository.Received(1).AddAsync(Arg.Any<Reservation>());
    }

    [Fact]
    public async Task NoTablesWithSufficientCapacity_ThrowsInvalidOperation()
    {
        _tableRepository.GetByCapacityAsync(5).Returns(new List<RestaurantTable>());

        var command = MakeCommand() with { PartySize = 5 };

        var act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task StartTimeNotAvailable_ThrowsInvalidOperation()
    {
        var table = MakeTable();
        _tableRepository.GetByCapacityAsync(2).Returns([table]);

        var blockingReservation = new Reservation(
            Guid.NewGuid(), table.Id, "Other", "other@test.com", "000",
            _testDate + RestaurantSettings.OpenTime, 1.5f, 2);

        _reservationRepository.GetByTableIdsAndDateAsync(Arg.Any<List<Guid>>(), _testDate)
            .Returns(new List<Reservation> { blockingReservation });

        var command = MakeCommand(startTime: RestaurantSettings.OpenTime);

        var act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task PicksSmallestSufficientTable()
    {
        var bigTable = MakeTable(capacity: 8);
        var smallTable = MakeTable(capacity: 3);
        _tableRepository.GetByCapacityAsync(2).Returns([bigTable, smallTable]);
        _reservationRepository.GetByTableIdsAndDateAsync(Arg.Any<List<Guid>>(), _testDate)
            .Returns(new List<Reservation>());

        var command = MakeCommand();

        await _sut.Handle(command, CancellationToken.None);

        await _reservationRepository.Received(1).AddAsync(
            Arg.Is<Reservation>(r => r.TableId == smallTable.Id));
    }

    [Fact]
    public async Task FirstTableFull_UsesSecondTable()
    {
        var table1 = MakeTable(capacity: 3);
        var table2 = MakeTable(capacity: 4);
        _tableRepository.GetByCapacityAsync(2).Returns([table1, table2]);

        var blockingReservation = new Reservation(
            Guid.NewGuid(), table1.Id, "Other", "other@test.com", "000",
            _testDate + RestaurantSettings.OpenTime, 1.5f, 2);

        _reservationRepository.GetByTableIdsAndDateAsync(Arg.Any<List<Guid>>(), _testDate)
            .Returns(new List<Reservation> { blockingReservation });

        var command = MakeCommand(startTime: RestaurantSettings.OpenTime);

        await _sut.Handle(command, CancellationToken.None);

        await _reservationRepository.Received(1).AddAsync(
            Arg.Is<Reservation>(r => r.TableId == table2.Id));
    }
}
