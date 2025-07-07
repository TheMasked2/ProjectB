using Spectre.Console;

public static class ReviewUI
{
    private static readonly Style highlightStyle = new(new Color(255, 122, 0));
    public static void ShowReviewMenu()
    {
        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(
                new FigletText("Reviews")
                    .Centered()
                    .Color(Color.Orange1));

            var choices = new List<string>
            {
                "Add a review",
                "View reviews",
                "View reviews by flight",
                "Back to main menu"
            };

            var input = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]Select an option:[/]")
                    .PageSize(10)
                    .AddChoices(choices)
                    .WrapAround(true));

            switch (input)
            {
                case "Add a review":
                    MakeAReview();
                    break;
                case "View reviews":
                    ViewReviews();
                    break;
                case "View reviews by flight":
                    FilterViewReviews();
                    break;
                case "Back to main menu":
                    return;
            }
        }
    }

    public static void MakeAReview()
    {
        int flightID = AnsiConsole.Prompt(
            new TextPrompt<int>("[#864000]Please enter the flight ID you wish to review:[/]")
                .PromptStyle(highlightStyle)
                .Validate(id =>
                {
                    FlightModel? flight = FlightLogic.GetFlightById(id);
                    if (flight == null)
                    {
                        return ValidationResult.Error("[red]Flight not found.[/]");
                    }
                    if (flight.Status != "Departed")
                    {
                        return ValidationResult.Error($"[red]You can't review a flight that hasn't flown yet. This flight's status is '{flight.Status}'.[/]");
                    }
                    return ValidationResult.Success();
                })
        );

        // If we get here, the flightID is valid. Now get the rest of the review details.
        int rating = AnsiConsole.Prompt(
            new TextPrompt<int>("[#864000]Please enter the rating for the review (1-5):[/]")
                .PromptStyle(highlightStyle)
                .ValidationErrorMessage("[red]Invalid rating. Please enter a whole number between 1 and 5.[/]")
                .Validate(r => r >= 1 && r <= 5)
        );

        string content = AnsiConsole.Prompt(
            new TextPrompt<string>("[#864000]Please enter the content of the review:[/]")
                .PromptStyle(highlightStyle));

        ReviewModel review = new ReviewModel(SessionManager.CurrentUser.UserID, flightID, content, rating);

        string errorMessage;
        if (ReviewLogic.AddReview(review, out errorMessage))
        {
            AnsiConsole.MarkupLine("[green]Review added successfully![/]");
            FlightUI.WaitForKeyPress();
        }
        else
        {
            AnsiConsole.MarkupLine($"[red]Error: {errorMessage}[/]");
            FlightUI.WaitForKeyPress();
        }
    }
    
    public static void ViewReviews()
    {
        string errorMessage;
        List<ReviewModel> reviews = ReviewLogic.GetAllReviews(out errorMessage);

        if (!string.IsNullOrEmpty(errorMessage))
        {
            AnsiConsole.MarkupLine($"[red]{errorMessage}[/]");
            FlightUI.WaitForKeyPress();
            return;
        }

        DisplayReviews(reviews);
        FlightUI.WaitForKeyPress();
    }

    public static void FilterViewReviews()
    {
        int flightId = AnsiConsole.Prompt(
                new TextPrompt<int>("[#864000]Please enter the flight id to filter reviews:[/]")
                    .PromptStyle(highlightStyle));
                    
        string errorMessage;
        List<ReviewModel> reviews = ReviewLogic.FilterReviewsByFlightID(flightId, out errorMessage);

        if (!string.IsNullOrEmpty(errorMessage))
        {
            AnsiConsole.MarkupLine($"[red]{errorMessage}[/]");
            FlightUI.WaitForKeyPress();
            return;
        }

        DisplayReviews(reviews);
        FlightUI.WaitForKeyPress();
    }

    private static void DisplayReviews(List<ReviewModel> reviews)
    {
        if (reviews == null || !reviews.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No reviews found.[/]");
            return;
        }

        foreach (var review in reviews)
        {
            var userModel = UserLogic.GetUserByID(review.UserID);
            var flightModel = FlightLogic.GetFlightById(review.FlightID);

            if (userModel == null || flightModel == null)
            {
                continue;
            }

            string goldStars = string.Join(" ", Enumerable.Repeat("★", (int)review.Rating));
            string grayStars = string.Join(" ", Enumerable.Repeat("☆", 5 - (int)review.Rating));
            
            string profile = $"[rgb(134,64,0)]Firstname:[/][rgb(255,122,0)]{userModel.FirstName}[/]\n" +
                             $"[rgb(134,64,0)]Date:[/]     [rgb(255,122,0)]{review.CreatedAt:yyyy-MM-dd}[/]\n" +
                             $"[rgb(134,64,0)]Airline:[/]  [rgb(255,122,0)]{flightModel.Airline}[/]\n" +
                             $"[rgb(134,64,0)]Departure:[/][rgb(255,122,0)]{flightModel.DepartureAirport}[/]\n" +
                             $"[rgb(134,64,0)]Arrival:[/]  [rgb(255,122,0)]{flightModel.ArrivalAirport}[/]\n" +
                             $"[rgb(134,64,0)]Rating:[/]   [rgb(255,122,0)]{goldStars + " " + grayStars}[/]\n" +
                             $"[rgb(134,64,0)]Content:[/]  [rgb(255,122,0)]{review.Content}[/]";

            var panel = new Panel(profile)
                .Header("[rgb(134,64,0)]Review[/]")
                .HeaderAlignment(Justify.Center)
                .Border(BoxBorder.Rounded)
                .BorderStyle(new Style(new Color(184, 123, 74)))
                .Padding(new Padding(1, 1, 1, 1));

            AnsiConsole.Write(panel);
        }
    }
}
