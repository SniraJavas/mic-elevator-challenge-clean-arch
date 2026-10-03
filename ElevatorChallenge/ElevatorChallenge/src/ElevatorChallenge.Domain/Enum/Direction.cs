using System;
using System.Collections.Generic;
using System.Text;

namespace ElevatorChallenge.src.ElevatorChallenge.Domain.Enum
{
    public enum Direction
    {
        Down = -1,
        None = 0,   // stationary (idle, or doors open)
        Up = 1
    }
}
