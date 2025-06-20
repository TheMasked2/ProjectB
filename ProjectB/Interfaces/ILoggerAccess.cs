public interface ILoggerAccess
{
    void WriteLogEntry(string logEntry);
    List<LogEntry> ReadAllLogEntries();
}