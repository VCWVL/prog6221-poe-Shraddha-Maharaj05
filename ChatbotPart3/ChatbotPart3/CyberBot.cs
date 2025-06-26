using System;
using System.Collections.Generic;

namespace ChatbotPart3
{
    class CyberBot
    {
        private readonly GreetingService _greetingService;
        private readonly QuestionService _questionService;
        private readonly TopicService _topicService;
        private readonly DisplayService _displayService;
        private UserProfile _userProfile;
        private List<string> _userInquiries;

        public CyberBot()
        {
            _greetingService = new GreetingService();
            _questionService = new QuestionService();
            _topicService = new TopicService();
            _displayService = new DisplayService();
            _userProfile = new UserProfile();
            _userInquiries = new List<string>();
        }

        public void StartChat()
        {
            _greetingService.PlayWelcomeSound();
            _displayService.DisplayAsciiArt();

            Console.Write("Please enter your name: ");
            _userProfile.Name = Console.ReadLine();

            _displayService.DisplayWelcomeMessage(_userProfile.Name);
            _questionService.AskPredefinedQuestions(_userProfile.Name);

            bool keepGoing = true;

            while (keepGoing)
            {
                Console.WriteLine("\nWhat would you like to learn about? (e.g., phishing, password safety, suspicious links, privacy)");
                Console.WriteLine("Type 'exit' at any time to quit the program.");
                string userInput = Console.ReadLine().Trim().ToLower();
                Console.Clear();

                if (userInput == "exit")
                {
                    _displayService.DisplayGoodbyeMessage(_userProfile.Name);
                    break;
                }

                bool validTopic = false;

                if (userInput.Contains("phishing"))
                {
                    _userInquiries.Add("phishing");
                    UpdateFavoriteTopic("phishing");
                    Console.WriteLine(_topicService.GetPhishingInfo());
                    validTopic = true;
                }
                else if (userInput.Contains("password"))
                {
                    _userInquiries.Add("password safety");
                    UpdateFavoriteTopic("password safety");
                    Console.WriteLine(_topicService.GetPasswordInfo());
                    validTopic = true;
                }
                else if (userInput.Contains("suspicious links"))
                {
                    _userInquiries.Add("suspicious links");
                    UpdateFavoriteTopic("suspicious links");
                    Console.WriteLine(_topicService.GetSuspiciousLinksInfo());
                    validTopic = true;
                }
                else if (userInput.Contains("privacy"))
                {
                    _userInquiries.Add("privacy");
                    UpdateFavoriteTopic("privacy");
                    Console.WriteLine(_topicService.GetPrivacyInfo());
                    validTopic = true;
                }

                if (!validTopic)
                {
                    _displayService.DisplayInvalidChoiceMessage();
                    continue;
                }

                ProvideContextualFollowUp();

                while (true)
                {
                    Console.WriteLine("\nDo you have any follow-up questions or need more details? (yes/no)");
                    string followUp = Console.ReadLine().Trim().ToLower();
                    if (followUp == "yes")
                    {
                        Console.Write("\nPlease ask your question: ");
                        string followUpQuestion = Console.ReadLine().Trim().ToLower();
                        DetectSentiment(followUpQuestion);

                        if (!string.IsNullOrEmpty(_userProfile.FavoriteTopic) &&
                            followUpQuestion.Contains(_userProfile.FavoriteTopic))
                        {
                            Console.WriteLine($"As someone interested in {_userProfile.FavoriteTopic}, here are some additional tips:");
                            ProvidePersonalizedTips();
                        }
                        else
                        {
                            HandleFollowUpQuestions(followUpQuestion);
                        }
                        break;
                    }
                    else if (followUp == "no")
                    {
                        Console.WriteLine("Okay! If you have any other questions, feel free to ask.");
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Please input 'yes' or 'no'.");
                    }
                }

                while (true)
                {
                    Console.WriteLine("\nIs there anything else you'd like to know about cybersecurity? (yes/no)");
                    string additionalInfo = Console.ReadLine().Trim().ToLower();
                    if (additionalInfo == "yes")
                    {
                        break;
                    }
                    else if (additionalInfo == "no")
                    {
                        Console.WriteLine("Alright! If you have any other questions in the future, feel free to reach out.");
                        _displayService.DisplayGoodbyeMessage(_userProfile.Name);
                        keepGoing = false;
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Please input 'yes' or 'no'.");
                    }
                }
            }
        }

        private void UpdateFavoriteTopic(string topic)
        {
            if (_userProfile.FavoriteTopic != topic)
            {
                _userProfile.FavoriteTopic = topic;
                Console.WriteLine($"Got it! I'll remember that you're interested in {topic}. It's a crucial part of staying safe online.");
            }
        }

        private void ProvidePersonalizedTips()
        {
            if (_userProfile.FavoriteTopic == "phishing")
            {
                Console.WriteLine("- Always verify the sender's email address.");
                Console.WriteLine("- Look for generic greetings like 'Dear Customer'.");
                Console.WriteLine("- Be cautious of attachments in unsolicited emails.");
            }
            else if (_userProfile.FavoriteTopic == "password safety")
            {
                Console.WriteLine("- Use two-factor authentication whenever possible.");
                Console.WriteLine("- Avoid using easily guessable information like birthdays.");
                Console.WriteLine("- Regularly update your passwords and avoid reusing them.");
            }
            else if (_userProfile.FavoriteTopic == "suspicious links")
            {
                Console.WriteLine("- Hover over links to see the actual URL.");
                Console.WriteLine("- Use URL expanders for shortened links.");
                Console.WriteLine("- Search for the website directly instead of clicking unknown links.");
            }
            else if (_userProfile.FavoriteTopic == "privacy")
            {
                Console.WriteLine("- Regularly review your social media privacy settings.");
                Console.WriteLine("- Limit the personal info you share online.");
                Console.WriteLine("- Use strong privacy settings on all accounts.");
            }
        }

        private void HandleFollowUpQuestions(string question)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;

            if (question.Contains("phishing"))
            {
                Console.WriteLine("Here are some additional tips on phishing:");
                ProvidePersonalizedTips();
            }
            else if (question.Contains("password"))
            {
                Console.WriteLine("Here are some additional tips on password safety:");
                ProvidePersonalizedTips();
            }
            else if (question.Contains("suspicious"))
            {
                Console.WriteLine("Here are some additional tips on suspicious links:");
                ProvidePersonalizedTips();
            }
            else if (question.Contains("privacy"))
            {
                Console.WriteLine("Here are some additional tips on privacy:");
                ProvidePersonalizedTips();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("I'm sorry, I can only provide info on phishing, password safety, suspicious links, or privacy.");
            }

            Console.ResetColor();
        }

        private void ProvideContextualFollowUp()
        {
            if (_userInquiries.Count == 0) return;

            string last = _userInquiries[^1];

            switch (last)
            {
                case "phishing":
                    AskFollowUp("Would you like to know about specific phishing techniques or how to recognize them?");
                    break;
                case "password safety":
                    AskFollowUp("Are you interested in learning about password managers or creating strong passwords?");
                    break;
                case "suspicious links":
                    AskFollowUp("Would you like tips on how to identify suspicious links or examples of common scams?");
                    break;
                case "privacy":
                    AskFollowUp("Are you interested in learning about privacy settings on social media or general privacy tips?");
                    break;
            }
        }

        private void AskFollowUp(string question)
        {
            Console.WriteLine(question);
            string response = Console.ReadLine().Trim().ToLower();
            while (response != "yes" && response != "no")
            {
                Console.WriteLine("Please answer with 'yes' or 'no'.");
                response = Console.ReadLine().Trim().ToLower();
            }

            if (response == "yes")
            {
                switch (question)
                {
                    case "Would you like to know about specific phishing techniques or how to recognize them?":
                        Console.WriteLine("Spear phishing targets specific people using personal details.");
                        Console.WriteLine("Training helps users spot and handle phishing emails.");
                        break;
                    case "Are you interested in learning about password managers or creating strong passwords?":
                        Console.WriteLine("Use 16+ characters with a mix of letters, numbers, and symbols.");
                        Console.WriteLine("A password manager can help you store and generate strong passwords.");
                        break;
                    case "Would you like tips on how to identify suspicious links or examples of common scams?":
                        Console.WriteLine("Look closely at the domain. Watch for odd endings or names.");
                        Console.WriteLine("Scams include imposter, advance-fee, romance, and investment fraud.");
                        break;
                    case "Are you interested in learning about privacy settings on social media or general privacy tips?":
                        Console.WriteLine("Avoid sharing personal info like your address or birthdate.");
                        Console.WriteLine("Always install updates — they include important security patches.");
                        break;
                }
            }
            else
            {
                Console.WriteLine("That's all for now? Feel free to reach out if you need more.");
            }
        }

        private void DetectSentiment(string userInput)
        {
            if (userInput.Contains("worried") || userInput.Contains("concerned") || userInput.Contains("anxious"))
            {
                Console.WriteLine("It's okay to feel that way. Scammers are tricky. Here’s how to stay safe:");
                Console.WriteLine("- Never give out personal info via email.");
                Console.WriteLine("- Check URLs carefully before entering data.");
                Console.WriteLine("- Don’t install software from untrusted sources.");
            }
            else if (userInput.Contains("curious") || userInput.Contains("want to know"))
            {
                Console.WriteLine("Great! Staying informed is smart.");
                Console.WriteLine("- Follow cybersecurity news and trends.");
                Console.WriteLine("- Explore tutorials and take online courses.");
            }
            else if (userInput.Contains("frustrated") || userInput.Contains("upset") || userInput.Contains("angry"))
            {
                Console.WriteLine("Cybersecurity can feel overwhelming. You're not alone!");
                Console.WriteLine("- Take breaks and revisit topics calmly.");
                Console.WriteLine("- Let me know how I can better support your learning.");
            }
        }
    }
}
