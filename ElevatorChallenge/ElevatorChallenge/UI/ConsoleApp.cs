using ElevatorChallenge.Controllers;
using ElevatorChallenge.Domain;
using ElevatorChallenge.src.ElevatorChallenge.Domain.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElevatorChallenge.UI
{
    public sealed class ConsoleApp(ElevatorController controller)
    {
        private readonly Building _building = controller.Building;

        /// <summary>The read-eval-print loop. Runs until the user types quit (or input ends).</summary>
        public void Run()
        {
            PrintHelp();
            PrintStatus();

            while (true)
            {
                Console.Write("\n> ");
                var line = Console.ReadLine();
                if (line is null) return;

                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (parts.Length == 0) continue;

                var args = parts[1..];
                // Command dispatch via pattern matching; short aliases for quick typing.
                switch (parts[0].ToLowerInvariant())
                {
                    case "status" or "s": PrintStatus(); break;
                    case "people" or "p": SetPeople(args); break;
                    case "call" or "c": CallElevator(args); break;
                    case "step" or "t": Step(args); break;
                    case "run" or "r": RunUntilIdle(); break;
                    case "help" or "h" or "?": PrintHelp(); break;
                    case "quit" or "q" or "exit": return;
                    default: Console.WriteLine("Unknown command. Type 'help'."); break;
                }
            }
        }

        // ---------- define commands ----------

        private void SetPeople(string[] args)
        {
            // Validate everything before touching state, and tell the user how to fix it.
            if (args.Length is < 2 or > 3
                || !int.TryParse(args[0], out int floor)
                || !int.TryParse(args[1], out int count)
                || count < 0)
            {
                Console.WriteLine("Usage: people <floor> <count> [destination]");
                return;
            }

            if (!_building.IsValidFloor(floor))
            {
                Console.WriteLine($"Floor must be between 0 and {_building.FloorCount - 1}.");
                return;
            }

            int? destination = null;
            if (args.Length == 3)
            {
                if (!int.TryParse(args[2], out int d) || !_building.IsValidFloor(d) || d == floor)
                {
                    Console.WriteLine("Destination must be a valid floor different from the origin.");
                    return;
                }
                destination = d;
            }

            var passengers = Enumerable.Range(0, count)
                .Select(_ => new Passenger(floor, destination ?? RandomOtherFloor(floor)))
                .ToList();

            _building.GetFloor(floor).SetWaiting(passengers);
            if (count > 0) controller.RequestElevator(floor);

            Console.WriteLine($"{count} people now waiting on floor {floor}" +
                              (destination is { } dest ? $" (all going to {dest})." : " (random destinations)."));
        }

        private void CallElevator(string[] args)
        {
            if (args.Length != 1 || !int.TryParse(args[0], out int floor) || !_building.IsValidFloor(floor))
            {
                Console.WriteLine($"Usage: call <floor 0-{_building.FloorCount - 1}>");
                return;
            }

            controller.RequestElevator(floor);
            Console.WriteLine($"Elevator called to floor {floor}.");
        }

        
        private void Step(string[] args)
        {
            int steps = args.Length > 0 && int.TryParse(args[0], out int n) && n > 0 ? n : 1;
            for (int i = 0; i < steps; i++)
            {
                PrintEvents(controller.Tick());
                PrintStatus();
            }
        }

        private void RunUntilIdle()
        {
            const int maxTicks = 500;
            for (int i = 0; i < maxTicks && !controller.IsIdle; i++)
            {
                Console.WriteLine($"--- tick {i + 1} ---");
                PrintEvents(controller.Tick());
                PrintStatus();
                Thread.Sleep(400); // slow enough to watch the elevators move
            }
            Console.WriteLine(controller.IsIdle ? "All elevators idle, nobody waiting." : "Stopped after max ticks.");
        }

        // ---------- rendering ----------
        private void PrintStatus()
        {
            Console.WriteLine();
            Console.WriteLine("ELEVATORS");
            foreach (var e in controller.Elevators)
            {
                var movement = e.IsMoving ? $"Moving {e.Direction}" : "Stationary";
                var stops = e.HasWork ? string.Join(",", e.Stops) : "-";
                Console.WriteLine($"  #{e.Id}  floor {e.CurrentFloor,2} | {movement,-11} | {e.PassengerCount}/{e.Capacity} people | stops: {stops}");
            }

            Console.WriteLine("FLOORS");
            // Reverse so the top floor prints first, like looking at a real building.
            foreach (var f in _building.Floors.Reverse())
                Console.WriteLine($"  {f.Number,2}: {f.WaitingCount} waiting");
        }

        private static void PrintEvents(IReadOnlyList<string> events)
        {
            foreach (var e in events) Console.WriteLine($"  * {e}");
        }

        private static void PrintHelp() => Console.WriteLine("""

        Commands:
          status | s                        Show elevators and waiting people
          people | p <floor> <n> [dest]     Set n people waiting on a floor (dest optional; random if omitted)
          call   | c <floor>                Call the nearest available elevator to a floor
          step   | t [n]                    Advance the simulation n ticks (default 1)
          run    | r                        Run automatically until everything is idle
          help   | h                        Show this help
          quit   | q                        Exit
        """);

        private int RandomOtherFloor(int origin)
        {
            int floor = Random.Shared.Next(_building.FloorCount - 1);
            return floor >= origin ? floor + 1 : floor;
        }
    }

}
