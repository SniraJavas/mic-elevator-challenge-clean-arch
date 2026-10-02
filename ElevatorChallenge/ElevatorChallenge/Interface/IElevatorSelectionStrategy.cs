using System;
using System.Collections.Generic;
using System.Text;

namespace ElevatorChallenge.Interface
{
    public interface IElevatorSelectionStrategy
    {
        Elevator? Select(IReadOnlyList<Elevator> elevators, int floor);
    }
}
