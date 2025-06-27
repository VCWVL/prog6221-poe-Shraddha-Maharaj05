using System;

namespace ChatbotPart3
{
    public class ActivityLogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Action { get; set; }
        public string Category { get; set; }
        public string Details { get; set; }

        public ActivityLogEntry(string action, string category, string details = "")
        {
            Timestamp = DateTime.Now;
            Action = action;
            Category = category;
            Details = details;
        }

        public override string ToString()
        {
            string formattedTime = Timestamp.ToString("yyyy-MM-dd HH:mm");
            string detailsText = string.IsNullOrEmpty(Details) ? "" : $" - {Details}";

            return $"[{formattedTime}] {Category}: {Action}{detailsText}";
        }

        public string GetShortDescription()
        {
            string detailsText = string.IsNullOrEmpty(Details) ? "" : $" ({Details})";
            return $"{Action}{detailsText}";
        }
    }
}
