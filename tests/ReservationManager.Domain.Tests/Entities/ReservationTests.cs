using FluentAssertions;
using ReservationManager.Domain.Entities;
using ReservationManager.Domain.Settings;

namespace ReservationManager.Domain.Tests.Entities;

public class ReservationTests
{
    private readonly Guid _validId = Guid.NewGuid();
    private readonly Guid _validTableId = Guid.NewGuid();
    private readonly DateTime _validDate = new(2025, 6, 15, 14, 0, 0);
    private const string ValidName = "John Doe";
    private const string ValidEmail = "john@example.com";
    private const string ValidPhone = "123456789";
    private const float ValidDuration = 1.5f;
    private const int ValidPartySize = 4;

    private Reservation CreateValid() => new(
        _validId, _validTableId, ValidName, ValidEmail, ValidPhone,
        _validDate, ValidDuration, ValidPartySize);

    [Fact]
    public void ValidArguments_CreatesWithPendingStatus()
    {
        var reservation = CreateValid();

        reservation.Id.Should().Be(_validId);
        reservation.TableId.Should().Be(_validTableId);
        reservation.CustomerName.Should().Be(ValidName);
        reservation.CustomerEmail.Should().Be(ValidEmail);
        reservation.CustomerPhone.Should().Be(ValidPhone);
        reservation.ReservationDate.Should().Be(_validDate);
        reservation.DurationHours.Should().Be(ValidDuration);
        reservation.PartySize.Should().Be(ValidPartySize);
        reservation.Status.Should().Be(ReservationStatus.Pending);
    }

    [Fact]
    public void EmptyId_ThrowsArgumentException()
    {
        var act = () => new Reservation(
            Guid.Empty, _validTableId, ValidName, ValidEmail, ValidPhone,
            _validDate, ValidDuration, ValidPartySize);

        act.Should().Throw<ArgumentException>().WithParameterName("id");
    }

    [Fact]
    public void EmptyTableId_ThrowsArgumentException()
    {
        var act = () => new Reservation(
            _validId, Guid.Empty, ValidName, ValidEmail, ValidPhone,
            _validDate, ValidDuration, ValidPartySize);

        act.Should().Throw<ArgumentException>().WithParameterName("tableId");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyCustomerName_ThrowsArgumentException(string? name)
    {
        var act = () => new Reservation(
            _validId, _validTableId, name!, ValidEmail, ValidPhone,
            _validDate, ValidDuration, ValidPartySize);

        act.Should().Throw<ArgumentException>().WithParameterName("customerName");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyCustomerEmail_ThrowsArgumentException(string? email)
    {
        var act = () => new Reservation(
            _validId, _validTableId, ValidName, email!, ValidPhone,
            _validDate, ValidDuration, ValidPartySize);

        act.Should().Throw<ArgumentException>().WithParameterName("customerEmail");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyCustomerPhone_ThrowsArgumentException(string? phone)
    {
        var act = () => new Reservation(
            _validId, _validTableId, ValidName, ValidEmail, phone!,
            _validDate, ValidDuration, ValidPartySize);

        act.Should().Throw<ArgumentException>().WithParameterName("customerPhone");
    }

    [Fact]
    public void DurationBelowMin_ThrowsArgumentOutOfRangeException()
    {
        var tooShort = (float)RestaurantSettings.MinBookingDuration.TotalHours - 0.5f;

        var act = () => new Reservation(
            _validId, _validTableId, ValidName, ValidEmail, ValidPhone,
            _validDate, tooShort, ValidPartySize);

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("durationHours");
    }

    [Fact]
    public void DurationAboveMax_ThrowsArgumentOutOfRangeException()
    {
        var tooLong = (float)RestaurantSettings.MaxBookingDuration.TotalHours + 0.5f;

        var act = () => new Reservation(
            _validId, _validTableId, ValidName, ValidEmail, ValidPhone,
            _validDate, tooLong, ValidPartySize);

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("durationHours");
    }

    [Fact]
    public void DurationNotInIncrement_ThrowsArgumentException()
    {
        var badDuration = 1.7f;

        var act = () => new Reservation(
            _validId, _validTableId, ValidName, ValidEmail, ValidPhone,
            _validDate, badDuration, ValidPartySize);

        act.Should().Throw<ArgumentException>().WithParameterName("durationHours");
    }

    [Fact]
    public void PartySizeBelowMin_ThrowsArgumentOutOfRangeException()
    {
        var act = () => new Reservation(
            _validId, _validTableId, ValidName, ValidEmail, ValidPhone,
            _validDate, ValidDuration, RestaurantSettings.MinPartySize - 1);

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("partySize");
    }

    [Fact]
    public void PartySizeAboveMax_ThrowsArgumentOutOfRangeException()
    {
        var act = () => new Reservation(
            _validId, _validTableId, ValidName, ValidEmail, ValidPhone,
            _validDate, ValidDuration, RestaurantSettings.MaxPartySize + 1);

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("partySize");
    }

    [Fact]
    public void Confirm_FromPending_SetsConfirmed()
    {
        var reservation = CreateValid();
        reservation.Status.Should().Be(ReservationStatus.Pending);

        reservation.Confirm();

        reservation.Status.Should().Be(ReservationStatus.Confirmed);
    }

    [Fact]
    public void Confirm_FromRejected_ThrowsInvalidOperationException()
    {
        var reservation = CreateValid();
        reservation.Reject();

        var act = () => reservation.Confirm();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Reject_FromPending_SetsRejected()
    {
        var reservation = CreateValid();

        reservation.Reject();

        reservation.Status.Should().Be(ReservationStatus.Rejected);
    }

    [Fact]
    public void Reject_FromConfirmed_SetsRejected()
    {
        var reservation = CreateValid();
        reservation.Confirm();

        reservation.Reject();

        reservation.Status.Should().Be(ReservationStatus.Rejected);
    }
}
