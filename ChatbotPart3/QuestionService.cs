namespace ChatbotPart3
{
    public class QuestionService
    {
        private int currentStep = 0;

        public int CurrentStep => currentStep;

        public string GetNextQuestion()
        {
            switch (currentStep)
            {
                case 0:
                    return "1. How would you rate your knowledge of cybersecurity?\n   a) Beginner\n   b) Intermediate\n   c) Advanced";
                case 1:
                    return "2. Which cybersecurity topics are you most interested in?\n(Options: phishing, password safety, suspicious links, privacy, social engineering, identity theft, or type 'none' if you have no interest)";
                case 2:
                    return "3. Are you currently worried about any cybersecurity threats?\n   a) Yes, I’ve been targeted or hacked before\n   b) Somewhat concerned\n   c) Not really";
                default:
                    return string.Empty;
            }
        }

        public string ProcessAnswer(string answer, UserProfile userProfile)
        {
            answer = answer.Trim().ToLower();
            string response = "";

            switch (currentStep)
            {
                case 0: // Cyber Knowledge Level
                    if (answer == "a" || answer.Contains("beginner"))
                    {
                        userProfile.CyberKnowledgeLevel = "Beginner";
                        response = "I’ll keep things simple and beginner-friendly.";
                    }
                    else if (answer == "b" || answer.Contains("intermediate"))
                    {
                        userProfile.CyberKnowledgeLevel = "Intermediate";
                        response = "Great! I’ll include some practical and slightly technical insights.";
                    }
                    else if (answer == "c" || answer.Contains("advanced"))
                    {
                        userProfile.CyberKnowledgeLevel = "Advanced";
                        response = "Awesome! I’ll throw in a few advanced tips where possible.";
                    }
                    else
                    {
                        response = "⚠️ Invalid input. Please choose a valid option (a, b, or c).";
                        return response; // Reprompt the same question
                    }
                    break;

                case 1: // Interest Areas
                    if (answer == "none")
                    {
                        userProfile.InterestAreas = "None";
                        userProfile.FavoriteTopic = "None";
                        response = "Thanks! I’ll focus on general topics since you have no specific interests.";
                    }
                    else if (!string.IsNullOrWhiteSpace(answer))
                    {
                        userProfile.InterestAreas = answer;
                        userProfile.FavoriteTopic = answer.Split(',')[0].Trim();
                        response = "Thanks! I’ll focus on those topics as we chat.";
                    }
                    else
                    {
                        response = "⚠️ No input detected. Please specify your interests or type 'none' if you have no interest.";
                        return response; // Reprompt the same question
                    }
                    break;

                case 2: // Concern Level
                    if (answer == "a" || answer.Contains("yes"))
                    {
                        userProfile.ConcernLevel = "High - previously targeted";
                        response = "That’s scary! I’ll prioritize tips to help you protect yourself.";
                    }
                    else if (answer == "b" || answer.Contains("somewhat"))
                    {
                        userProfile.ConcernLevel = "Medium - somewhat concerned";
                        response = "Good to be cautious — we’ll explore how to reduce risk.";
                    }
                    else if (answer == "c" || answer.Contains("not"))
                    {
                        userProfile.ConcernLevel = "Low - not really concerned";
                        response = "Cyber threats are always evolving. Staying informed is key!";
                    }
                    else
                    {
                        response = "⚠️ Invalid input. Please choose a valid option (a, b, or c).";
                        return response; // Reprompt the same question
                    }
                    break;
            }

            currentStep++;
            return response;
        }

        public bool IsQuestionnaireComplete()
        {
            return currentStep >= 3;
        }

        public void Reset()
        {
            currentStep = 0;
        }
    }
}
