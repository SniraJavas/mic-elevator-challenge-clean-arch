using ElevatorChallenge.Interface;
using ElevatorChallenge.src.ElevatorChallenge.Domain.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElevatorChallenge.Controllers
{
    public sealed class ElevatorController(
       Building building,
       IReadOnlyList<Elevator> elevators,
       IElevatorSelectionStrategy strategy)
    {
        // No leaking of many people clicking the button, no duplicate
        private readonly HashSet<int> _pendingCalls = [];

        public Building Building { get; } = building;
        public IReadOnlyList<Elevator> Elevators { get; } = elevators;

      
        public bool IsIdle =>
            _pendingCalls.Count == 0
            && Elevators.All(e => !e.HasWork)
            && Building.Floors.All(f => f.WaitingCount == 0);

  
        public void RequestElevator(int floor) => _pendingCalls.Add(floor);

        public IReadOnlyList<string> Tick()
        {
            var events = new List<string>();

            // Step 1: people left behind (e.g. the elevator was full) press the button again.
            foreach (var floor in Building.Floors.Where(f => f.WaitingCount > 0))
            {
                if (!Elevators.Any(e => e.HasStopAt(floor.Number)))
                    _pendingCalls.Add(floor.Number);
            }

            // Step 2: assign each pending call to the best elevator.
            foreach (int call in _pendingCalls.Order().ToList())
            {
                // An elevator is already on its way here: nothing to assign.
                if (Elevators.Any(e => e.HasStopAt(call)))
                {
                    _pendingCalls.Remove(call);
                    continue;
                }

                var chosen = strategy.Select(Elevators, call);
                if (chosen is null) continue; // every elevator is full - leave it pending, retry next tick

                chosen.AddStop(call);
                _pendingCalls.Remove(call);
                events.Add($"Elevator {chosen.Id} dispatched to floor {call}.");
            }

            // Step 3: let every elevator act (open doors or move one floor).
            foreach (var elevator in Elevators)
                elevator.Tick(Building, events.Add);

            return events;
        }
    }

}
