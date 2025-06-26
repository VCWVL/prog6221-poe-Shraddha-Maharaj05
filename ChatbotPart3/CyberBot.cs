using ChatbotPart3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        private UserProfile _userProfile = new UserProfile();
        private List<string> _userInquiries = new List<string>();

        public CyberBot()
        {
            _taskService = new TaskService();
            _taskManager = new TaskManager(_taskService, _displayService);
        }

        public void Run()
        {
            Console.WriteLine("Welcome to CyberBot!");
            _greetingService.PlayWelcomeSound();

            Console.WriteLine(_displayService.GetAsciiArt());
            Console.WriteLine("Please enter your name:");
            _userProfile.Name = Console.ReadLine();

            Console.WriteLine(_displayService.GetWelcomeMessageBox(_userProfile.Name));
            Console.WriteLine("Let's begin a few quick questions to get to know your needs.");

            while (!_questionService.IsQuestionnaireComplete())
            {
                Console.WriteLine(_questionService.GetNextQuestion());
                string answer = Console.ReadLine();
                Console.WriteLine(_questionService.ProcessAnswer(answer, _userProfile));
            }

            Console.WriteLine("\nThanks! Here's a summary of what I learned about you:");
            Console.WriteLine(_userProfile.GetUserSummary());

            Console.WriteLine("\nWhat would you like to learn about?");
            Console.WriteLine("(phishing, password safety, suspicious links, privacy, social engineering, identity theft)");
            Console.WriteLine("You can also manage tasks by saying 'add task', 'view tasks', 'complete task', or 'delete task'");
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
                    Console.WriteLine(_displayService.GetGoodbyeMessage(_userProfile.Name));
                    Console.WriteLine("\nPress Enter to exit...");
                    Console.ReadLine();  // Wait for user input before closing
                    break;
                }

                // Handle task-related commands
                string taskResponse = _taskManager.ProcessTaskCommand(_userProfile, input);
                if (taskResponse != null)
                {
                    Console.WriteLine(taskResponse);
                    Console.WriteLine("\nWhat else would you like to do?");
                    continue;
                }

                DetectSentiment(input);

                string topic = DetectTopic(input);
                bool wantsMoreDetails = DetectMoreDetailsRequest(input);

                if (topic != null)
                {
                    _userInquiries.Add(topic);
                    UpdateFavoriteTopic(topic);

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
            }
        }

        private string DetectTopic(string input)
        {
            // Return first matching topic keyword found in input
            foreach (var topicKey in _topicService.BasicInfoHandlers.Keys)
            {
                if (input.Contains(topicKey))
                    return topicKey;
            }
            return null;
        }

        private bool DetectMoreDetailsRequest(string input)
        {
            string[] detailPhrases = new[]
            {
                "more details", "extra info", "tell me more", "explain further", "more information", "deeper info"
            };

            foreach (var phrase in detailPhrases)
            {
                if (input.Contains(phrase)) return true;
            }
            return false;
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
                        break;
                    case "password safety":
                        AskFollowUp("Here's some useful advice on using password managers and strong password habits:");
                        break;
                    case "suspicious links":
                        AskFollowUp("Let's talk about how to detect suspicious links and scams online:");
                        break;
                    case "privacy":
                        AskFollowUp("Here's more on protecting your privacy on social media and online accounts:");
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
            else if (prompt.Contains("privacy"))
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
            }
            else if (input.Contains("curious") || input.Contains("want to know"))
            {
                Console.WriteLine("Great curiosity! Here's how to learn more:");
                Console.WriteLine("- Follow trusted cybersecurity blogs.");
                Console.WriteLine("- Take a beginner course on platforms like Coursera or Udemy.");
            }
            else if (input.Contains("frustrated") || input.Contains("upset") || input.Contains("angry"))
            {
                Console.WriteLine("Cybersecurity can be frustrating sometimes. Here's how to stay calm and secure:");
                Console.WriteLine("- Break problems into smaller steps.");
                Console.WriteLine("- Seek help from trusted forums like Stack Overflow or Reddit.");
                Console.WriteLine("- Ask a friend or colleague for help.");
            }
        }
    }
}
