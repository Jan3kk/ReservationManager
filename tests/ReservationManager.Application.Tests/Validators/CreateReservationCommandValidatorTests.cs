using FluentAssertions;
using ReservationManager.Application.Features.Reservations.Commands.CreateReservation;
using ReservationManager.Domain.Settings;

namespace ReservationManager.Application.Tests.Validators;

public class CreateReservationCommandValidatorTests
{
    private readonly CreateReservationCommandValidator _sut = new();
    private readonly DateTime _futureDate = DateTime.UtcNow.Date.AddDays(30);

    private CreateReservationCommand MakeValid() => new(
        Date: _futureDate,
        StartTime: RestaurantSettings.OpenTime,
        DurationHours: 1.5f,
        PartySize: 2,
        CustomerName: "John Doe",
        CustomerEmail: "john@example.com",
        CustomerPhone: "123456789");

    [Fact]
    public void ValidCommand_NoErrors()
    {
        var result = _sut.Validate(MakeValid());
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void DateInPast_HasError()
    {
        var command = MakeValid() with { Date = DateTime.UtcNow.Date.AddDays(-1) };
        var result = _sut.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Date");
    }

    [Fact]
    public void StartTimeBeforeOpen_HasError()
    {
        var command = MakeValid() with { StartTime = RestaurantSettings.OpenTime - TimeSpan.FromHours(1) };
        var result = _sut.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "StartTime");
    }

    [Fact]
    public void StartTimeAtOrAfterClose_HasError()
    {
        var command = MakeValid() with { StartTime = RestaurantSettings.CloseTime };
        var result = _sut.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "StartTime");
    }

    [Fact]
    public void PartySizeBelowMin_HasError()
    {
        var command = MakeValid() with { PartySize = RestaurantSettings.MinPartySize - 1 };
        var result = _sut.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PartySize");
    }

    [Fact]
    public void PartySizeAboveMax_HasError()
    {
        var command = MakeValid() with { PartySize = RestaurantSettings.MaxPartySize + 1 };
        var result = _sut.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PartySize");
    }

    [Fact]
    public void DurationBelowMin_HasError()
    {
        var command = MakeValid() with { DurationHours = (float)RestaurantSettings.MinBookingDuration.TotalHours - 0.5f };
        var result = _sut.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DurationHours");
    }

    [Fact]
    public void DurationAboveMax_HasError()
    {
        var command = MakeValid() with { DurationHours = (float)RestaurantSettings.MaxBookingDuration.TotalHours + 0.5f };
        var result = _sut.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DurationHours");
    }

    [Fact]
    public void DurationNotInIncrement_HasError()
    {
        var command = MakeValid() with { DurationHours = 1.7f };
        var result = _sut.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DurationHours");
    }

    [Fact]
    public void EmptyCustomerName_HasError()
    {
        var command = MakeValid() with { CustomerName = "" };
        var result = _sut.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CustomerName");
    }

    [Fact]
    public void EmptyCustomerEmail_HasError()
    {
        var command = MakeValid() with { CustomerEmail = "" };
        var result = _sut.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CustomerEmail");
    }

    [Fact]
    public void InvalidEmailFormat_HasError()
    {
        var command = MakeValid() with { CustomerEmail = "not-an-email" };
        var result = _sut.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CustomerEmail");
    }

    [Fact]
    public void EmptyCustomerPhone_HasError()
    {
        var command = MakeValid() with { CustomerPhone = "" };
        var result = _sut.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CustomerPhone");
    }

    [Fact]
    public void ReservationTooCloseToNow_HasError()
    {
        var command = MakeValid() with
        {
            Date = DateTime.UtcNow.Date,
            StartTime = DateTime.UtcNow.TimeOfDay + TimeSpan.FromMinutes(30)
        };

        var startTime = command.StartTime;
        if (startTime < RestaurantSettings.OpenTime)
            return;

        var result = _sut.Validate(command);
        result.IsValid.Should().BeFalse();
    }
}
