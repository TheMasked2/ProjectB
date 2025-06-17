using Spectre.Console;

public static class Programs
{
	public static void Main()
	{
		AnsiConsole.Clear();
		System.Console.WriteLine("Updating database...");
		Thread.Sleep(1000); // Simulate a delay for the update
		FlightLogic.UpdateFlightDB();
		Menu.ShowMainMenu();
	}
}