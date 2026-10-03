using ElevatorChallenge.Controllers;
using ElevatorChallenge.Domain;
using ElevatorChallenge.Service;
using ElevatorChallenge.UI;

Console.WriteLine("=== Elevator Challenge ===");
// Reads an integer in [min, max]; Enter accepts the default. Re-prompts on bad input.
static int ReadInt(string prompt, int defaultValue, int min, int max)
{
    while (true)
    {
        Console.Write($"{prompt} [{defaultValue}]: ");
        var input = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(input)) return defaultValue;
        if (int.TryParse(input, out int value) && value >= min && value <= max) return value;
        Console.WriteLine($"  Please enter a number between {min} and {max}.");
    }
}

//Define your elevator characteristics
int floors = ReadInt("Number of floors", defaultValue: 10, min: 2, max: 100);
int elevatorCount = ReadInt("Number of elevators", defaultValue: 3, min: 1, max: 20);
int capacity = ReadInt("Weight limit (max people per elevator)", defaultValue: 8, min: 1, max: 50);

//Define you building
var building = new Building(floors);

var elevators = Enumerable.Range(1, elevatorCount)
    .Select(id => new Elevator(id, capacity))
    .ToList();


var controller = new ElevatorController(building, elevators, new NearestAvailableElevatorStrategy());
new ConsoleApp(controller).Run(); // WHere the main movie happens
