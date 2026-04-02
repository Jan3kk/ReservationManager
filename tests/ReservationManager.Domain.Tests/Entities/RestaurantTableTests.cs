using FluentAssertions;
using ReservationManager.Domain.Entities;

namespace ReservationManager.Domain.Tests.Entities;

public class RestaurantTableTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(25)]
    [InlineData(50)]
    public void ValidCapacity_CreatesTable(int capacity)
    {
        var id = Guid.NewGuid();
        var table = new RestaurantTable(id, "T1", "Table 1", capacity);

        table.Id.Should().Be(id);
        table.UniqueName.Should().Be("T1");
        table.Label.Should().Be("Table 1");
        table.Capacity.Should().Be(capacity);
    }

    [Fact]
    public void CapacityBelowOne_ThrowsArgumentOutOfRangeException()
    {
        var act = () => new RestaurantTable(Guid.NewGuid(), "T1", "Table 1", 0);

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("capacity");
    }

    [Fact]
    public void CapacityAboveFifty_ThrowsArgumentOutOfRangeException()
    {
        var act = () => new RestaurantTable(Guid.NewGuid(), "T1", "Table 1", 51);

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("capacity");
    }

    [Fact]
    public void EmptyId_ThrowsArgumentException()
    {
        var act = () => new RestaurantTable(Guid.Empty, "T1", "Table 1", 4);

        act.Should().Throw<ArgumentException>().WithParameterName("id");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyUniqueName_ThrowsArgumentException(string? name)
    {
        var act = () => new RestaurantTable(Guid.NewGuid(), name!, "Table 1", 4);

        act.Should().Throw<ArgumentException>().WithParameterName("uniqueName");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyLabel_ThrowsArgumentException(string? label)
    {
        var act = () => new RestaurantTable(Guid.NewGuid(), "T1", label!, 4);

        act.Should().Throw<ArgumentException>().WithParameterName("label");
    }
}
