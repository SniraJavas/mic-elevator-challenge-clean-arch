using ElevatorChallenge.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElevatorChallenge.Service
{
    public sealed class NearestAvailableElevatorStrategy : IElevatorSelectionStrategy
    {
        public Elevator? Select(IReadOnlyList<Elevator> elevators, int floor) =>
            elevators
                .Where(e => !e.IsFull)                       // skip elevators at the weight limit
                .OrderBy(e => e.EstimateDistanceTo(floor))   // nearest first (direction-aware)
                .ThenBy(e => e.Stops.Count)                  // tie-break: the less busy elevator
                .ThenBy(e => e.Id)                           // final tie-break keeps it deterministic
                .FirstOrDefault();
    }
}
