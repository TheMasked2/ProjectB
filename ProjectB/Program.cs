using Spectre.Console;

public static class Program
{
	public static void Main()
	{
		AnsiConsole.Clear();
		Console.WriteLine("Updating database...");
		Thread.Sleep(1000); // Simulate a delay for the update
		FlightLogic.UpdateFlightDB();
		Menu.ShowMainMenu();;
	}
}