using ElevatorChallenge.src.ElevatorChallenge.Domain.Domain;
using ElevatorChallenge.src.ElevatorChallenge.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

public sealed class Elevator
{
    private readonly List<Passenger> _passengers = [];
    private readonly SortedSet<int> _stops = [];
    public int Id { get; }

    public int Capacity { get; }
    public int CurrentFloor { get; private set; }
    public Direction Direction { get; private set; } = Direction.None;

    public int PassengerCount => _passengers.Count;
    public bool IsFull => PassengerCount >= Capacity;
    public bool IsMoving => Direction != Direction.None;
    public bool HasWork => _stops.Count > 0;
    public IReadOnlyCollection<int> Stops => _stops;


    public bool HasStopAt(int floor) => _stops.Contains(floor);

    
    public void AddStop(int floor) => _stops.Add(floor);
    public Elevator(int id, int capacity, int startFloor = 0)
    {
        //An elevator that can carry nobody makes no sense.
        ArgumentOutOfRangeException.ThrowIfLessThan(capacity, 1);
        Id = id;
        Capacity = capacity;
        CurrentFloor = startFloor;
    }


    public int EstimateDistanceTo(int floor)
    {
        if (!IsMoving || !HasWork)
            return Math.Abs(CurrentFloor - floor);

        bool onPath = Direction == Direction.Up ? floor >= CurrentFloor : floor <= CurrentFloor;
        if (onPath)
            return Math.Abs(CurrentFloor - floor);

        // Heading away: go to the turnaround point (furthest queued stop), then back to the caller.
        int turnaround = Direction == Direction.Up ? _stops.Max : _stops.Min;
        return Math.Abs(CurrentFloor - turnaround) + Math.Abs(turnaround - floor);
    }

    public void Tick(Building building, Action<string> log)
    {
        // Case 1: we are standing on a stop. Open the doors.
        if (_stops.Remove(CurrentFloor))
        {
            ServiceFloor(building.GetFloor(CurrentFloor), log);
            if (!HasWork) Direction = Direction.None;
            return;
        }

        // Case 2: nothing to do - stay put.
        if (!HasWork)
        {
            Direction = Direction.None;
            return;
        }

        // Case 3: travel one floor towards the next stop.
        Direction = ChooseDirection();
        CurrentFloor += (int)Direction;
    }

    private void ServiceFloor(Floor floor, Action<string> log)
    {
        // Unload first so the space freed up can be used by people boarding on this same floor.
        int exited = _passengers.RemoveAll(p => p.DestinationFloor == CurrentFloor);

        int boarded = 0;
        while (!IsFull && floor.TryBoardNext(out var passenger))
        {
            _passengers.Add(passenger);
            _stops.Add(passenger.DestinationFloor); // the new rider's destination becomes a stop
            boarded++;
        }

        var message = $"Elevator {Id} at floor {CurrentFloor}: {exited} got off, {boarded} got on.";
        if (floor.WaitingCount > 0)
            message += $" Full! {floor.WaitingCount} still waiting.";
        log(message);
    }

    private Direction ChooseDirection()
    {
        bool stopsAbove = _stops.Max > CurrentFloor;
        bool stopsBelow = _stops.Min < CurrentFloor;

        // Keep going while there is something ahead.
        if (Direction == Direction.Up && stopsAbove) return Direction.Up;
        if (Direction == Direction.Down && stopsBelow) return Direction.Down;

        // Stops on both sides: pick whichever is closer.
        if (stopsAbove && stopsBelow)
        {
            int up = _stops.GetViewBetween(CurrentFloor + 1, int.MaxValue).Min - CurrentFloor;
            int down = CurrentFloor - _stops.GetViewBetween(int.MinValue, CurrentFloor - 1).Max;
            return up <= down ? Direction.Up : Direction.Down;
        }

        return stopsAbove ? Direction.Up : Direction.Down;
    }
}


