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
                    .AddChoices(choices));

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
        bool succes = false;
        do
        {
            int Rating = AnsiConsole.Prompt(
                new TextPrompt<int>("[#864000]Please enter the rating of the review (1-5):[/]")
                    .PromptStyle(highlightStyle));

            string Content = AnsiConsole.Prompt(
                new TextPrompt<string>("[#864000]Please enter the content of the review:[/]")
                    .PromptStyle(highlightStyle));

            int FlightID = AnsiConsole.Prompt(
            new TextPrompt<int>("[#864000]Please enter the flight id of the review:[/]")
                .PromptStyle(highlightStyle));

            ReviewModel Review = new ReviewModel(SessionManager.CurrentUser.UserID, FlightID, Content, Rating);
            succes = ReviewLogic.AddReview(Review);

            if (succes)
            {
                AnsiConsole.MarkupLine($"[green]Review added successfully![/]");
            }
            else
            {
                AnsiConsole.MarkupLine($"[red]Failed to add review![/]");
            }
            FlightUI.WaitForKeyPress();
        } while (!succes);
    }
    
    public static void ViewReviews()
    {
        List<ReviewModel> reviews = ReviewLogic.GetAllReviews();

        foreach (var review in reviews)
        {
            // var UserModel = UserAccess.GetUserInfoByID(review.UserID);
            var UserModel = UserLogic.GetUserByID(review.UserID);
            var FlightModel = FlightLogic.GetFlightById(review.FlightID);

            if (UserModel == null || FlightModel == null)
            {
                continue;
            }

            string goldStars = string.Join(" ", Enumerable.Repeat("★", review.Rating));
            string grayStars = string.Join(" ", Enumerable.Repeat("☆", 5 - review.Rating));
            
            string profile = $"[rgb(134,64,0)]Firstname:[/][rgb(255,122,0)]{UserModel.FirstName}[/]\n" +
                             $"[rgb(134,64,0)]Date:[/]     [rgb(255,122,0)]{review.CreatedAt:yyyy-MM-dd}[/]\n" +
                             $"[rgb(134,64,0)]Airline:[/]  [rgb(255,122,0)]{FlightModel.Airline}[/]\n" +
                             $"[rgb(134,64,0)]Departure:[/][rgb(255,122,0)]{FlightModel.DepartureAirport}[/]\n" +
                             $"[rgb(134,64,0)]Arrival:[/]  [rgb(255,122,0)]{FlightModel.ArrivalAirport}[/]\n" +
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
        FlightUI.WaitForKeyPress();
    }

        public static void FilterViewReviews()
    {
        int flightId = AnsiConsole.Prompt(
                new TextPrompt<int>("[#864000]Please enter the flight id to filter reviews:[/]")
                    .PromptStyle(highlightStyle));
                    
        List<ReviewModel> reviews = ReviewLogic.FilterReviewsByFlightID(flightId);

        foreach (var review in reviews)
        {
            var UserModel = UserLogic.GetUserByID(review.UserID);
            var FlightModel = FlightLogic.GetFlightById(review.FlightID);

            string goldStars = string.Join(" ", Enumerable.Repeat("★", review.Rating));
            string grayStars = string.Join(" ", Enumerable.Repeat("☆", 5 - review.Rating));

            string profile = $"[rgb(134,64,0)]Firstname:[/][rgb(255,122,0)]{UserModel.FirstName}[/]\n" +
                             $"[rgb(134,64,0)]Date:[/]     [rgb(255,122,0)]{review.CreatedAt:yyyy-MM-dd}[/]\n" +
                             $"[rgb(134,64,0)]Airline:[/]  [rgb(255,122,0)]{FlightModel.Airline}[/]\n" +
                             $"[rgb(134,64,0)]Departure:[/][rgb(255,122,0)]{FlightModel.DepartureAirport}[/]\n" +
                             $"[rgb(134,64,0)]Arrival:[/]  [rgb(255,122,0)]{FlightModel.ArrivalAirport}[/]\n" +
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
        FlightUI.WaitForKeyPress();
    }
}
