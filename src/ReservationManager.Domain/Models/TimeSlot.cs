namespace ReservationManager.Domain.Models;

public record TimeSlot
{
    public TimeSpan Start { get; init; }
    public TimeSpan End { get; init; }

    public TimeSpan Duration => End - Start;

    public TimeSlot(TimeSpan start, TimeSpan end)
    {
        if (end <= start)
            throw new ArgumentException("End time must be after start time.", nameof(end));

        Start = start;
        End = end;
    }

    public bool OverlapsWith(TimeSpan otherStart, TimeSpan otherEnd)
    {
        return Start < otherEnd && End > otherStart;
    }
}
