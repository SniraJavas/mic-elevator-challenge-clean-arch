using System;
using System.Collections.Generic;
using System.Text;

namespace ElevatorChallenge.Domain
{
    public sealed record Passenger(int OriginFloor, int DestinationFloor);
}
