using System;
using System.Collections.Generic;

namespace ChatbotPart3
{
    public class QuestionService
    {
        private readonly Dictionary<string, string> _keywordResponses = new()
        {
            { "password", "Make sure to use strong, unique passwords for each account. Avoid using personal details in your passwords." },
            { "scam", "Be cautious of unsolicited messages or emails that ask for personal information. Always verify the source before clicking on links." },
            { "privacy", "Protect your privacy by adjusting your social media settings and being mindful of the information you share online." }
        };

        public string GetWelcomeMessage(string userName)
        {
            return $"\nHello {userName}! How are you today?";
        }

        public string GetInitialGuidance()
        {
            return "\nBefore we begin, you can ask me about cybersecurity topics like password safety, scams, or privacy.";
        }

        public string GetResponseForQuestion(string question)
        {
            if (string.IsNullOrWhiteSpace(question))
                return "\nOkay! Let's jump straight into it!";

            string questionLower = question.ToLower();
            foreach (var keyword in _keywordResponses.Keys)
            {
                if (questionLower.Contains(keyword))
                {
                    return _keywordResponses[keyword];
                }
            }
            return "Hmm... I can only provide information on password safety, scams, and privacy for now. Sorry :(";
        }

        // Resolves the 'AskPredefinedQuestions' missing method error in CyberBot.cs
        public void AskPredefinedQuestions(string userName)
        {
            Console.WriteLine($"\nNice to meet you, {userName}!");
            Console.WriteLine("Here are a few things I can help you with:");
            Console.WriteLine("- Password Safety");
            Console.WriteLine("- Scams");
            Console.WriteLine("- Privacy Settings\n");
        }
    }
}
