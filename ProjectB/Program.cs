public static class Programs
{
	public static void Main()
	{
		System.Console.WriteLine("Updating database...");
		FlightLogic.UpdateFlightDB();
		System.Console.WriteLine("Opening menu...");
		Menu.ShowMainMenu();
	}
}