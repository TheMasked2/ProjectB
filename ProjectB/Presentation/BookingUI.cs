using Spectre.Console;

public static class BookingUI
{
    private static readonly Style highlightStyle = new(new Color(255, 122, 0));
    private static readonly bool[] BoolChoices = { true, false };

    public static void WaitForKeyPress()
    {
        AnsiConsole.MarkupLine("\n[grey]Press any key to continue...[/]");
        Console.ReadKey(true);
    }
    public static void BookADamnFlight()
    {
        // Display all bookable flights based on user input
        List<FlightModel> bookableFlights = FlightUI.DisplayFilteredFlights();
        if (bookableFlights == null || !bookableFlights.Any())
        {
            return;
        }
        FlightModel selectedFlight = SelectBookableFlight(bookableFlights);
        // Display and handle seatmap and seat selection
        SeatModel selectedSeat = HandleSeatSelection(selectedFlight.FlightID);
        // Build booking details (we have flight and seat, now we need user info and extra options))
        // Ask for extra luggage
        int amountLuggage = PurchaseExtraLuggage();
        // Ask for insurance
        bool insuranceStatus = PurchasheInsurance();
        // Apply discount if applicable (first time booking, senior citizen, coupon)
        Coupons? couponCode = AddCouponCode();
        // If user is logged in, use their info, otherwise prompt for information from guest
        if (SessionManager.CurrentUser.IsGuest)
        {
            UserUI.GuestEditUserInfo();
        }
        User user = SessionManager.CurrentUser;
        // BookingBuilder
        BookingModel booking = BookingLogic.BookingBuilder(
            user,
            selectedFlight,
            selectedSeat,
            couponCode,
            amountLuggage,
            insuranceStatus
        );
        // Display booking details
        DisplayBookingDetails(booking, selectedFlight, user);
        // Ask to confirm booking
        ConfirmBooking(booking, selectedSeat);
    }

    private static SeatModel HandleSeatSelection(int flightID)
    {
        // Get seat map for the selected flight
        // Display seat map
        List<SeatModel> seatMapModelList = SeatMapLogic.GetSeatMap(flightID);
        List<string> seatMap = SeatMapLogic.BuildSeatMapLayout(seatMapModelList);
        DisplaySeatMap(seatMap);
        // Get seat input from user
        SeatModel selectedSeat = SeatInput(seatMapModelList);
        return selectedSeat;
    }
 
    private static FlightModel SelectBookableFlight(List<FlightModel> flights)
    {
        FlightModel flight = null;
        do
        {
            AnsiConsole.MarkupLine("\n[green]Select a flight to book:[/]");
            int flightIdInput = AnsiConsole.Prompt(
                new TextPrompt<int>("[#864000]Flight ID:[/]")
                    .PromptStyle(highlightStyle)
                    .Validate(flightIdInput => flightIdInput > 0));
            flight = flights.FirstOrDefault(f => f.FlightID == flightIdInput);
            if (flight == null)
            {
                AnsiConsole.MarkupLine("[red]Flight not found. Please try again.[/]");
                WaitForKeyPress();
                continue;
            }
        } while (flight == null);

        return flight;
    }

    private static void DisplayBookingDetails(BookingModel booking, FlightModel flight = null, User user = null, bool ComesFromModify = false)
    {
        if (flight == null)
        {
            flight = FlightLogic.GetFlightById(booking.FlightID);
        }
    
        if (user == null)
        {
            user = SessionManager.CurrentUser;
        }

        AirplaneModel airplane = AirplaneLogic.GetAirplaneByID(flight.AirplaneID);

        if (!ComesFromModify)
        {
            AnsiConsole.MarkupLine($"[green]Booking confirmation will be sent to: {user.EmailAddress}[/]");
        }
        AnsiConsole.MarkupLine("[yellow]Booking Details:[/]");
        AnsiConsole.MarkupLine($"[yellow]Seat[/]: [white]{(booking.SeatID.Contains("-") ? booking.SeatID.Split('-')[1] : booking.SeatID)}[/]");
        AnsiConsole.MarkupLine($"[yellow]Airline[/]: [white]{flight.Airline}[/]");
        AnsiConsole.MarkupLine($"[yellow]Airplane Model[/]: [white]{airplane.AirplaneName}[/]");
        AnsiConsole.MarkupLine($"[yellow]Flight ID[/]: [white]{booking.FlightID}[/]");
        AnsiConsole.MarkupLine($"[yellow]From[/]: [white]{flight.DepartureAirport}[/]");
        AnsiConsole.MarkupLine($"[yellow]To[/]: [white]{flight.ArrivalAirport}[/]");
        AnsiConsole.MarkupLine($"[yellow]Departure[/]: [white]{flight.DepartureTime:g}[/]");
        AnsiConsole.MarkupLine($"[yellow]Arrival[/]: [white]{flight.ArrivalTime:g}[/]");
        AnsiConsole.MarkupLine("");
        AnsiConsole.MarkupLine("[yellow]Personal Details:[/]");
        AnsiConsole.MarkupLine($"[yellow]Passenger[/]: [white]{user.FirstName} {user.LastName}[/]");
        AnsiConsole.MarkupLine($"[yellow]Email[/]: [white]{user.EmailAddress ?? "N/A"}[/]");
        AnsiConsole.MarkupLine($"[yellow]Phone[/]: [white]{user.PhoneNumber ?? "N/A"}[/]");
        AnsiConsole.MarkupLine("");
        AnsiConsole.MarkupLine("[yellow]Extra Options:[/]");
        AnsiConsole.MarkupLine($"[yellow]Luggage[/]: [white]{booking.LuggageAmount}[/]");
        AnsiConsole.MarkupLine($"[yellow]Insurance[/]: [white]{(booking.HasInsurance ? "Yes" : "No")}[/]");
        AnsiConsole.MarkupLine("");
        AnsiConsole.MarkupLine("[yellow]Pricing Details:[/]");
        AnsiConsole.MarkupLine(booking.TotalPrice < 0 
            ? $"[yellow]Price[/]: [white]{Math.Abs(booking.TotalPrice)} Imperial units of Spice[/]" 
            : $"[yellow]Price[/]: [white]€{booking.TotalPrice}[/]");
        
        if (booking.Discount < 1.0m)
        {
            AnsiConsole.MarkupLine($"[yellow]Discount[/]: [white]{(1.0m - booking.Discount) * 100:0}%[/]");
        }
    }

    private static int PurchaseExtraLuggage()
    {
        var user = SessionManager.CurrentUser;
        bool extraLuggage = AnsiConsole.Prompt(
            new SelectionPrompt<bool>()
                .Title("[#864000]Would you like to add additional luggage to your booking?\nIt's $50 extra per luggage piece.[/]")
                .AddChoices(new[] { true, false })
                .UseConverter(choice => choice ? "Yes" : "No")
                .HighlightStyle(highlightStyle)
        );
        if (extraLuggage)
        {
            int additionalLuggage = AnsiConsole.Prompt(
                new TextPrompt<int>("[#864000]How many extra pieces of luggage would you like to add?(1 or 2):[/]")
                    .PromptStyle(highlightStyle)
                    .Validate(input => input >= 1 && input <= 2, "Please enter either 1 or 2.")
            );
            AnsiConsole.MarkupLine($"[green]Successfully added {additionalLuggage} extra luggage {(additionalLuggage == 1 ? "piece" : "pieces")} to your booking![/]");
            return additionalLuggage;
        }
        else
        {
            AnsiConsole.MarkupLine("[red]No extra luggage added to your booking.[/]");
        }
        return 0;
    }

    private static bool PurchasheInsurance()
    {
        bool insuranceStatus = AnsiConsole.Prompt(
            new SelectionPrompt<bool>()
                .Title("[#864000]Do you want to purchase travel insurance?\nThis is not required for booking, but you will not be eligible for a refund![/]")
                .AddChoices(new[] { true, false })
                .UseConverter(choice => choice ? "Yes" : "No")
                .HighlightStyle(highlightStyle)
        );
        if (insuranceStatus)
        {
            AnsiConsole.MarkupLine("[green]Succesfully added travel insurance to your booking![/]");
        }
        else
        {
            AnsiConsole.MarkupLine("[red]No travel insurance added to your booking.[/]");
        }
        return insuranceStatus;
    }

    private static Coupons? AddCouponCode()
    {
        bool wantsCoupon = AnsiConsole.Prompt(
            new SelectionPrompt<bool>()
                .Title("[#864000]Do you have a coupon code?[/]")
                .AddChoices(BoolChoices)
                .UseConverter(choice => choice ? "Yes" : "No")
                .HighlightStyle(highlightStyle)
        );

        if (!wantsCoupon)
        {
            AnsiConsole.MarkupLine("[red]No coupon code applied.[/]");
            AnsiConsole.MarkupLine("\n[grey]Press any key to continue...[/]");
            Console.ReadKey(true);
            return null;
        }

        while (true)
        {
            string code = AnsiConsole.Prompt(
                new TextPrompt<string>("[#864000]Enter your coupon code (Leave empty to skip!):[/]")
                    .PromptStyle(highlightStyle)
                    .AllowEmpty()
            ).Trim().ToUpper();

            if (string.IsNullOrEmpty(code))
            {
                AnsiConsole.MarkupLine("[red]No coupon code entered. No discount applied.[/]");
                AnsiConsole.MarkupLine("\n[grey]Press any key to continue...[/]");
                Console.ReadKey(true);
                return null;
            }

            if (Enum.TryParse(code, ignoreCase:true, out Coupons coupon))
            {
                AnsiConsole.MarkupLine("[green]Coupon code applied successfully![/]");
                AnsiConsole.MarkupLine("\n[grey]Press any key to continue...[/]");
                Console.ReadKey(true);
                return coupon;
            }

            AnsiConsole.MarkupLine("[red]Invalid coupon code. Please try again or leave empty to skip.[/]");
        }
    }

    public static void DisplaySeatMap(List<string> seatMap)
    {
        AnsiConsole.MarkupLine("[green]Seat Map:[/]");
        foreach (string seat in seatMap)
            AnsiConsole.MarkupLine(seat);

        AnsiConsole.MarkupLine(
            "[yellow]L[/]=Luxury: $900  [cyan]B[/]=Business: $700  [magenta]P[/]=Premium: $500  [blue]E[/]=Standard Extra Legroom: $400  [green]O[/]=Standard: $300  [red]X[/]=Occupied"
        );
    }

    public static void ConfirmBooking(BookingModel booking, SeatModel selectedSeat)
    {
        bool confirm = AnsiConsole.Prompt(
            new SelectionPrompt<bool>()
                .Title("[#864000]Do you want to confirm the booking?[/]")
                .AddChoices(new[] { true, false })
                .UseConverter(choice => choice ? "Yes" : "No")
                .HighlightStyle(highlightStyle)
        );
        if (confirm)
        {
            string paymentMethod = booking.TotalPrice < 0 ? "SPICE" : AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[#864000]Select payment method:[/]")
                    .AddChoices(new[] { "Credit Card", "Bank Transfer", "PayPal" })
                    .HighlightStyle(highlightStyle)
            );
    
            // Require user to type CONFIRM or CANCEL
            string confirmationInput;
            do
            {
                confirmationInput = AnsiConsole.Prompt(
                    new TextPrompt<string>("[yellow]Type [bold]CONFIRM[/] to finalize or [bold]CANCEL[/] to abort your booking:[/]")
                        .PromptStyle(highlightStyle)
                ).Trim().ToUpper();
    
                if (confirmationInput == "CANCEL")
                {
                    AnsiConsole.MarkupLine("[red]Booking cancelled.[/]");
                    AnsiConsole.MarkupLine("\n[grey]Press any key to continue...[/]");
                    Console.ReadKey(true);
                    return;
                }
            } while (confirmationInput != "CONFIRM");
    
            BookingLogic.BookTheDamnFlight(booking);
            SeatMapLogic.OccupySeat(booking.FlightID, selectedSeat);
    
            AnsiConsole.MarkupLine("[green]Booking confirmed![/]");
            AnsiConsole.MarkupLine($"[green]Payment of {(booking.TotalPrice < 0 ? $"SPICE {Math.Abs(booking.TotalPrice)}" : $"€{booking.TotalPrice}")} processed successfully via {(booking.TotalPrice < 0 ? "SPICE payment" : paymentMethod)}.[/]");
            AnsiConsole.MarkupLine("[yellow]Thank you for booking with Airtreides![/]");
    
            // If user has email, confirm that confirmation will be sent
            if (!string.IsNullOrEmpty(SessionManager.CurrentUser.EmailAddress))
            {
                AnsiConsole.MarkupLine($"[green]Booking confirmation has been sent to: {SessionManager.CurrentUser.EmailAddress}[/]");
            }
        }
        else
        {
            AnsiConsole.MarkupLine("[red]Booking cancelled.[/]");
        }
        AnsiConsole.MarkupLine("\n[grey]Press any key to continue...[/]");
        Console.ReadKey(true);
    }

    public static SeatModel SeatInput(List<SeatModel> seats)
    {
        string seatInput;
        SeatModel? selectedSeat;
        do
        {
            seatInput = AnsiConsole.Prompt(
            new TextPrompt<string>("[#864000]Enter seat (e.g., 12A or A12):[/]")
                .PromptStyle(highlightStyle)
            ).Trim();
            selectedSeat = SeatMapLogic.ValidateSeatInput(seatInput, seats);
            if (selectedSeat == null || selectedSeat.IsOccupied)
            {
                AnsiConsole.MarkupLine("[red]Invalid seat input or seat is occupied. Please try again.[/]");
                continue;
            }
        } while (selectedSeat == null || selectedSeat.IsOccupied);
        return selectedSeat;
    }

    public static void CancelBookingPrompt()
    {
        if (!SessionManager.IsLoggedIn())
        {
            AnsiConsole.MarkupLine("[red]You must be logged in to cancel a booking.[/]");
            return;
        }

        var user = SessionManager.CurrentUser;
        List<BookingModel> bookings = BookingLogic.GetBookingsForUser(user.UserID, true);
        if (!bookings.Any())
        {
            AnsiConsole.MarkupLine("[yellow]You have no upcoming bookings to cancel.[/]");
            return;
        }

        Spectre.Console.Rendering.IRenderable bookingTable = BookingLogic.CreateBookingTable(bookings);
        AnsiConsole.Write(bookingTable);

        int bookingId = AnsiConsole.Prompt(
            new TextPrompt<int>("[#864000]Enter the Booking ID to cancel:[/]")
                .PromptStyle(highlightStyle)
                .Validate(id => bookings.Any(b => b.BookingID == id), "[red]Invalid Booking ID.[/]") // You have to pick a valid flight or you are stuck!
        );

        var selectedBooking = bookings.First(b => b.BookingID == bookingId);
        if(selectedBooking.BookingStatus == "Cancelled")
        {
            AnsiConsole.MarkupLine("[red]This booking has already been cancelled.[/]");
            return;
        }

        // Inform the user about cancellation terms based on insurance status
        if (!selectedBooking.HasInsurance)
        {
            var confirm = AnsiConsole.Prompt(
                new SelectionPrompt<bool>()
                    .Title("[#864000]This booking does not have insurance. A cancellation fee of €100 will apply. Do you want to continue?[/]")
                    .AddChoices(new[] { true, false })
                    .UseConverter(choice => choice ? "Yes" : "No")
                    .HighlightStyle(highlightStyle)
            );

            if (!confirm)
            {
                AnsiConsole.MarkupLine("[yellow]Cancellation aborted.[/]");
                return;
            }
        }

        (bool cancelled, bool freeCancel) = BookingLogic.CancelBooking(bookingId);

        DisplayBookingDetails(selectedBooking);
        
        if (cancelled)
        {
            AnsiConsole.MarkupLine("[green]Booking successfully cancelled![/]");

            if (freeCancel)
            {
                AnsiConsole.MarkupLine("[green]Since you had insurance, you will receive a full refund.[/]");
            }
            else
            {
                AnsiConsole.MarkupLine("[yellow]A cancellation fee of €100 has been applied to your refund.[/]");
            }

        }
        else
        {
            AnsiConsole.MarkupLine("[red]Cancellation failed.[/]");
        }
        
    }

    public static void ModifyBookingPrompt()
    {
        if (!SessionManager.IsLoggedIn())
        {
            AnsiConsole.MarkupLine("[red]You must be logged in to modify a booking.[/]");
            return;
        }

        User user = SessionManager.CurrentUser;
        List<BookingModel> bookings = BookingLogic.GetBookingsForUser(user.UserID, true);
        if (!bookings.Any())
        {
            AnsiConsole.MarkupLine("[yellow]You have no upcoming bookings to modify.[/]");
            return;
        }

        Spectre.Console.Rendering.IRenderable bookingTable = BookingLogic.CreateBookingTable(bookings);
        AnsiConsole.Write(bookingTable);

        int bookingId = AnsiConsole.Prompt(
            new TextPrompt<int>("[#864000]Enter the Booking ID to modify:[/]")
                .PromptStyle(highlightStyle)
                .Validate(id => bookings.Any(b => b.BookingID == id), "[red]Invalid Booking ID.[/]")
        );

        BookingModel selectedBooking = bookings.First(b => b.BookingID == bookingId);
        DisplayBookingDetails(selectedBooking, user:user, ComesFromModify:true);

        SeatModel selectedSeat = HandleSeatSelection(selectedBooking.FlightID);

        bool successfulModification = BookingLogic.ModifyBooking(
            bookingId,
            selectedSeat,
            selectedBooking.LuggageAmount
        );

        if (successfulModification)
        {
            AnsiConsole.MarkupLine($"[green]Booking with Booking ID: {selectedBooking.BookingID} successfully modified![/]");
            AnsiConsole.MarkupLine("[green]Your previous payment will be refunded.[/]");
            BookingModel updatedModel = BookingLogic.GetBookingById(bookingId);
            DisplayBookingDetails(updatedModel, ComesFromModify:true);
            AnsiConsole.MarkupLine("[green]A modification fee of $50 has been applied.[/]");
            AnsiConsole.MarkupLine($"[green]Your new total: {updatedModel.TotalPrice + 50}[/]");
        }
        else
        {
            AnsiConsole.MarkupLine("[red]Modification failed.[/]");
        }
    }

    public static void ViewUserBookings(bool upcoming)
    {
        if (!SessionManager.IsLoggedIn())
        {
            AnsiConsole.MarkupLine("[red]You must be logged in to view bookings.[/]");
            WaitForKeyPress();
            return;
        }
        var user = SessionManager.CurrentUser;
        var bookings = BookingLogic.GetBookingsForUser(user.UserID, upcoming);
        if (!bookings.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No bookings found.[/]");
            WaitForKeyPress();
            return;
        }
        Spectre.Console.Rendering.IRenderable bookingTable = BookingLogic.CreateBookingTable(bookings);
        AnsiConsole.Write(bookingTable);
        WaitForKeyPress();
    }
}