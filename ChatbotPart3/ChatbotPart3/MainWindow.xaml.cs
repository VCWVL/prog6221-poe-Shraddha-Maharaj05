using ChatbotPart3;
using System;
using System.Collections.Generic;
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

        public MainWindow()
        {
            InitializeComponent();
            StartChat();
        }

        private void StartChat()
        {
            AppendChat("Welcome to CyberBot!");
            _greetingService.PlayWelcomeSound();

            // Display ASCII art from DisplayService
            AppendChat(_displayService.GetAsciiArt());

            AppendChat("Please enter your name:");
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string userInput = UserInputBox.Text.Trim();
            if (string.IsNullOrEmpty(userInput)) return;

            AppendChat($"> {userInput}");
            UserInputBox.Clear();

            switch (currentState)
            {
                case "askName":
                    _userProfile.Name = userInput;
                    AppendChat(_displayService.GetWelcomeMessageBox(_userProfile.Name));
                    _questionService.AskPredefinedQuestions(_userProfile.Name);
                    currentState = "mainChat";
                    AppendChat("What would you like to learn about? (phishing, password safety, suspicious links, privacy)");
                    AppendChat("Type 'exit' to quit.");
                    break;

                case "mainChat":
                    HandleMainChat(userInput.ToLower());
                    break;

                case "followUp":
                    HandleFollowUp(userInput.ToLower());
                    break;

                case "moreInfo":
                    HandleMoreInfo(userInput.ToLower());
                    break;

                case "handleQuestion":
                    AppendChat("Thanks for your question. I’ll try to address it in future updates!");
                    currentState = "moreInfo";
                    AppendChat("Would you like to learn about another topic? (yes/no)");
                    break;
            }
        }

        private void HandleMainChat(string input)
        {
            if (input == "exit")
            {
                AppendChat(_displayService.GetGoodbyeMessage(_userProfile.Name));
                Application.Current.Shutdown();
                return;
            }

            bool matched = false;

            if (input.Contains("phishing"))
            {
                matched = true;
                _userInquiries.Add("phishing");
                UpdateFavoriteTopic("phishing");
                AppendChat(_topicService.GetPhishingInfo());
            }
            else if (input.Contains("password safety"))
            {
                matched = true;
                _userInquiries.Add("password safety");
                UpdateFavoriteTopic("password safety");
                AppendChat(_topicService.GetPasswordInfo());
            }
            else if (input.Contains("suspicious links"))
            {
                matched = true;
                _userInquiries.Add("suspicious links");
                UpdateFavoriteTopic("suspicious links");
                AppendChat(_topicService.GetSuspiciousLinksInfo());
            }
            else if (input.Contains("privacy"))
            {
                matched = true;
                _userInquiries.Add("privacy");
                UpdateFavoriteTopic("privacy");
                AppendChat(_topicService.GetPrivacyInfo());
            }

            if (!matched)
            {
                AppendChat(_displayService.GetInvalidChoiceMessage());
                return;
            }

            ProvideContextualFollowUp();
            currentState = "followUp";
        }

        private void HandleFollowUp(string input)
        {
            if (input == "yes")
            {
                AppendChat("Please enter your follow-up question:");
                currentState = "handleQuestion";
            }
            else if (input == "no")
            {
                AppendChat("Anything else you'd like to know about cybersecurity? (yes/no)");
                currentState = "moreInfo";
            }
            else
            {
                AppendChat("Please answer with 'yes' or 'no'.");
            }
        }

        private void HandleMoreInfo(string input)
        {
            if (input == "yes")
            {
                AppendChat("What would you like to learn about?");
                currentState = "mainChat";
            }
            else if (input == "no")
            {
                AppendChat(_displayService.GetGoodbyeMessage(_userProfile.Name));
                Application.Current.Shutdown();
            }
            else
            {
                AppendChat("Please enter 'yes' or 'no'.");
            }
        }

        private void UpdateFavoriteTopic(string topic)
        {
            if (_userProfile.FavoriteTopic != topic)
            {
                _userProfile.FavoriteTopic = topic;
                AppendChat($"Got it! I'll remember that you're interested in {topic}.");
            }
        }

        private void ProvideContextualFollowUp()
        {
            if (_userInquiries.Count == 0) return;

            string last = _userInquiries[^1];
            string question = last switch
            {
                "phishing" => "Would you like to know about specific phishing techniques?",
                "password safety" => "Do you want to learn about password managers?",
                "suspicious links" => "Would you like tips on identifying suspicious links?",
                "privacy" => "Do you want to explore privacy settings on social media?",
                _ => null
            };

            if (!string.IsNullOrEmpty(question))
            {
                AppendChat(question + " (yes/no)");
            }
        }

        private void AppendChat(string message)
        {
            ChatOutput.Text += $"{message}\n\n";
            ChatOutput.ScrollToEnd();
        }
    }
}
