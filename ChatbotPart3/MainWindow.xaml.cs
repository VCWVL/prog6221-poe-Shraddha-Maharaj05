using ChatbotPart3;
using System;
using System.Collections.Generic;
using System.Linq;
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

        private UserProfile _userProfile = new UserProfile();
        private List<string> _userInquiries = new List<string>();
        private string currentState = "askName";
        private string currentTopic = null;

        public MainWindow()
        {
            InitializeComponent();
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
                    AppendChat(_displayService.GetGoodbyeMessage(_userProfile.Name));
                    await Task.Delay(1500);
                    Application.Current.Shutdown();
                    return;
                }

                if (input.Contains("remember") && input.Contains("name"))
                {
                    AppendChat($"Of course, your name is {_userProfile.Name}!");
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
                AppendChat("🤔 I'm not sure how to help with that. Try asking about topics like phishing, password safety, or privacy.");
                PromptTopics();
            }
            catch (Exception ex)
            {
                AppendChat("🚨 An unexpected error occurred during the chat.");
                Console.WriteLine($"Chat error: {ex.Message}");
            }
        }

        private string DetectTopic(string input)
        {
            if (input.Contains("phishing")) return "phishing";
            if (input.Contains("password safety") || input.Contains("password")) return "password safety";
            if (input.Contains("suspicious links")) return "suspicious links";
            if (input.Contains("privacy")) return "privacy";
            if (input.Contains("social engineering")) return "social engineering";
            if (input.Contains("identity theft")) return "identity theft";
            return null;
        }

        private bool DetectMoreDetailsRequest(string input)
        {
            string[] phrases = { "more details", "extra info", "tell me more", "explain further", "more information", "deeper info" };
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
                return true;
            }

            if (input.Contains("curious") || input.Contains("interested") || input.Contains("want to know"))
            {
                AppendChat("💡 Great curiosity! You can explore free cybersecurity courses online.");
                return true;
            }

            if (input.Contains("frustrated") || input.Contains("upset") || input.Contains("angry"))
            {
                AppendChat("💭 Cybersecurity can be overwhelming sometimes. Stay calm and break it down step by step.");
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
                    break;
                case "password safety":
                    AppendChat("🔐 Context tip: Use a password manager and avoid reusing passwords.");
                    break;
                case "suspicious links":
                    AppendChat("🔗 Context tip: Hover over links before clicking. Check for misspellings in URLs.");
                    break;
                case "privacy":
                    AppendChat("🔒 Context tip: Lock down your social media privacy settings and limit app permissions.");
                    break;
                case "social engineering":
                    AppendChat("🎭 Context tip: Never trust requests for sensitive data over calls or emails without verification.");
                    break;
                case "identity theft":
                    AppendChat("🆔 Context tip: Monitor your credit reports and use identity protection services.");
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
                    AppendChat("- Don’t overshare on public forums.");
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
            AppendChat("What else would you like to learn about?");
            AppendChat("(phishing, password safety, suspicious links, privacy, social engineering, identity theft)");
            AppendChat("Type 'exit' to quit.");
        }

        private void AppendChat(string text)
        {
            ChatTextBox.AppendText(text + "\n\n");
            ChatTextBox.ScrollToEnd();
        }
    }
}
