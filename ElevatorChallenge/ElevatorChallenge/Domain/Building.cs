using System;
using System.Collections.Generic;
using System.Text;

namespace ElevatorChallenge.Domain
{
    public sealed class Building
    {
        private readonly Floor[] _floors;
        
        public Building(int floorCount)
        {
            // Since an elevator can only be needed from 2 flows minimum
            if (floorCount < 2)
                throw new ArgumentOutOfRangeException(nameof(floorCount), "A building needs at least 2 floors.");
            _floors = Enumerable.Range(0, floorCount).Select(n => new Floor(n)).ToArray();
        }
        public int FloorCount => _floors.Length;
        public IReadOnlyList<Floor> Floors => _floors;
 
        public bool IsValidFloor(int floor) => floor >= 0 && floor < FloorCount;

        public Floor GetFloor(int floor) => _floors[floor];
    }
}
