using System.Text;

public class LoggerAccess : ILoggerAccess
{
    private static readonly string LogDirectory = "DataSources";
    private static readonly string AdminActionsLog = Path.Combine(LogDirectory, "admin_actions.csv");

    public LoggerAccess()
    {
        if (!Directory.Exists(LogDirectory))
        {
            Directory.CreateDirectory(LogDirectory);
        }

        if (!File.Exists(AdminActionsLog))
        {
            using (StreamWriter writer = new StreamWriter(AdminActionsLog, false, Encoding.UTF8))
            {
                writer.WriteLine("Timestamp;Action;AdminID;AdminName;TargetUserID;TargetUserName;Details");
            }
        }
    }
    
    public void WriteLogEntry(string logEntry)
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(AdminActionsLog, true, Encoding.UTF8))
            {
                writer.WriteLine(logEntry);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to write log entry: {ex.Message}", ex);
        }
    }
    
    public List<LogEntry> ReadAllLogEntries()
    {
        try
        {
            if (!File.Exists(AdminActionsLog))
            {
                return new List<LogEntry>();
            }
            
            using (StreamReader reader = new StreamReader(AdminActionsLog, Encoding.UTF8))
            {
                List<LogEntry> entries = new List<LogEntry>();
                
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
                
                return entries;
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to read log entries: {ex.Message}", ex);
        }
    }
}