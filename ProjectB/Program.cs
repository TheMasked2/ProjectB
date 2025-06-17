public static class Programs
{
	public static void Main()
	{
		System.Console.WriteLine("Updating database...");
		FlightLogic.UpdateFlightDB();
		Menu.ShowMainMenu();
	}
}