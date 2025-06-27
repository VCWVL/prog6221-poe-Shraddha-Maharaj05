using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatbotPart3
{
    public class QuizService
    {
        private readonly List<QuizQuestion> _quizQuestions;
        private readonly Random _random = new Random();

        public QuizService()
        {
            _quizQuestions = InitializeQuizQuestions();
        }

        public List<QuizQuestion> GetRandomQuestions(int count = 5)
        {
            // Ensure we don't try to get more questions than available
            count = Math.Min(count, _quizQuestions.Count);

            // Get random questions
            return _quizQuestions
                .OrderBy(q => _random.Next())
                .Take(count)
                .ToList();
        }

        public List<QuizQuestion> GetQuestionsByCategory(string category, int count = 5)
        {
            var categoryQuestions = _quizQuestions
                .Where(q => q.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                .ToList();

            // Ensure we don't try to get more questions than available
            count = Math.Min(count, categoryQuestions.Count);

            // Get random questions from the category
            return categoryQuestions
                .OrderBy(q => _random.Next())
                .Take(count)
                .ToList();
        }

        public List<string> GetAvailableCategories()
        {
            return _quizQuestions
                .Select(q => q.Category)
                .Distinct()
                .ToList();
        }

        private List<QuizQuestion> InitializeQuizQuestions()
        {
            var questions = new List<QuizQuestion>
            {
                // Phishing Questions
                new QuizQuestion(
                    "What should you do if you receive an email asking for your password?",
                    new[] { "Reply with your password", "Delete the email", "Report the email as phishing", "Ignore it" },
                    2,
                    "Reporting phishing emails helps protect you and others from scams. Never share your password via email.",
                    "Phishing"
                ),

                new QuizQuestion(
                    "Which of these is a sign of a phishing email?",
                    new[] { "The email is from someone you know", "The email has no spelling errors", "The email creates urgency to act quickly", "The email has the company's correct logo" },
                    2,
                    "Creating a false sense of urgency is a common tactic in phishing emails to make you act without thinking.",
                    "Phishing"
                ),

                new QuizQuestion(
                    "What is 'spear phishing'?",
                    new[] { "A fishing technique using spears", "Targeted phishing attacks customized for specific individuals", "Mass phishing emails sent to many people", "Phishing attempts via text messages" },
                    1,
                    "Spear phishing targets specific individuals with personalized information to make the scam more convincing.",
                    "Phishing"
                ),
                
                // Password Security
                new QuizQuestion(
                    "Which of the following is the strongest password?",
                    new[] { "password123", "P@ssw0rd!", "Tr0ub4dor&3", "correct-horse-battery-staple" },
                    3,
                    "A long passphrase with unrelated words is typically stronger than shorter passwords with special characters.",
                    "Password Security"
                ),

                new QuizQuestion(
                    "What is two-factor authentication (2FA)?",
                    new[] { "Using two different passwords", "Requiring two security questions", "Using something you know and something you have", "Changing your password twice a year" },
                    2,
                    "2FA combines something you know (password) with something you have (like a phone) or something you are (biometrics).",
                    "Password Security"
                ),

                new QuizQuestion(
                    "How often should you change your passwords?",
                    new[] { "Every day", "Only when there's a security breach", "Every 30 days without fail", "Never, if they're strong enough" },
                    1,
                    "Modern security advice suggests changing passwords when there's a breach rather than on a schedule, which can lead to weaker passwords.",
                    "Password Security"
                ),
                
                // Safe Browsing
                new QuizQuestion(
                    "What does HTTPS in a URL indicate?",
                    new[] { "The site is a government website", "The connection to the site is encrypted", "The site is high-speed", "The site is hosted in a secure location" },
                    1,
                    "HTTPS (Hypertext Transfer Protocol Secure) indicates that the connection between your browser and the website is encrypted.",
                    "Safe Browsing"
                ),

                new QuizQuestion(
                    "What should you check before entering sensitive information on a website?",
                    new[] { "The website's color scheme", "If the site has HTTPS and a valid certificate", "If the site has many advertisements", "How many pages the website has" },
                    1,
                    "Always verify that a site uses HTTPS (look for the padlock icon) before entering sensitive information.",
                    "Safe Browsing"
                ),
                
                // Social Engineering
                new QuizQuestion(
                    "What is social engineering in cybersecurity?",
                    new[] { "Building social networks securely", "Manipulating people to divulge confidential information", "Engineering social media platforms", "Creating fake social media profiles" },
                    1,
                    "Social engineering uses psychological manipulation to trick people into revealing sensitive information or performing actions.",
                    "Social Engineering"
                ),

                new QuizQuestion(
                    "Which of these is NOT a common social engineering tactic?",
                    new[] { "Pretexting", "Baiting", "Quantum computing", "Tailgating" },
                    2,
                    "Quantum computing is a technology, not a social engineering tactic. The others are methods used to manipulate people.",
                    "Social Engineering"
                ),
                
                // Mobile Security
                new QuizQuestion(
                    "What is the best practice for downloading mobile apps?",
                    new[] { "Download from any website offering the app", "Only download from official app stores", "Always download the newest apps", "Only download free apps" },
                    1,
                    "Official app stores like Google Play and Apple App Store review apps for security issues before publishing them.",
                    "Mobile Security"
                ),

                new QuizQuestion(
                    "What permission should you be most cautious about granting to mobile apps?",
                    new[] { "Camera access", "Location access", "Contact list access", "All of the above" },
                    3,
                    "All these permissions can be misused by malicious apps, so only grant them when necessary and to trusted applications.",
                    "Mobile Security"
                ),
                
                // Data Protection
                new QuizQuestion(
                    "What is the purpose of encrypting your data?",
                    new[] { "To make it load faster", "To make it unreadable to unauthorized users", "To compress it to save space", "To make it compatible with all devices" },
                    1,
                    "Encryption converts data into a code to prevent unauthorized access, making it unreadable without the decryption key.",
                    "Data Protection"
                ),

                new QuizQuestion(
                    "Which of these is a good practice for data backups?",
                    new[] { "Keep one copy of important files", "Store backups only on your computer", "Follow the 3-2-1 rule (3 copies, 2 types of media, 1 offsite)", "Back up only when you remember to" },
                    2,
                    "The 3-2-1 backup rule helps ensure your data survives various disaster scenarios.",
                    "Data Protection"
                ),
                
                // True/False Questions
                new QuizQuestion(
                    "True or False: Public Wi-Fi networks are generally secure for online banking.",
                    new[] { "True", "False" },
                    1,
                    "Public Wi-Fi networks are often unsecured and can be monitored by attackers. Use a VPN or mobile data for sensitive activities.",
                    "Safe Browsing"
                ),

                new QuizQuestion(
                    "True or False: If you receive a suspicious call claiming to be from tech support, it's best to provide your information if they know basic details about you.",
                    new[] { "True", "False" },
                    1,
                    "Scammers often know basic information about you from public sources. Never provide sensitive information to unsolicited callers.",
                    "Social Engineering"
                ),

                new QuizQuestion(
                    "True or False: Using the same password across multiple sites is safe as long as it's a strong password.",
                    new[] { "True", "False" },
                    1,
                    "Even strong passwords should not be reused. If one site is breached, all your accounts would be vulnerable.",
                    "Password Security"
                ),

                new QuizQuestion(
                    "True or False: Software updates are primarily for adding new features, not security.",
                    new[] { "True", "False" },
                    1,
                    "While updates may add features, they often contain critical security patches to protect against newly discovered vulnerabilities.",
                    "Data Protection"
                ),

                new QuizQuestion(
                    "True or False: Antivirus software provides 100% protection against all cyber threats.",
                    new[] { "True", "False" },
                    1,
                    "No security solution is perfect. Antivirus is important but should be part of a layered security approach.",
                    "Data Protection"
                )
            };

            return questions;
        }
    }
}
