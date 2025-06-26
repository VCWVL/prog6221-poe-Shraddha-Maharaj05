using System;

namespace ChatbotPart3
{
    public class TopicService
    {
        private readonly Random _random = new Random();

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

        public string GetPhishingInfo()
        {
            return GetRandomTip(phishingTips);
        }

        public string GetPasswordInfo()
        {
            return GetRandomTip(passwordTips);
        }

        public string GetSuspiciousLinksInfo()
        {
            return GetRandomTip(suspiciousLinksTips);
        }

        public string GetPrivacyInfo()
        {
            return GetRandomTip(privacyTips);
        }

        // Methods for CyberBot.cs (alternate naming)
        public string PhishingInfo() => GetPhishingInfo();
        public string PasswordInfo() => GetPasswordInfo();
        public string SuspiciousLinksInfo() => GetSuspiciousLinksInfo();
        public string PrivacyInfo() => GetPrivacyInfo();

        private string GetRandomTip(string[] tips)
        {
            int index = _random.Next(tips.Length);
            return tips[index];
        }
    }
}
