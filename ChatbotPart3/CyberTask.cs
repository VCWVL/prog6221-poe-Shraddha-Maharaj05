using System;

namespace ChatbotPart3
{
    public class CyberTask
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? ReminderDate { get; set; }
        public bool IsCompleted { get; set; }

        public override string ToString()
        {
            string status = IsCompleted ? "✅ Completed" : "🕒 Pending";
            string reminder = ReminderDate.HasValue ? $"⏰ Reminder: {ReminderDate.Value.ToShortDateString()}" : "🔕 No reminder set";

            // Calculate days remaining if there's a reminder date
            string daysRemaining = "";
            if (ReminderDate.HasValue && !IsCompleted)
            {
                int days = (int)(ReminderDate.Value.Date - DateTime.Now.Date).TotalDays;
                if (days == 0)
                    daysRemaining = " (Due today)";
                else if (days < 0)
                    daysRemaining = $" (Overdue by {Math.Abs(days)} day{(Math.Abs(days) != 1 ? "s" : "")})";
                else
                    daysRemaining = $" ({days} day{(days != 1 ? "s" : "")} remaining)";
            }

            return $"{Title} - {Description}\n{status} | {reminder}{daysRemaining}";
        }
    }
}
