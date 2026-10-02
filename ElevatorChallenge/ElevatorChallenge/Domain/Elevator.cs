using ElevatorChallenge.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElevatorChallenge.Domain
{
    public sealed class Elevator
    {
        private readonly List<Passenger> _passengers = [];
        private readonly SortedSet<int> _stops = [];
        public int Id { get; }
        public int Capacity { get; }
        public int CurrentFloor { get; private set; }
        public Direction Direction { get; private set; } = Direction.None;
        public Elevator(int id, int capacity, int startFloor = 0)
        {
            //An elevator that can carry nobody makes no sense.
            ArgumentOutOfRangeException.ThrowIfLessThan(capacity, 1);
            Id = id;
            Capacity = capacity;
            CurrentFloor = startFloor;
        }
    

    }
}
