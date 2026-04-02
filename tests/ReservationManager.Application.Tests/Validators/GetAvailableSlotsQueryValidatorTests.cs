using FluentAssertions;
using ReservationManager.Application.Features.Availability.Queries.GetAvailableSlots;
using ReservationManager.Domain.Settings;

namespace ReservationManager.Application.Tests.Validators;

public class GetAvailableSlotsQueryValidatorTests
{
    private readonly GetAvailableSlotsQueryValidator _sut = new();
    private readonly DateTime _futureDate = DateTime.UtcNow.Date.AddDays(30);

    private GetAvailableSlotsQuery MakeValid() => new(
        Date: _futureDate,
        PartySize: 2,
        DurationHours: 1.5f);

    [Fact]
    public void ValidQuery_NoErrors()
    {
        var result = _sut.Validate(MakeValid());
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void DateInPast_HasError()
    {
        var query = MakeValid() with { Date = DateTime.UtcNow.Date.AddDays(-1) };
        var result = _sut.Validate(query);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Date");
    }

    [Fact]
    public void PartySizeBelowMin_HasError()
    {
        var query = MakeValid() with { PartySize = RestaurantSettings.MinPartySize - 1 };
        var result = _sut.Validate(query);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PartySize");
    }

    [Fact]
    public void PartySizeAboveMax_HasError()
    {
        var query = MakeValid() with { PartySize = RestaurantSettings.MaxPartySize + 1 };
        var result = _sut.Validate(query);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PartySize");
    }

    [Fact]
    public void DurationBelowMin_HasError()
    {
        var query = MakeValid() with { DurationHours = (float)RestaurantSettings.MinBookingDuration.TotalHours - 0.5f };
        var result = _sut.Validate(query);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DurationHours");
    }

    [Fact]
    public void DurationAboveMax_HasError()
    {
        var query = MakeValid() with { DurationHours = (float)RestaurantSettings.MaxBookingDuration.TotalHours + 0.5f };
        var result = _sut.Validate(query);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DurationHours");
    }

    [Fact]
    public void DurationNotInIncrement_HasError()
    {
        var query = MakeValid() with { DurationHours = 1.7f };
        var result = _sut.Validate(query);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DurationHours");
    }
}
