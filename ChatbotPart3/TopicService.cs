using System;
using System.Collections.Generic;

namespace ChatbotPart3
{
    public class TopicService
    {
        private readonly Random _random = new Random();

        // Delegate to provide a string tip/info about a topic
        public delegate string TopicHandler();

        // Dictionary for quick lookup of basic info handlers by topic name
        public Dictionary<string, TopicHandler> BasicInfoHandlers { get; private set; }

        public TopicService()
        {
            BasicInfoHandlers = new Dictionary<string, TopicHandler>(StringComparer.OrdinalIgnoreCase)
            {
                { "phishing", GetPhishingInfo },
                { "password safety", GetPasswordInfo },
                { "suspicious links", GetSuspiciousLinksInfo },
                { "privacy", GetPrivacyInfo },
                { "social engineering", GetSocialEngineeringInfo },
                { "identity theft", GetIdentityTheftInfo }
            };
        }

        private readonly string[] phishingTips = new[]
        {
            "Never click on suspicious links or download unexpected attachments.",
            "Phishing emails often use urgent or threatening language to trick you.",
            "Check the sender's email address carefully — it might be spoofed.",
            "Enable multi-factor authentication to protect your accounts.",
            "Verify websites by manually typing URLs into your browser."
        };

        private readonly string[] passwordTips = new[]
        {
            "Use a mix of letters, numbers, and symbols in your passwords.",
            "Never reuse passwords across multiple sites.",
            "Consider using a reputable password manager.",
            "Change your passwords regularly, especially after breaches.",
            "Avoid using personal information like your birthdate in passwords."
        };

        private readonly string[] suspiciousLinksTips = new[]
        {
            "Hover over links to see where they actually lead.",
            "Avoid clicking on shortened URLs unless you trust the source.",
            "Look for slight misspellings in URLs (like gooogle.com).",
            "Be cautious of links from unknown senders, even if they seem urgent.",
            "Check for HTTPS and a valid security certificate before entering info."
        };

        private readonly string[] privacyTips = new[]
        {
            "Limit how much personal information you share on social media.",
            "Regularly review app permissions on your phone.",
            "Use private/incognito mode when browsing sensitive topics.",
            "Turn off location tracking on apps that don’t need it.",
            "Avoid using the same email for all your online accounts."
        };

        private readonly string[] socialEngineeringTips = new[]
        {
            "Always verify the identity of anyone asking for sensitive information.",
            "Be cautious of unsolicited phone calls or emails asking for details.",
            "Don’t rush to respond — attackers use urgency to trick you.",
            "Never give out passwords or PINs over the phone or email.",
            "Educate yourself about common social engineering tactics like pretexting."
        };

        private readonly string[] identityTheftTips = new[]
        {
            "Regularly monitor your credit reports and bank statements.",
            "Use strong, unique passwords and enable multi-factor authentication.",
            "Be cautious about sharing personal information online.",
            "Shred documents containing personal information before disposal.",
            "Report any suspicious activity immediately to your bank or authorities."
        };

        public string GetPhishingInfo() => GetRandomTip(phishingTips);
        public string GetPasswordInfo() => GetRandomTip(passwordTips);
        public string GetSuspiciousLinksInfo() => GetRandomTip(suspiciousLinksTips);
        public string GetPrivacyInfo() => GetRandomTip(privacyTips);
        public string GetSocialEngineeringInfo() => GetRandomTip(socialEngineeringTips);
        public string GetIdentityTheftInfo() => GetRandomTip(identityTheftTips);

        private string GetRandomTip(string[] tips)
        {
            int index = _random.Next(tips.Length);
            return tips[index];
        }

        public string GetDetailedInfo(string topic)
        {
            return topic.ToLower() switch
            {
                "phishing" =>
                    "Advanced phishing includes spear phishing targeting specific individuals, " +
                    "attackers researching social media to craft believable messages, and urgent language like 'your account will be locked'.",

                "password safety" =>
                    "Strong passwords should be at least 12 characters combining symbols, uppercase, and lowercase letters. " +
                    "Use password managers to avoid reuse and never store passwords in plain text.",

                "suspicious links" =>
                    "Suspicious links may lead to fake websites designed to steal your info. Always verify URLs carefully, " +
                    "watch for typos, and avoid links from untrusted sources.",

                "privacy" =>
                    "Protect your privacy by limiting data shared on social networks, adjusting app permissions, " +
                    "and avoiding public Wi-Fi for sensitive transactions.",

                "social engineering" =>
                    "Social engineering exploits human trust and emotions to gain confidential info. Examples include phishing calls, baiting, and impersonation.",

                "identity theft" =>
                    "Identity theft occurs when criminals use your personal info for fraud. Prevent it by monitoring accounts, securing your data, and reporting anomalies promptly.",

                _ => "Sorry, no detailed info is available for that topic."
            };
        }
    }
}
