using System;
using System.Collections.Generic;

namespace ChatbotPart3
{
    public class UserProfile
    {
        // User's name, can be null initially
        public string? Name { get; set; }

        // User's favorite topic, can be null initially
        public string? FavoriteTopic { get; set; }

        // User's self-assessed cybersecurity knowledge level (default to "Unspecified")
        public string CyberKnowledgeLevel { get; set; } = "Unspecified";

        // User's areas of interest in cybersecurity (default to "Not specified")
        public string InterestAreas { get; set; } = "Not specified";

        // User's level of concern (default to "Unspecified")
        public string ConcernLevel { get; set; } = "Unspecified";

        // Private list to keep track of user's inquiries/questions asked (lowercase)
        private readonly List<string> _inquiries = new();

        // Add a new inquiry string to the list (ignores null/empty or whitespace)
        public void AddInquiry(string inquiry)
        {
            if (!string.IsNullOrWhiteSpace(inquiry))
            {
                _inquiries.Add(inquiry.ToLowerInvariant());
            }
        }

        // Returns a formatted summary string of the user profile info
        public string GetUserSummary()
        {
            return
                "🔐 Here's what I know about you so far:\n" +
                $"- Name: {Name ?? "Not specified"}\n" +
                $"- Cybersecurity Knowledge Level: {CyberKnowledgeLevel}\n" +
                $"- Topics of Interest: {InterestAreas}\n" +
                $"- Concern Level: {ConcernLevel}\n" +
                $"- Number of inquiries so far: {_inquiries.Count}";
        }
    }
}
