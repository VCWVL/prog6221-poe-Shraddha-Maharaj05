using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ChatbotPart3
{
    public class TaskManager
    {
        private readonly TaskService _taskService;
        private readonly DisplayService _displayService;

        // NLP patterns for task commands
        private readonly Dictionary<string, List<string>> _taskCommandPatterns = new Dictionary<string, List<string>>
        {
            {
                "add_task", new List<string>
                {
                    "add task", "create task", "new task", "make task", "add a task", "create a task",
                    "add reminder", "create reminder", "set reminder", "remind me to", "need to remember",
                    "i need to", "i should", "can you remind me", "don't let me forget", "help me remember"
                }
            },
            {
                "view_tasks", new List<string>
                {
                    "view tasks", "show tasks", "list tasks", "see tasks", "display tasks", "what are my tasks",
                    "show my tasks", "view my tasks", "list my tasks", "show all tasks", "what tasks do i have",
                    "show reminders", "view reminders", "what do i need to do", "my to-do list", "my todo list"
                }
            },
            {
                "complete_task", new List<string>
                {
                    "complete task", "mark task", "finish task", "done with task", "task complete", "task finished",
                    "mark as done", "mark as complete", "mark as completed", "i completed", "i finished",
                    "task is done", "completed task", "finished task", "mark task as done"
                }
            },
            {
                "delete_task", new List<string>
                {
                    "delete task", "remove task", "cancel task", "get rid of task", "erase task",
                    "delete reminder", "remove reminder", "cancel reminder", "i don't need to", "no longer need to"
                }
            },
            {
                "set_reminder", new List<string>
                {
                    "remind me", "set reminder", "add reminder", "create reminder", "remind me about",
                    "alert me", "notify me", "don't let me forget", "remember to", "remind me to"
                }
            },
            {
                "task_help", new List<string>
                {
                    "task help", "help with tasks", "how to use tasks", "task commands", "task instructions",
                    "how do i add tasks", "how do tasks work", "help me with tasks", "task assistance"
                }
            },
            {
                "activity_log", new List<string>
                {
                    "activity log", "show log", "view log", "show activity", "view activity",
                    "show history", "view history", "what have i done", "show actions"
                }
            }
        };

        // Cybersecurity-related keywords for task title extraction
        private readonly List<string> _cybersecurityKeywords = new List<string>
        {
            "password", "2fa", "two factor", "authentication", "backup", "update", "patch",
            "privacy", "settings", "scan", "virus", "malware", "firewall", "security",
            "encryption", "vpn", "phishing", "suspicious", "links", "email", "social engineering",
            "identity theft", "data breach", "secure", "protect", "antivirus", "review", "check"
        };

        public TaskManager(TaskService taskService, DisplayService displayService)
        {
            _taskService = taskService;
            _displayService = displayService;
        }

        public string ProcessTaskCommand(UserProfile userProfile, string input)
        {
            input = input.ToLower().Trim();

            // Special handling for activity log request
            if (IsActivityLogRequest(input))
            {
                return userProfile.GetActivityLogSummary();
            }

            // Special handling for task viewing requests
            if (IsViewTasksRequest(input))
            {
                userProfile.LogActivity("Viewed task list", "Task", "Displayed all tasks");
                return _taskService.ViewTasks(userProfile);
            }

            // Detect command type using NLP patterns
            string commandType = DetectTaskCommandType(input);
            if (commandType == null) return null; // Not a task-related command

            return commandType switch
            {
                "add_task" => HandleAddTask(userProfile, input),
                "complete_task" => HandleCompleteTask(userProfile, input),
                "delete_task" => HandleDeleteTask(userProfile, input),
                "set_reminder" => HandleSetReminder(userProfile, input),
                "task_help" => HandleTaskHelp(),
                _ => null
            };
        }

        private bool IsActivityLogRequest(string input)
        {
            return _taskCommandPatterns["activity_log"].Any(pattern => input.Contains(pattern)) ||
                   (input.Contains("show") && input.Contains("log")) ||
                   input.Contains("what have you done for me");
        }

        private bool IsViewTasksRequest(string input)
        {
            return _taskCommandPatterns["view_tasks"].Any(pattern => input.Contains(pattern));
        }

        private string HandleAddTask(UserProfile userProfile, string input)
        {
            // Extract task title using NLP techniques
            string title = ExtractTaskTitle(input);

            if (string.IsNullOrWhiteSpace(title))
            {
                return "Please provide a task title. For example: 'Add task - Update passwords'";
            }

            // Check if the input includes reminder information
            DateTime? reminderDate = ExtractReminderDate(input);

            // Generate a description based on the title
            string description = GenerateTaskDescription(title);

            // Log the activity
            string reminderInfo = reminderDate.HasValue ? $" with reminder for {reminderDate.Value.ToShortDateString()}" : "";
            userProfile.LogActivity("Added task", "Task", $"'{title}'{reminderInfo}");

            // Add the task with reminder if specified
            return _taskService.AddTask(userProfile, title, description, reminderDate);
        }

        private DateTime? ExtractReminderDate(string input)
        {
            int daysForReminder = ExtractDaysFromInput(input);
            return daysForReminder > 0 ? DateTime.Now.AddDays(daysForReminder) : (DateTime?)null;
        }

        private string HandleCompleteTask(UserProfile userProfile, string input)
        {
            int taskIndex = ExtractTaskNumber(input) - 1; // Convert to zero-based index
            if (taskIndex < 0 || taskIndex >= userProfile.Tasks.Count)
            {
                return "❌ Invalid task number. Use 'View tasks' to see your task list with numbers.";
            }

            userProfile.LogActivity("Completed task", "Task", $"Marked '{userProfile.Tasks[taskIndex].Title}' as completed");
            return _taskService.MarkTaskComplete(userProfile, taskIndex);
        }

        private string HandleDeleteTask(UserProfile userProfile, string input)
        {
            int taskIndex = ExtractTaskNumber(input) - 1; // Convert to zero-based index
            if (taskIndex < 0 || taskIndex >= userProfile.Tasks.Count)
            {
                return "❌ Invalid task number. Use 'View tasks' to see your task list with numbers.";
            }

            string taskTitle = userProfile.Tasks[taskIndex].Title;
            userProfile.LogActivity("Deleted task", "Task", $"Removed '{taskTitle}'");
            return _taskService.DeleteTask(userProfile, taskIndex);
        }

        private string HandleSetReminder(UserProfile userProfile, string input)
        {
            int taskIndex = ExtractTaskNumber(input) - 1; // Convert to zero-based index
            if (taskIndex < 0 || taskIndex >= userProfile.Tasks.Count)
            {
                return "❌ Please specify which task to set a reminder for. Example: 'Remind me about task 2 in 3 days'";
            }

            DateTime? reminderDate = ExtractReminderDate(input);
            if (!reminderDate.HasValue)
            {
                return "❌ Please specify when to remind you. Example: 'Remind me about task 2 in 3 days'";
            }

            userProfile.Tasks[taskIndex].ReminderDate = reminderDate;
            userProfile.LogActivity("Set reminder", "Reminder", $"For '{userProfile.Tasks[taskIndex].Title}' on {reminderDate.Value.ToShortDateString()}");
            return $"⏰ Reminder set for \"{userProfile.Tasks[taskIndex].Title}\" on {reminderDate.Value.ToShortDateString()}";
        }

        private string HandleTaskHelp()
        {
            return GetTaskHelpText();
        }

        private string DetectTaskCommandType(string input)
        {
            foreach (var pattern in _taskCommandPatterns)
            {
                if (pattern.Value.Any(phrase => input.Contains(phrase)))
                {
                    return pattern.Key;
                }
            }
            return null;
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

            // Use NLP to extract task title
            string cleanedInput = input;

            // Special handling for "need to remember" pattern
            if (input.Contains("need to remember to"))
            {
                return input.Substring(input.IndexOf("need to remember to") + 19).Trim();
            }

            if (input.Contains("remind me to"))
            {
                return input.Substring(input.IndexOf("remind me to") + 12).Trim();
            }

            // Remove time-related phrases first
            string[] timePatterns = { "tomorrow", "next week", "in a day", "in a week", "in one day", "in one week" };
            foreach (var pattern in timePatterns)
            {
                cleanedInput = cleanedInput.Replace(pattern, "").Trim();
            }

            // Remove phrases like "in X days" or "in X weeks"
            cleanedInput = Regex.Replace(cleanedInput, @"in \d+ days?", "").Trim();
            cleanedInput = Regex.Replace(cleanedInput, @"in \d+ weeks?", "").Trim();

            // Now remove command patterns
            foreach (var patterns in _taskCommandPatterns.Values)
            {
                foreach (var pattern in patterns)
                {
                    cleanedInput = cleanedInput.Replace(pattern, "").Trim();
                }
            }

            // Remove common filler words
            string[] fillerWords = { "please", "could you", "can you", "i want to", "i need to", "about", "for me", "for", "to", "?" };
            foreach (var word in fillerWords)
            {
                cleanedInput = cleanedInput.Replace(word, "").Trim();
            }

            // Clean up extra spaces and punctuation
            cleanedInput = Regex.Replace(cleanedInput, @"\s+", " ").Trim();
            cleanedInput = cleanedInput.Trim(',', '.', '!', '?', ':', ';');

            if (!string.IsNullOrWhiteSpace(cleanedInput))
            {
                return char.ToUpper(cleanedInput[0]) + cleanedInput.Substring(1);
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

            // Look for number patterns like "#3" or "number 3"
            Match numberMatch = Regex.Match(input, @"#(\d+)");
            if (numberMatch.Success)
            {
                if (int.TryParse(numberMatch.Groups[1].Value, out int taskNumber))
                {
                    return taskNumber;
                }
            }

            numberMatch = Regex.Match(input, @"number (\d+)");
            if (numberMatch.Success)
            {
                if (int.TryParse(numberMatch.Groups[1].Value, out int taskNumber))
                {
                    return taskNumber;
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

            // Handle "in X days"
            Match daysMatch = Regex.Match(input, @"in (\d+) days?");
            if (daysMatch.Success)
            {
                if (int.TryParse(daysMatch.Groups[1].Value, out int extractedDays))
                {
                    return extractedDays;
                }
            }

            // Handle "in X weeks"
            Match weeksMatch = Regex.Match(input, @"in (\d+) weeks?");
            if (weeksMatch.Success)
            {
                if (int.TryParse(weeksMatch.Groups[1].Value, out int weeks))
                {
                    return weeks * 7; // Convert weeks to days
                }
            }

            // Handle "in a day" or "in one day"
            if (input.Contains("in a day") || input.Contains("in one day"))
            {
                return 1;
            }

            // Handle "in a week" or "in one week"
            if (input.Contains("in a week") || input.Contains("in one week"))
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
            else if (title.Contains("review") || title.Contains("check"))
            {
                return "Review your current settings and ensure they align with best practices for security.";
            }
            else
            {
                return $"Complete this cybersecurity task to improve your online safety.";
            }
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

            // Log the activity
            userProfile.LogActivity("Reminder alert", "Reminder", $"{dueReminders.Count} tasks due today");

            return sb.ToString();
        }

        // Task guidance to user
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
            sb.AppendLine("- Show activity log: View your recent actions");
            sb.AppendLine();
            sb.AppendLine("You can also use natural language like:");
            sb.AppendLine("- \"Remind me to update my passwords tomorrow\"");
            sb.AppendLine("- \"I need to enable two-factor authentication\"");
            sb.AppendLine("- \"What tasks do I have?\"");
            sb.AppendLine("- \"I finished task 2\"");
            sb.AppendLine("- \"What have you done for me?\"");

            return sb.ToString();
        }
    }
}
