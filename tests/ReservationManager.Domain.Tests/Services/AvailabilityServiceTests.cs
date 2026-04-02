using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using ReservationManager.Domain.Entities;
using ReservationManager.Domain.Services;
using ReservationManager.Domain.Settings;

namespace ReservationManager.Domain.Tests.Services;

public class AvailabilityServiceTests
{
    private readonly FakeTimeProvider _timeProvider;
    private readonly AvailabilityService _sut;
    private readonly RestaurantTable _defaultTable;
    private readonly DateTime _testDate = new(2025, 6, 15);

    public AvailabilityServiceTests()
    {
        _timeProvider = new FakeTimeProvider(new DateTimeOffset(2025, 6, 15, 8, 0, 0, TimeSpan.Zero));
        _sut = new AvailabilityService(_timeProvider);
        _defaultTable = new RestaurantTable(Guid.NewGuid(), "T1", "Table 1", 4);
    }

    private Reservation MakeReservation(TimeSpan startTime, float durationHours, ReservationStatus status = ReservationStatus.Confirmed)
    {
        var reservation = new Reservation(
            Guid.NewGuid(),
            _defaultTable.Id,
            "Test Customer",
            "test@test.com",
            "123456789",
            _testDate + startTime,
            durationHours,
            partySize: 2);

        if (status == ReservationStatus.Confirmed)
            reservation.Confirm();
        else if (status == ReservationStatus.Rejected)
            reservation.Reject();

        return reservation;
    }

    // --- Empty day ---

    [Fact]
    public void EmptyDay_MinDuration_ReturnsGridFromOpenTime()
    {
        var result = _sut.GetAvailableStartTimes([], 2, _defaultTable, _testDate,
            RestaurantSettings.MinBookingDuration);

        result.Should().NotBeEmpty();
        result.First().Should().Be(RestaurantSettings.OpenTime);

        var gridStep = RestaurantSettings.MinBookingDuration + RestaurantSettings.BufferTime;
        for (int i = 1; i < result.Count; i++)
        {
            (result[i] - result[i - 1]).Should().Be(gridStep);
        }
    }

    [Fact]
    public void EmptyDay_MinDuration_LastSlotPlusDurationDoesNotExceedClose()
    {
        var result = _sut.GetAvailableStartTimes([], 2, _defaultTable, _testDate,
            RestaurantSettings.MinBookingDuration);

        var lastSlot = result.Last();
        (lastSlot + RestaurantSettings.MinBookingDuration).Should()
            .BeLessThanOrEqualTo(RestaurantSettings.CloseTime);
    }

    [Fact]
    public void EmptyDay_MaxDuration_AllSlotsCanFitDuration()
    {
        var maxResult = _sut.GetAvailableStartTimes([], 2, _defaultTable, _testDate,
            RestaurantSettings.MaxBookingDuration);

        foreach (var slot in maxResult)
        {
            (slot + RestaurantSettings.MaxBookingDuration).Should()
                .BeLessThanOrEqualTo(RestaurantSettings.CloseTime);
        }
    }

    // --- Reservation at start of day ---

    [Fact]
    public void ReservationAtOpen_NextAnchorIsEndPlusBuffer()
    {
        var reservations = new List<Reservation>
        {
            MakeReservation(RestaurantSettings.OpenTime, 1.5f)
        };

        var result = _sut.GetAvailableStartTimes(reservations, 2, _defaultTable, _testDate,
            RestaurantSettings.MinBookingDuration);

        var expectedAnchor = RestaurantSettings.OpenTime
            + TimeSpan.FromHours(1.5)
            + RestaurantSettings.BufferTime;

        result.Should().NotBeEmpty();
        result.First().Should().Be(expectedAnchor);
    }

    // --- Reservation in the middle ---

    [Fact]
    public void ReservationInMiddle_SlotsInBothWindows()
    {
        var resStart = RestaurantSettings.OpenTime + TimeSpan.FromHours(4);
        var reservations = new List<Reservation>
        {
            MakeReservation(resStart, 1.5f)
        };

        var result = _sut.GetAvailableStartTimes(reservations, 2, _defaultTable, _testDate,
            RestaurantSettings.MinBookingDuration);

        var slotsBeforeRes = result.Where(s => s < resStart).ToList();
        var resEnd = resStart + TimeSpan.FromHours(1.5);
        var slotsAfterRes = result.Where(s => s >= resEnd + RestaurantSettings.BufferTime).ToList();

        slotsBeforeRes.Should().NotBeEmpty("the window before the reservation should have valid slots");
        slotsAfterRes.Should().NotBeEmpty("the window after the reservation should have valid slots");
    }

    // --- Multiple reservations ---

    [Fact]
    public void TwoAdjacentReservations_NoSlotsBetween()
    {
        var res1End = RestaurantSettings.OpenTime + TimeSpan.FromHours(1.5);
        var res2Start = res1End + RestaurantSettings.BufferTime;
        var reservations = new List<Reservation>
        {
            MakeReservation(RestaurantSettings.OpenTime, 1.5f),
            MakeReservation(res2Start, 1.5f)
        };

        var result = _sut.GetAvailableStartTimes(reservations, 2, _defaultTable, _testDate,
            RestaurantSettings.MinBookingDuration);

        var res2End = res2Start + TimeSpan.FromHours(1.5);
        var slotsBetween = result.Where(s => s > res1End && s < res2Start).ToList();
        slotsBetween.Should().BeEmpty();
    }

    // --- Gap-after validation ---

    [Fact]
    public void GapAfter_RemainingZero_IsValid()
    {
        var resStart = new TimeSpan(15, 30, 0);
        var reservations = new List<Reservation>
        {
            MakeReservation(resStart, 1.5f)
        };

        var result = _sut.GetAvailableStartTimes(reservations, 2, _defaultTable, _testDate,
            RestaurantSettings.MinBookingDuration);

        var deadline = resStart - RestaurantSettings.BufferTime;
        var slotsWithZeroRemaining = result
            .Where(s => s + RestaurantSettings.MinBookingDuration == deadline)
            .ToList();

        slotsWithZeroRemaining.Should().NotBeEmpty(
            "a slot whose end exactly matches the deadline (remaining=0) should be valid");
    }

    [Fact]
    public void GapAfter_TooSmallRemaining_SlotRejected()
    {
        var result = _sut.GetAvailableStartTimes([], 2, _defaultTable, _testDate,
            RestaurantSettings.MinBookingDuration);

        foreach (var slot in result)
        {
            var remaining = RestaurantSettings.CloseTime - (slot + RestaurantSettings.MinBookingDuration);
            var minGap = RestaurantSettings.BufferTime + RestaurantSettings.MinBookingDuration;

            (remaining == TimeSpan.Zero || remaining >= minGap).Should().BeTrue(
                $"slot {slot} leaves remaining {remaining} which is invalid");
        }
    }

    // --- Window ending at reservation vs close ---

    [Fact]
    public void WindowEndingAtReservation_DeadlineSubtractsBuffer()
    {
        var resStart = RestaurantSettings.OpenTime + TimeSpan.FromHours(5);
        var reservations = new List<Reservation>
        {
            MakeReservation(resStart, 1.5f)
        };

        var result = _sut.GetAvailableStartTimes(reservations, 2, _defaultTable, _testDate,
            RestaurantSettings.MinBookingDuration);

        var slotsBeforeRes = result.Where(s => s < resStart).ToList();
        foreach (var slot in slotsBeforeRes)
        {
            var slotEnd = slot + RestaurantSettings.MinBookingDuration;
            var deadline = resStart - RestaurantSettings.BufferTime;
            slotEnd.Should().BeLessThanOrEqualTo(deadline);
        }
    }

    // --- Capacity filtering ---

    [Fact]
    public void TableCapacityLessThanPartySize_ReturnsEmpty()
    {
        var smallTable = new RestaurantTable(Guid.NewGuid(), "S1", "Small", 2);

        var result = _sut.GetAvailableStartTimes([], 5, smallTable, _testDate,
            RestaurantSettings.MinBookingDuration);

        result.Should().BeEmpty();
    }

    // --- MinAdvanceBookingTime ---

    [Fact]
    public void SlotTooCloseToNow_Filtered()
    {
        var now = new DateTimeOffset(2025, 6, 15, 13, 0, 0, TimeSpan.Zero);
        var tp = new FakeTimeProvider(now);
        var sut = new AvailabilityService(tp);

        var result = sut.GetAvailableStartTimes([], 2, _defaultTable, _testDate,
            RestaurantSettings.MinBookingDuration);

        var minAllowed = now.DateTime + RestaurantSettings.MinAdvanceBookingTime;
        foreach (var slot in result)
        {
            (_testDate.Date + slot).Should().BeOnOrAfter(minAllowed);
        }
    }

    [Fact]
    public void SlotFarInFuture_NotFiltered()
    {
        var tp = new FakeTimeProvider(new DateTimeOffset(2025, 6, 14, 8, 0, 0, TimeSpan.Zero));
        var sut = new AvailabilityService(tp);

        var result = sut.GetAvailableStartTimes([], 2, _defaultTable, _testDate,
            RestaurantSettings.MinBookingDuration);

        result.Should().NotBeEmpty();
        result.First().Should().Be(RestaurantSettings.OpenTime);
    }

    // --- Duration variations ---

    [Fact]
    public void DurationTooLongForAnyWindow_ReturnsEmpty()
    {
        var gridStep = RestaurantSettings.MinBookingDuration + RestaurantSettings.BufferTime;
        var reservations = new List<Reservation>();

        var current = RestaurantSettings.OpenTime;
        while (current + RestaurantSettings.MinBookingDuration <= RestaurantSettings.CloseTime)
        {
            reservations.Add(MakeReservation(current, (float)RestaurantSettings.MinBookingDuration.TotalHours));
            current += gridStep;
        }

        var result = _sut.GetAvailableStartTimes(reservations, 2, _defaultTable, _testDate,
            RestaurantSettings.MinBookingDuration);

        result.Should().BeEmpty();
    }

    // --- Edge cases ---

    [Fact]
    public void RejectedReservation_DoesNotBlockSlots()
    {
        var reservations = new List<Reservation>
        {
            MakeReservation(RestaurantSettings.OpenTime, 1.5f, ReservationStatus.Rejected)
        };

        var withRejected = _sut.GetAvailableStartTimes(reservations, 2, _defaultTable, _testDate,
            RestaurantSettings.MinBookingDuration);

        var withoutAny = _sut.GetAvailableStartTimes([], 2, _defaultTable, _testDate,
            RestaurantSettings.MinBookingDuration);

        withRejected.Should().BeEquivalentTo(withoutAny);
    }

    [Fact]
    public void ExactGridCalculation_MatchesExpectedSlots()
    {
        var reservations = new List<Reservation>
        {
            MakeReservation(RestaurantSettings.OpenTime, 3f)
        };

        var result = _sut.GetAvailableStartTimes(reservations, 2, _defaultTable, _testDate,
            RestaurantSettings.MinBookingDuration);

        var anchor = RestaurantSettings.OpenTime + TimeSpan.FromHours(3) + RestaurantSettings.BufferTime;
        var gridStep = RestaurantSettings.MinBookingDuration + RestaurantSettings.BufferTime;
        var minGap = RestaurantSettings.BufferTime + RestaurantSettings.MinBookingDuration;

        var expected = new List<TimeSpan>();
        var candidate = anchor;
        while (candidate + RestaurantSettings.MinBookingDuration <= RestaurantSettings.CloseTime)
        {
            var remaining = RestaurantSettings.CloseTime - (candidate + RestaurantSettings.MinBookingDuration);
            if (remaining == TimeSpan.Zero || remaining >= minGap)
                expected.Add(candidate);
            candidate += gridStep;
        }

        result.Should().BeEquivalentTo(expected, options => options.WithStrictOrdering());
    }
}
