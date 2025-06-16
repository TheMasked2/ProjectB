public static class Programs
{
	public static void Main()
	{
		// SessionManager.CurrentUser = new User
        // {
        //     UserID = 0,
        //     FirstName = "Guest",
        //     LastName = "User",
        //     IsAdmin = false,
        //     Guest = true
        // };

		FlightLogic.UpdateFlightDB();
		Menu.ShowMainMenu();
	}
}