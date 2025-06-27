using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ChatbotPart3
{
    public class CyberBot
    {
        private readonly GreetingService _greetingService = new GreetingService();
        private readonly QuestionService _questionService = new QuestionService();
        private readonly TopicService _topicService = new TopicService();
        private readonly DisplayService _displayService = new DisplayService();
        private readonly TaskService _taskService = new TaskService();
        private readonly TaskManager _taskManager;
        private readonly QuizService _quizService;
        private readonly QuizManager _quizManager;

        private UserProfile _userProfile = new UserProfile();
        private List<string> _userInquiries = new List<string>();

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

        public CyberBot()
        {
            _taskService = new TaskService();
            _taskManager = new TaskManager(_taskService, _displayService);
            _quizService = new QuizService();
            _quizManager = new QuizManager(_quizService, _displayService);
        }

        public void Run()
        {
            Console.WriteLine("Welcome to CyberBot!");
            _greetingService.PlayWelcomeSound();

            Console.WriteLine(_displayService.GetAsciiArt());
            Console.WriteLine("Please enter your name:");
            _userProfile.Name = Console.ReadLine();

            // Log the start of the session
            _userProfile.LogActivity("Started session", "System", $"User: {_userProfile.Name}");

            Console.WriteLine(_displayService.GetWelcomeMessageBox(_userProfile.Name));
            Console.WriteLine("Let's begin a few quick questions to get to know your needs.");

            while (!_questionService.IsQuestionnaireComplete())
            {
                Console.WriteLine(_questionService.GetNextQuestion());
                string answer = Console.ReadLine();
                Console.WriteLine(_questionService.ProcessAnswer(answer, _userProfile));
            }

            // Log the completion of the questionnaire
            _userProfile.LogActivity("Completed questionnaire", "System", $"Knowledge level: {_userProfile.CyberKnowledgeLevel}, Interests: {_userProfile.InterestAreas}");

            Console.WriteLine("\nThanks! Here's a summary of what I learned about you:");
            Console.WriteLine(_userProfile.GetUserSummary());

            Console.WriteLine("\nWhat would you like to learn about?");
            Console.WriteLine("(phishing, password safety, suspicious links, privacy, social engineering, identity theft)");
            Console.WriteLine("You can also manage tasks by saying 'add task', 'view tasks', 'complete task', or 'delete task'");
            Console.WriteLine("Or test your knowledge with a quiz by typing 'start quiz' or 'quiz categories'");
            Console.WriteLine("Type 'show activity log' to see your recent actions");
            Console.WriteLine("Type 'exit' to quit.");

            while (true)
            {
                // Check for due reminders
                string reminderAlert = _taskManager.CheckForDueReminders(_userProfile);
                if (!string.IsNullOrEmpty(reminderAlert))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(reminderAlert);
                    Console.ResetColor();
                }

                string input = Console.ReadLine().ToLower();

                if (input == "exit")
                {
                    // Log the end of the session
                    _userProfile.LogActivity("Ended session", "System", "User exited the application");

                    Console.WriteLine(_displayService.GetGoodbyeMessage(_userProfile.Name));
                    Console.WriteLine("\nPress Enter to exit...");
                    Console.ReadLine();  // Wait for user input before closing
                    break;
                }

                // Check for activity log request
                if (_activityLogRequestPatterns.Any(pattern => input.Contains(pattern)))
                {
                    Console.WriteLine(_userProfile.GetActivityLogSummary());
                    Console.WriteLine("\nWhat else would you like to do?");
                    continue;
                }

                // Check for action summary request
                if (IsSummaryRequest(input))
                {
                    Console.WriteLine(GenerateActionSummary());
                    Console.WriteLine("\nWhat else would you like to do?");
                    continue;
                }

                // Handle task-related commands
                string taskResponse = _taskManager.ProcessTaskCommand(_userProfile, input);
                if (taskResponse != null)
                {
                    Console.WriteLine(taskResponse);
                    Console.WriteLine("\nWhat else would you like to do?");
                    continue;
                }

                // Handle quiz-related commands
                if (_quizManager.IsQuizInProgress() || input.Contains("quiz"))
                {
                    string quizResponse = _quizManager.ProcessQuizCommand(input);
                    if (quizResponse != null)
                    {
                        Console.WriteLine(quizResponse);

                        // If quiz is in progress, wait for the next answer
                        if (_quizManager.IsQuizInProgress())
                        {
                            input = Console.ReadLine().ToLower();
                            continue; // Skip the rest of the loop to process the quiz answer
                        }

                        Console.WriteLine("\nWhat else would you like to do?");
                        continue;
                    }
                }

                DetectSentiment(input);

                string topic = DetectTopic(input);
                bool wantsMoreDetails = DetectMoreDetailsRequest(input);

                if (topic != null)
                {
                    _userInquiries.Add(topic);
                    UpdateFavoriteTopic(topic);

                    // Log the topic inquiry
                    _userProfile.LogActivity("Asked about topic", "Topic", topic);

                    if (wantsMoreDetails)
                    {
                        Console.WriteLine(_topicService.GetDetailedInfo(topic));
                        HandleFollowUpQuestions(topic);
                    }
                    else
                    {
                        // Use delegate from TopicService for basic info
                        if (_topicService.BasicInfoHandlers.TryGetValue(topic, out var handler))
                        {
                            Console.WriteLine(handler());
                        }
                        else
                        {
                            Console.WriteLine(_displayService.GetInvalidChoiceMessage());
                        }
                    }

                    ProvideContextualFollowUp();
                }
                else
                {
                    Console.WriteLine(_displayService.GetInvalidChoiceMessage());
                }

                Console.WriteLine("\nWhat else would you like to learn about?");
                Console.WriteLine("(Type 'show activity log' to see your recent actions)");
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
            foreach (var topicKey in _topicService.BasicInfoHandlers.Keys)
            {
                if (input.Contains(topicKey))
                    return topicKey;
            }

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
            string[] detailPhrases = new[]
            {
                "more details", "extra info", "tell me more", "explain further", "more information", "deeper info",
                "elaborate", "in depth", "details", "specifics", "more about", "additional information",
                "can you explain", "how does it work", "need more info"
            };

            return detailPhrases.Any(phrase => input.Contains(phrase));
        }

        private void UpdateFavoriteTopic(string topic)
        {
            if (string.IsNullOrEmpty(_userProfile.FavoriteTopic) ||
                !_userProfile.FavoriteTopic.Equals(topic, StringComparison.OrdinalIgnoreCase))
            {
                _userProfile.FavoriteTopic = topic;
            }
        }

        private void HandleFollowUpQuestions(string topic)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;

            if (topic.Contains("phishing"))
            {
                Console.WriteLine("Additional phishing tips:");
                Console.WriteLine("- Verify sender email addresses.");
                Console.WriteLine("- Watch out for generic greetings.");
                Console.WriteLine("- Be cautious of unexpected attachments.");
            }
            else if (topic.Contains("password safety"))
            {
                Console.WriteLine("More on password safety:");
                Console.WriteLine("- Enable two-factor authentication.");
                Console.WriteLine("- Don't reuse passwords.");
                Console.WriteLine("- Avoid obvious personal info.");
            }
            else if (topic.Contains("suspicious links"))
            {
                Console.WriteLine("More on suspicious links:");
                Console.WriteLine("- Hover to preview the real link.");
                Console.WriteLine("- Use URL expanders for shortened links.");
                Console.WriteLine("- Avoid clicking on unknown URLs.");
            }
            else if (topic.Contains("privacy"))
            {
                Console.WriteLine("Extra privacy tips:");
                Console.WriteLine("- Use strict social media privacy settings.");
                Console.WriteLine("- Be mindful of oversharing online.");
                Console.WriteLine("- Keep your software updated.");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Sorry, I can only give extended advice on phishing, password safety, suspicious links, or privacy.");
            }

            Console.ResetColor();

            // Log the detailed inquiry
            _userProfile.LogActivity("Requested details", "Topic", $"Detailed information about {topic}");
        }

        private void ProvideContextualFollowUp()
        {
            if (_userInquiries.Count > 0)
            {
                string last = _userInquiries[^1]; // C# 8.0 index from end

                switch (last)
                {
                    case "phishing":
                        AskFollowUp("Here's more about phishing techniques and how to spot them:");
                        Console.WriteLine("Want to test your knowledge? Type 'start quiz phishing' to take a quiz on this topic!");
                        break;
                    case "password safety":
                        AskFollowUp("Here's some useful advice on using password managers and strong password habits:");
                        Console.WriteLine("Want to test your knowledge? Type 'start quiz password security' to take a quiz on this topic!");
                        break;
                    case "suspicious links":
                        AskFollowUp("Let's talk about how to detect suspicious links and scams online:");
                        Console.WriteLine("Want to test your knowledge? Type 'start quiz safe browsing' to take a quiz on this topic!");
                        break;
                    case "privacy":
                        AskFollowUp("Here's more on protecting your privacy on social media and online accounts:");
                        Console.WriteLine("Want to test your knowledge? Type 'start quiz data protection' to take a quiz on this topic!");
                        break;
                    case "social engineering":
                        AskFollowUp("Here's more about social engineering tactics and how to avoid them:");
                        Console.WriteLine("Want to test your knowledge? Type 'start quiz social engineering' to take a quiz on this topic!");
                        break;
                    case "identity theft":
                        AskFollowUp("Here's more about protecting yourself from identity theft:");
                        Console.WriteLine("Want to test your knowledge? Type 'start quiz data protection' to take a quiz on this topic!");
                        break;
                }
            }
        }

        private void AskFollowUp(string prompt)
        {
            Console.WriteLine(prompt);
            Console.ForegroundColor = ConsoleColor.Yellow;

            if (prompt.Contains("phishing"))
            {
                Console.WriteLine("Spear phishing is highly targeted and uses personalized details.");
                Console.WriteLine("Always scrutinize links, even if they seem to come from known sources.");
            }
            else if (prompt.Contains("password managers"))
            {
                Console.WriteLine("Password managers generate and store complex passwords securely.");
                Console.WriteLine("Use unique passwords for each account to limit damage from breaches.");
            }
            else if (prompt.Contains("suspicious links"))
            {
                Console.WriteLine("Common scams include fake invoices, support fraud, and lottery scams.");
                Console.WriteLine("Check links for typos, strange domains, or unexpected redirects.");
            }
            else if (prompt.Contains("privacy")
)
            {
                Console.WriteLine("Limit visibility of your profile information and posts.");
                Console.WriteLine("Enable two-step verification on your accounts.");
            }

            Console.ResetColor();
        }

        private void DetectSentiment(string input)
        {
            if (input.Contains("worried") || input.Contains("concerned") || input.Contains("anxious"))
            {
                Console.WriteLine("It's okay to feel that way. Here are some basics to help ease your concerns:");
                Console.WriteLine("- Avoid sharing sensitive data via email or unknown websites.");
                Console.WriteLine("- Look out for fake login pages.");
                Console.WriteLine("- Install a trusted antivirus tool.");

                // Log the sentiment detection
                _userProfile.LogActivity("Expressed concern", "Sentiment", "User expressed worry or concern");
            }
            else if (input.Contains("curious") || input.Contains("want to know"))
            {
                Console.WriteLine("Great curiosity! Here's how to learn more:");
                Console.WriteLine("- Follow trusted cybersecurity blogs.");
                Console.WriteLine("- Take a beginner course on platforms like Coursera or Udemy.");

                // Log the sentiment detection
                _userProfile.LogActivity("Expressed curiosity", "Sentiment", "User expressed curiosity");
            }
            else if (input.Contains("frustrated") || input.Contains("upset") || input.Contains("angry"))
            {
                Console.WriteLine("Cybersecurity can be frustrating sometimes. Here's how to stay calm and secure:");
                Console.WriteLine("- Break problems into smaller steps.");
                Console.WriteLine("- Seek help from trusted forums like Stack Overflow or Reddit.");
                Console.WriteLine("- Ask a friend or colleague for help.");

                // Log the sentiment detection
                _userProfile.LogActivity("Expressed frustration", "Sentiment", "User expressed frustration or anger");
            }
        }
    }
}
