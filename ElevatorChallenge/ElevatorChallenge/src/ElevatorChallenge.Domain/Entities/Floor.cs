using System;
using System.Collections.Generic;
using System.Text;

namespace ElevatorChallenge.src.ElevatorChallenge.Domain.Domain
{
    public sealed class Floor(int number)
    {

        private readonly Queue<Passenger> _waiting = new();
        public int Number { get; } = number;

        public int WaitingCount => _waiting.Count;

        public void SetWaiting(IEnumerable<Passenger> passengers)
        {
            _waiting.Clear();
            foreach (var passenger in passengers)
                _waiting.Enqueue(passenger);
        }
   
        public bool TryBoardNext(out Passenger passenger) => _waiting.TryDequeue(out passenger!);
    }
}
