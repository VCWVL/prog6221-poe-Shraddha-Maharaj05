using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChatbotPart3
{
    public class TaskService
    {
        public string AddTask(UserProfile profile, string title, string description, DateTime? reminder = null)
        {
            var task = new CyberTask
            {
                Title = title,
                Description = description,
                ReminderDate = reminder,
                IsCompleted = false
            };

            profile.Tasks.Add(task);

            string response = $"📝 Task added: {title}\n{description}";

            if (reminder.HasValue)
            {
                response += $"\n⏰ Reminder set for {reminder.Value.ToShortDateString()}";
            }
            else
            {
                response += "\nTip: You can set a reminder by saying 'Remind me about task " + profile.Tasks.Count + " in 3 days'";
            }

            return response;
        }

        public string ViewTasks(UserProfile profile)
        {
            if (profile.Tasks.Count == 0)
                return "📭 You have no cybersecurity tasks yet. Try adding one with 'Add task - [title]'";

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("📋 Your Cybersecurity Tasks:");

            for (int i = 0; i < profile.Tasks.Count; i++)
            {
                sb.AppendLine($"{i + 1}. {profile.Tasks[i]}");

                // Add a blank line between tasks for readability
                if (i < profile.Tasks.Count - 1)
                    sb.AppendLine();
            }

            return sb.ToString();
        }

        public string MarkTaskComplete(UserProfile profile, int index)
        {
            if (index < 0 || index >= profile.Tasks.Count)
                return "❌ Invalid task number. Use 'View tasks' to see your task list with numbers.";

            profile.Tasks[index].IsCompleted = true;
            return $"✅ Marked \"{profile.Tasks[index].Title}\" as completed.";
        }

        public string DeleteTask(UserProfile profile, int index)
        {
            if (index < 0 || index >= profile.Tasks.Count)
                return "❌ Invalid task number. Use 'View tasks' to see your task list with numbers.";

            string removedTitle = profile.Tasks[index].Title;
            profile.Tasks.RemoveAt(index);
            return $"🗑️ Deleted task: {removedTitle}";
        }
    }
}
