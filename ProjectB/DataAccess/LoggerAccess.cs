using System.Text;

public class LoggerAccess : ILoggerAccess
{
    private static readonly string LogDirectory = "DataSources";
    private static readonly string AdminActionsLog = Path.Combine(LogDirectory, "admin_actions.csv");

    public LoggerAccess()
    {
        // Create logs directory if it doesn't exist
        if (!Directory.Exists(LogDirectory))
        {
            Directory.CreateDirectory(LogDirectory);
        }

        // Create the log file with headers if it doesn't exist
        if (!File.Exists(AdminActionsLog))
        {
            using (StreamWriter writer = new StreamWriter(AdminActionsLog, false, Encoding.UTF8))
            {
                writer.WriteLine("Timestamp;Action;AdminID;AdminName;TargetUserID;TargetUserName;Details");
            }
        }
    }
    
    public bool WriteLogEntry(string logEntry)
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(AdminActionsLog, true, Encoding.UTF8))
            {
                writer.WriteLine(logEntry);
            }
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error writing to log file: {ex.Message}");
            return false;
        }
    }
    
    public List<LogEntry> ReadAllLogEntries()
    {
        List<LogEntry> entries = new List<LogEntry>();
        
        try
        {
            if (!File.Exists(AdminActionsLog))
            {
                return entries;
            }
            
            using (StreamReader reader = new StreamReader(AdminActionsLog, Encoding.UTF8))
            {
                // Skip the header line
                reader.ReadLine();
                
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;
                        
                    string[] parts = line.Split(';');
                    if (parts.Length >= 7)
                    {
                        LogEntry entry = new LogEntry
                        {
                            Timestamp = parts[0],
                            Action = parts[1],
                            AdminID = parts[2],
                            AdminName = parts[3],
                            TargetUserID = parts[4],
                            TargetUserName = parts[5],
                            Details = parts[6]
                        };
                        entries.Add(entry);
                    }
                }
            }
            
            return entries;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading log file: {ex.Message}");
            return new List<LogEntry>();
        }
    }
}