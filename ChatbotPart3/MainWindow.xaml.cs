using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace ChatbotPart3
{
    public partial class MainWindow : Window
    {
        private readonly GreetingService _greetingService = new GreetingService();
        private readonly QuestionService _questionService = new QuestionService();
        private readonly TopicService _topicService = new TopicService();
        private readonly DisplayService _displayService = new DisplayService();
        private readonly TaskService _taskService = new TaskService();
        private readonly TaskManager _taskManager;
        private readonly QuizService _quizService = new QuizService();
        private readonly QuizManager _quizManager;

        private UserProfile _userProfile = new UserProfile();
        private List<string> _userInquiries = new List<string>();
        private string currentState = "askName";
        private string currentTopic = null;

        // NLP patterns for action summary request
        private readonly List<string> _summaryRequestPatterns = new List<string>
        {
            "what have you done", "what did you do", "show me what you've done", "list actions",
            "show actions", "what tasks", "show my tasks", "what reminders", "show my reminders",
            "what have you done for me", "what have you helped me with", "show summary", "action summary"
        };

        // NLP patterns for activity log request
        private readonly List<string> _activityLogRequestPatterns = new List<string>
        {
            "activity log", "show log", "view log", "show activity", "view activity",
            "show history", "view history", "what have i done", "show actions"
        };

        public MainWindow()
        {
            InitializeComponent();
            _taskManager = new TaskManager(_taskService, _displayService);
            _quizManager = new QuizManager(_quizService, _displayService);
            StartChat();
        }

        private void StartChat()
        {
            AppendChat("Welcome to CyberBot!");
            _greetingService.PlayWelcomeSound();
            AppendChat(_displayService.GetAsciiArt());
            AppendChat("Please enter your name:");
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string userInput = UserInputBox.Text.Trim();
            if (string.IsNullOrEmpty(userInput))
            {
                AppendChat("⚠️ Please enter a valid message.");
                return;
            }

            try
            {
                AppendChat($"> {userInput}");
                UserInputBox.Clear();

                switch (currentState)
                {
                    case "askName":
                        _userProfile.Name = userInput;
                        // Log the start of the session
                        _userProfile.LogActivity("Started session", "System", $"User: {_userProfile.Name}");
                        AppendChat(_displayService.GetWelcomeMessageBox(_userProfile.Name));
                        currentState = "askQuestions";
                        AppendChat("Let's begin a few quick questions to get to know your needs.");
                        AppendChat(_questionService.GetNextQuestion());
                        break;

                    case "askQuestions":
                        string response = _questionService.ProcessAnswer(userInput, _userProfile);
                        CleanUserProfileTopics();
                        AppendChat(response);

                        if (_questionService.IsQuestionnaireComplete())
                        {
                            currentState = "mainChat";
                            // Log the completion of the questionnaire
                            _userProfile.LogActivity("Completed questionnaire", "System", $"Knowledge level: {_userProfile.CyberKnowledgeLevel}, Interests: {_userProfile.InterestAreas}");
                            AppendChat("\nThanks! Here's a summary of what I learned about you:");
                            AppendChat(_userProfile.GetUserSummary());
                            PromptTopics();
                        }
                        else
                        {
                            AppendChat(_questionService.GetNextQuestion());
                        }
                        break;

                    case "mainChat":
                        await HandleMainChatAsync(userInput.ToLower());
                        break;
                }
            }
            catch (Exception ex)
            {
                AppendChat("🚨 Oops! Something went wrong. Please try again.");
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private async Task HandleMainChatAsync(string input)
        {
            try
            {
                if (input == "exit")
                {
                    // Log the end of the session
                    _userProfile.LogActivity("Ended session", "System", "User exited the application");

                    AppendChat(_displayService.GetGoodbyeMessage(_userProfile.Name));
                    await Task.Delay(1500);
                    Application.Current.Shutdown();
                    return;
                }

                // Check for due reminders
                string reminderAlert = _taskManager.CheckForDueReminders(_userProfile);
                if (!string.IsNullOrEmpty(reminderAlert))
                {
                    AppendChat(reminderAlert);
                }

                // Check for activity log request
                if (_activityLogRequestPatterns.Any(pattern => input.Contains(pattern)))
                {
                    AppendChat(_userProfile.GetActivityLogSummary());
                    PromptTopics();
                    return;
                }

                // Check for action summary request
                if (IsSummaryRequest(input))
                {
                    AppendChat(GenerateActionSummary());
                    PromptTopics();
                    return;
                }

                if (input.Contains("remember") && input.Contains("name"))
                {
                    AppendChat($"Of course, your name is {_userProfile.Name}!");
                    PromptTopics();
                    return;
                }

                // Handle task-related commands
                string taskResponse = _taskManager.ProcessTaskCommand(_userProfile, input);
                if (taskResponse != null)
                {
                    AppendChat(taskResponse);
                    PromptTopics();
                    return;
                }

                // Handle quiz-related commands
                string quizResponse = _quizManager.ProcessQuizCommand(input);
                if (quizResponse != null)
                {
                    AppendChat(quizResponse);
                    PromptTopics();
                    return;
                }

                bool sentimentDetected = DetectSentiment(input);
                bool askedForMoreDetails = DetectMoreDetailsRequest(input);
                string topic = DetectTopic(input);

                if (askedForMoreDetails)
                {
                    if (!string.IsNullOrEmpty(currentTopic))
                    {
                        AppendChat(_topicService.GetDetailedInfo(currentTopic));
                        HandleFollowUp(currentTopic);

                        // Log the detailed inquiry
                        _userProfile.LogActivity("Requested details", "Topic", $"Detailed information about {currentTopic}");
                    }
                    else
                    {
                        AppendChat("❓ Please ask about a topic first (e.g., phishing, password safety) before requesting more info.");
                    }

                    PromptTopics();
                    return;
                }

                if (!string.IsNullOrEmpty(topic))
                {
                    currentTopic = topic;
                    _userInquiries.Add(topic);
                    UpdateFavoriteTopic(topic);

                    // Log the topic inquiry
                    _userProfile.LogActivity("Asked about topic", "Topic", topic);

                    AppendChat(GetBasicInfoByTopic(topic));
                    ProvideContextualFollowUp(topic);
                    PromptTopics();
                    return;
                }

                if (sentimentDetected)
                {
                    PromptTopics();
                    return;
                }

                // Unrecognized input fallback
                AppendChat("🤔 I'm not sure how to help with that. Try asking about topics like phishing, password safety, or privacy, or manage your tasks with commands like 'add task' or 'view tasks'.");
                PromptTopics();
            }
            catch (Exception ex)
            {
                AppendChat("🚨 An unexpected error occurred during the chat.");
                Console.WriteLine($"Chat error: {ex.Message}");
            }
        }

        private bool IsSummaryRequest(string input)
        {
            return _summaryRequestPatterns.Any(pattern => input.Contains(pattern));
        }

        private string GenerateActionSummary()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("📋 Here's a summary of recent actions:");

            // Add task summary
            int completedTasks = _userProfile.Tasks.Count(t => t.IsCompleted);
            int pendingTasks = _userProfile.Tasks.Count - completedTasks;

            if (_userProfile.Tasks.Count > 0)
            {
                sb.AppendLine($"Tasks: {_userProfile.Tasks.Count} total ({completedTasks} completed, {pendingTasks} pending)");

                // List tasks with reminders
                var tasksWithReminders = _userProfile.Tasks.Where(t => t.ReminderDate.HasValue).ToList();
                if (tasksWithReminders.Any())
                {
                    sb.AppendLine("Tasks with reminders:");
                    int count = 1;
                    foreach (var task in tasksWithReminders)
                    {
                        string status = task.IsCompleted ? "✅ Completed" : "🕒 Pending";
                        sb.AppendLine($"{count++}. {task.Title} - {status}, reminder on {task.ReminderDate?.ToShortDateString()}");
                    }
                }
            }
            else
            {
                sb.AppendLine("No tasks have been created yet.");
            }

            // Add topic inquiries
            if (_userInquiries.Count > 0)
            {
                sb.AppendLine($"\nTopics you've asked about: {_userInquiries.Count}");
                var topTopics = _userInquiries
                    .GroupBy(t => t)
                    .OrderByDescending(g => g.Count())
                    .Take(3)
                    .ToList();

                if (topTopics.Any())
                {
                    sb.AppendLine("Most frequently discussed topics:");
                    foreach (var topic in topTopics)
                    {
                        sb.AppendLine($"- {topic.Key} ({topic.Count()} times)");
                    }
                }
            }

            // Log the activity
            _userProfile.LogActivity("Viewed action summary", "System", "Displayed summary of tasks and topics");

            return sb.ToString();
        }

        private string DetectTopic(string input)
        {
            // Enhanced topic detection with NLP
            // Check for direct topic mentions
            if (input.Contains("phishing")) return "phishing";
            if (input.Contains("password safety") || input.Contains("password")) return "password safety";
            if (input.Contains("suspicious links")) return "suspicious links";
            if (input.Contains("privacy")) return "privacy";
            if (input.Contains("social engineering")) return "social engineering";
            if (input.Contains("identity theft")) return "identity theft";

            // Check for related terms
            if (Regex.IsMatch(input, @"\b(email|spam|scam|fake email)\b"))
                return "phishing";

            if (Regex.IsMatch(input, @"\b(password|secure login|credentials|authentication)\b"))
                return "password safety";

            if (Regex.IsMatch(input, @"\b(url|link|website safety|clicking|suspicious website)\b"))
                return "suspicious links";

            if (Regex.IsMatch(input, @"\b(private|data protection|information security|personal data)\b"))
                return "privacy";

            if (Regex.IsMatch(input, @"\b(manipulation|pretexting|baiting|impersonation|trust)\b"))
                return "social engineering";

            if (Regex.IsMatch(input, @"\b(identity|personal information|stolen identity|fraud)\b"))
                return "identity theft";

            return null;
        }

        private bool DetectMoreDetailsRequest(string input)
        {
            string[] phrases = {
                "more details", "extra info", "tell me more", "explain further", "more information", "deeper info",
                "elaborate", "in depth", "details", "specifics", "more about", "additional information",
                "can you explain", "how does it work", "need more info"
            };
            return phrases.Any(p => input.Contains(p));
        }

        private bool DetectSentiment(string input)
        {
            input = input.ToLower();

            if (input.Contains("worried") || input.Contains("concerned") || input.Contains("anxious"))
            {
                AppendChat("💬 It's okay to feel that way. Here are some simple steps to stay secure:");
                AppendChat("- Don't click unknown links.");
                AppendChat("- Use strong passwords.");
                AppendChat("- Install antivirus software.");

                // Log the sentiment detection
                _userProfile.LogActivity("Expressed concern", "Sentiment", "User expressed worry or concern");
                return true;
            }

            if (input.Contains("curious") || input.Contains("interested") || input.Contains("want to know"))
            {
                AppendChat("💡 Great curiosity! You can explore free cybersecurity courses online.");

                // Log the sentiment detection
                _userProfile.LogActivity("Expressed curiosity", "Sentiment", "User expressed curiosity");
                return true;
            }

            if (input.Contains("frustrated") || input.Contains("upset") || input.Contains("angry"))
            {
                AppendChat("💭 Cybersecurity can be overwhelming sometimes. Stay calm and break it down step by step.");

                // Log the sentiment detection
                _userProfile.LogActivity("Expressed frustration", "Sentiment", "User expressed frustration or anger");
                return true;
            }

            return false;
        }

        private string GetBasicInfoByTopic(string topic)
        {
            return _topicService.BasicInfoHandlers.TryGetValue(topic, out var handler)
                ? handler()
                : _displayService.GetInvalidChoiceMessage();
        }

        private void ProvideContextualFollowUp(string topic)
        {
            switch (topic)
            {
                case "phishing":
                    AppendChat("🛡️ Context tip: Spear phishing uses personal info to trick you. Always double-check email authenticity.");
                    AppendChat("Want to test your knowledge? Type 'start quiz phishing' to take a quiz on this topic!");
                    break;
                case "password safety":
                    AppendChat("🔐 Context tip: Use a password manager and avoid reusing passwords.");
                    AppendChat("Want to test your knowledge? Type 'start quiz password security' to take a quiz on this topic!");
                    break;
                case "suspicious links":
                    AppendChat("🔗 Context tip: Hover over links before clicking. Check for misspellings in URLs.");
                    AppendChat("Want to test your knowledge? Type 'start quiz safe browsing' to take a quiz on this topic!");
                    break;
                case "privacy":
                    AppendChat("🔒 Context tip: Lock down your social media privacy settings and limit app permissions.");
                    AppendChat("Want to test your knowledge? Type 'start quiz data protection' to take a quiz on this topic!");
                    break;
                case "social engineering":
                    AppendChat("🎭 Context tip: Never trust requests for sensitive data over calls or emails without verification.");
                    AppendChat("Want to test your knowledge? Type 'start quiz social engineering' to take a quiz on this topic!");
                    break;
                case "identity theft":
                    AppendChat("🆔 Context tip: Monitor your credit reports and use identity protection services.");
                    AppendChat("Want to test your knowledge? Type 'start quiz data protection' to take a quiz on this topic!");
                    break;
            }
        }

        private void HandleFollowUp(string topic)
        {
            switch (topic)
            {
                case "phishing":
                    AppendChat("- Advanced phishing attacks mimic real login pages.");
                    AppendChat("- Always verify sender addresses.");
                    break;
                case "password safety":
                    AppendChat("- Avoid using pet names or birthdays.");
                    AppendChat("- Update your passwords regularly.");
                    break;
                case "suspicious links":
                    AppendChat("- Use link scanners or antivirus tools to test suspicious links.");
                    break;
                case "privacy":
                    AppendChat("- Don't overshare on public forums.");
                    AppendChat("- Use private browsing when needed.");
                    break;
                case "social engineering":
                    AppendChat("- Social engineers may pretend to be HR or IT support.");
                    break;
                case "identity theft":
                    AppendChat("- Shred documents with personal info before throwing them away.");
                    break;
            }
        }

        private void UpdateFavoriteTopic(string topic)
        {
            if (string.IsNullOrEmpty(_userProfile.FavoriteTopic) ||
                !_userProfile.FavoriteTopic.Equals(topic, StringComparison.OrdinalIgnoreCase))
            {
                _userProfile.FavoriteTopic = topic;
            }
        }

        private void CleanUserProfileTopics()
        {
            if (string.IsNullOrWhiteSpace(_userProfile.InterestAreas)) return;

            string[] validTopics = { "phishing", "password safety", "suspicious links", "privacy", "social engineering", "identity theft" };

            var cleanedTopics = _userProfile.InterestAreas
                .ToLower()
                .Split(new[] { ',', ';', '.', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(t => validTopics.Contains(t))
                .Distinct();

            _userProfile.InterestAreas = string.Join(", ", cleanedTopics);
        }

        private void PromptTopics()
        {
            AppendChat("What would you like to do next?");
            AppendChat("- Learn about: phishing, password safety, suspicious links, privacy, social engineering, identity theft");
            AppendChat("- Task management: add task, view tasks, complete task, delete task, set reminder, task help");
            AppendChat("- Take a quiz: start quiz, quiz categories");
            AppendChat("- View activity: show activity log, what have you done for me");
            AppendChat("Type 'exit' to quit.");
        }

        private void AppendChat(string text)
        {
            ChatTextBox.AppendText(text + "\n\n");
            ChatTextBox.ScrollToEnd();
        }
    }
}
