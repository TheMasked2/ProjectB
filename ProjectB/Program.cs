using Spectre.Console;

public static class Program
{
	public static void Main()
	{
		// For Spectre.Console to work in debugger
		AnsiConsole.Profile.Capabilities.Interactive = true;

		AnsiConsole.Clear();
		Console.WriteLine("Updating database...");
		Thread.Sleep(1000); // Simulate a delay for the update
		FlightLogic.UpdateFlightDB();
		Menu.ShowMainMenu();;
	}
}