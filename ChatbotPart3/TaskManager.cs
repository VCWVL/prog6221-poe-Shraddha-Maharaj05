using ChatbotPart3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChatbotPart3
{
    public class TaskManager
    {
        private readonly TaskService _taskService;
        private readonly DisplayService _displayService;

        public TaskManager(TaskService taskService, DisplayService displayService)
        {
            _taskService = taskService;
            _displayService = displayService;
        }

        public string ProcessTaskCommand(UserProfile userProfile, string input)
        {
            input = input.ToLower().Trim();

            // Add task command
            if (input.StartsWith("add task") || input.Contains("create task"))
            {
                return HandleAddTask(userProfile, input);
            }
            // View tasks command
            else if (input.Contains("view tasks") || input.Contains("show tasks") || input.Contains("list tasks"))
            {
                return _taskService.ViewTasks(userProfile);
            }
            // Complete task command
            else if (input.Contains("complete task") || input.Contains("mark task"))
            {
                return HandleCompleteTask(userProfile, input);
            }
            // Delete task command
            else if (input.Contains("delete task") || input.Contains("remove task"))
            {
                return HandleDeleteTask(userProfile, input);
            }
            // Set reminder for task
            else if (input.Contains("remind") || input.Contains("reminder"))
            {
                return HandleSetReminder(userProfile, input);
            }
            // Task help command
            else if (input.Contains("task help") || (input.Contains("help") && input.Contains("task")))
            {
                return GetTaskHelpText();
            }

            return null; // Not a task-related command
        }

        private string HandleAddTask(UserProfile userProfile, string input)
        {
            // Extract task title after "add task" or "create task"
            string title = ExtractTaskTitle(input);

            if (string.IsNullOrWhiteSpace(title))
            {
                return "Please provide a task title. For example: 'Add task - Update passwords'";
            }

            // Check if the input includes reminder information
            DateTime? reminderDate = null;
            int days = ExtractDaysFromInput(input);
            if (days > 0)
            {
                reminderDate = DateTime.Now.AddDays(days);
            }

            // Generate a description based on the title
            string description = GenerateTaskDescription(title);

            // Add the task with reminder if specified
            return _taskService.AddTask(userProfile, title, description, reminderDate);
        }

        private string HandleCompleteTask(UserProfile userProfile, string input)
        {
            int taskIndex = ExtractTaskNumber(input) - 1; // Convert to zero-based index

            if (taskIndex < 0 || taskIndex >= userProfile.Tasks.Count)
            {
                return "❌ Invalid task number. Use 'View tasks' to see your task list with numbers.";
            }

            return _taskService.MarkTaskComplete(userProfile, taskIndex);
        }

        private string HandleDeleteTask(UserProfile userProfile, string input)
        {
            int taskIndex = ExtractTaskNumber(input) - 1; // Convert to zero-based index

            if (taskIndex < 0 || taskIndex >= userProfile.Tasks.Count)
            {
                return "❌ Invalid task number. Use 'View tasks' to see your task list with numbers.";
            }

            return _taskService.DeleteTask(userProfile, taskIndex);
        }

        private string HandleSetReminder(UserProfile userProfile, string input)
        {
            int taskIndex = ExtractTaskNumber(input) - 1; // Convert to zero-based index

            // If no task number is specified but we have tasks, assume the most recent task
            if (taskIndex < 0 && userProfile.Tasks.Count > 0 && input.Contains("remind"))
            {
                taskIndex = userProfile.Tasks.Count - 1;
            }

            if (taskIndex < 0 || taskIndex >= userProfile.Tasks.Count)
            {
                return "❌ Please specify which task to set a reminder for. Example: 'Remind me about task 2 in 3 days'";
            }

            // Extract days from input (e.g., "in 3 days" or "in 1 week")
            int days = ExtractDaysFromInput(input);

            if (days <= 0)
            {
                return "Please specify when to remind you. Example: 'Remind me about task 2 in 3 days'";
            }

            // Set the reminder date
            userProfile.Tasks[taskIndex].ReminderDate = DateTime.Now.AddDays(days);

            return $"⏰ Reminder set for \"{userProfile.Tasks[taskIndex].Title}\" on {userProfile.Tasks[taskIndex].ReminderDate?.ToShortDateString()}";
        }

        public string CheckForDueReminders(UserProfile userProfile)
        {
            var dueReminders = userProfile.Tasks
                .Where(t => !t.IsCompleted && t.ReminderDate.HasValue && t.ReminderDate.Value.Date <= DateTime.Now.Date)
                .ToList();

            if (dueReminders.Count == 0)
                return null;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("⏰ **Reminder Alert** ⏰");
            sb.AppendLine("The following tasks are due today:");

            foreach (var task in dueReminders)
            {
                sb.AppendLine($"- {task.Title}: {task.Description}");
            }

            return sb.ToString();
        }

        private string ExtractTaskTitle(string input)
        {
            // Handle "Add task - Title" format
            if (input.Contains(" - "))
            {
                string title = input.Substring(input.IndexOf(" - ") + 3).Trim();
                // Remove any reminder text if present
                if (title.Contains(" in "))
                {
                    title = title.Substring(0, title.IndexOf(" in ")).Trim();
                }
                return title;
            }

            // Handle "Add task Title" format
            string[] parts = input.Split(new[] { "add task", "create task" }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0)
            {
                string title = parts[parts.Length - 1].Trim();
                // Remove any reminder text if present
                if (title.Contains(" in "))
                {
                    title = title.Substring(0, title.IndexOf(" in ")).Trim();
                }
                return title;
            }

            return string.Empty;
        }

        private int ExtractTaskNumber(string input)
        {
            // Extract task number from phrases like "task 3" or "task #3"
            if (input.Contains("task"))
            {
                int taskIndex = input.IndexOf("task") + 4;
                if (taskIndex < input.Length)
                {
                    string afterTask = input.Substring(taskIndex).Trim();
                    string digits = new string(afterTask.TakeWhile(char.IsDigit).ToArray());

                    if (int.TryParse(digits, out int taskNumber))
                    {
                        return taskNumber;
                    }
                }
            }

            // Extract any digits as fallback
            string allDigits = new string(input.Where(char.IsDigit).ToArray());

            if (int.TryParse(allDigits, out int number))
            {
                return number;
            }

            return -1;
        }

        private int ExtractDaysFromInput(string input)
        {
            // Handle "in X days"
            if (input.Contains("day"))
            {
                int inIndex = input.IndexOf(" in ");
                if (inIndex >= 0)
                {
                    string afterIn = input.Substring(inIndex + 4);
                    string digits = new string(afterIn.TakeWhile(char.IsDigit).ToArray());

                    if (int.TryParse(digits, out int days))
                    {
                        return days;
                    }
                }
                else
                {
                    // Try to find any number before "day"
                    int dayIndex = input.IndexOf("day");
                    if (dayIndex > 0)
                    {
                        string beforeDay = input.Substring(0, dayIndex);
                        string digits = new string(beforeDay.Where(char.IsDigit).ToArray());

                        if (int.TryParse(digits, out int days))
                        {
                            return days;
                        }
                    }
                }
            }

            // Handle "in X weeks"
            if (input.Contains("week"))
            {
                int inIndex = input.IndexOf(" in ");
                if (inIndex >= 0)
                {
                    string afterIn = input.Substring(inIndex + 4);
                    string digits = new string(afterIn.TakeWhile(char.IsDigit).ToArray());

                    if (int.TryParse(digits, out int weeks))
                    {
                        return weeks * 7; // Convert weeks to days
                    }
                }
                else
                {
                    // Try to find any number before "week"
                    int weekIndex = input.IndexOf("week");
                    if (weekIndex > 0)
                    {
                        string beforeWeek = input.Substring(0, weekIndex);
                        string digits = new string(beforeWeek.Where(char.IsDigit).ToArray());

                        if (int.TryParse(digits, out int weeks))
                        {
                            return weeks * 7; // Convert weeks to days
                        }
                    }
                }
            }

            // Handle "tomorrow"
            if (input.Contains("tomorrow"))
            {
                return 1;
            }

            // Handle "next week"
            if (input.Contains("next week"))
            {
                return 7;
            }

            return -1;
        }

        private string GenerateTaskDescription(string title)
        {
            // Generate appropriate descriptions based on common cybersecurity task titles
            if (title.Contains("password") || title.Contains("pwd"))
            {
                return "Update passwords to be strong and unique across all your accounts.";
            }
            else if (title.Contains("two factor") || title.Contains("2fa"))
            {
                return "Enable two-factor authentication on your important accounts for added security.";
            }
            else if (title.Contains("update") || title.Contains("patch"))
            {
                return "Update your software and operating systems to protect against security vulnerabilities.";
            }
            else if (title.Contains("backup"))
            {
                return "Create secure backups of your important data to protect against ransomware.";
            }
            else if (title.Contains("privacy") || title.Contains("settings"))
            {
                return "Review account privacy settings to ensure your data is protected.";
            }
            else if (title.Contains("scan") || title.Contains("virus") || title.Contains("malware"))
            {
                return "Run a security scan to check for malware or viruses on your devices.";
            }
            else
            {
                return $"Complete this cybersecurity task to improve your online safety.";
            }
        }

        private string GetTaskHelpText()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("📋 **Task Management Commands** 📋");
            sb.AppendLine("- Add task - [title]: Create a new cybersecurity task");
            sb.AppendLine("- Add task - [title] in [X] days: Create task with reminder");
            sb.AppendLine("- View tasks: See all your tasks");
            sb.AppendLine("- Complete task [number]: Mark a task as completed");
            sb.AppendLine("- Delete task [number]: Remove a task");
            sb.AppendLine("- Remind me about task [number] in [X] days: Add a reminder");

            return sb.ToString();
        }
    }
}
