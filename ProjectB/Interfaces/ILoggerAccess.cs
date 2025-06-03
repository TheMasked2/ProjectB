public interface ILoggerAccess
{
    bool WriteLogEntry(string logEntry);
    List<LogEntry> ReadAllLogEntries();
}