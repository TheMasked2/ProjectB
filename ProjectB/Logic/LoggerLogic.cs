using System;
using System.Collections.Generic;
using System.Text;

public static class Logger
{
    public static ILoggerAccess LoggerAccessService { get; set; } = new LoggerAccess();
    public static bool LogUserCreation(User adminUser, User createdUser)
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string action = "CREATE_USER";
        string adminId = adminUser.UserID.ToString();
        string adminName = $"{adminUser.FirstName} {adminUser.LastName}";
        string targetId = createdUser.UserID.ToString();
        string targetName = $"{createdUser.FirstName} {createdUser.LastName}";
        string details = $"Email={createdUser.EmailAddress}|Admin={createdUser.IsAdmin}";

        string logEntry = $"{timestamp};{action};{adminId};{adminName};{targetId};{targetName};{details}";

        return LoggerAccessService.WriteLogEntry(logEntry);
    }


    public static bool LogUserEdit(User adminUser, User editedUser, Dictionary<string, string> changedFields)
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string action = "EDIT_USER";
        string adminId = adminUser.UserID.ToString();
        string adminName = $"{adminUser.FirstName} {adminUser.LastName}";
        string targetId = editedUser.UserID.ToString();
        string targetName = $"{editedUser.FirstName} {editedUser.LastName}";

        // Show old_value -> new_value based on changed fields
        StringBuilder detailsBuilder = new StringBuilder();
        foreach (var change in changedFields)
        {
            detailsBuilder.Append($"{change.Key}={change.Value}|");
        }
        string details = detailsBuilder.ToString().TrimEnd('|');

        string logEntry = $"{timestamp};{action};{adminId};{adminName};{targetId};{targetName};{details}";

        return LoggerAccessService.WriteLogEntry(logEntry);
    }

    public static List<LogEntry> ReadLogEntries()
    {
        var entries = LoggerAccessService.ReadAllLogEntries();
        // Order by timestamp descending (newest first)
        return entries.OrderByDescending(e => DateTime.TryParse(e.Timestamp, out var dt) ? dt : DateTime.MinValue).ToList();
    }
}