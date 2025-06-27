using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ChatbotPart3
{
    public class QuizManager
    {
        private readonly QuizService _quizService;
        private readonly DisplayService _displayService;
        private List<QuizQuestion> _currentQuizQuestions;
        private int _currentQuestionIndex;
        private int _correctAnswers;
        private bool _quizInProgress;

        // NLP patterns for quiz commands
        private readonly Dictionary<string, List<string>> _quizCommandPatterns = new Dictionary<string, List<string>>
        {
            {
                "start_quiz", new List<string>
                {
                    "start quiz", "take quiz", "begin quiz", "play quiz", "start a quiz", "take a quiz",
                    "test my knowledge", "quiz me", "let's do a quiz", "i want to take a quiz",
                    "can i take a quiz", "give me a quiz", "challenge me", "test me"
                }
            },
            {
                "quiz_categories", new List<string>
                {
                    "quiz categories", "quiz topics", "what quizzes", "quiz options", "available quizzes",
                    "what categories", "show categories", "show topics", "what topics", "quiz types",
                    "what kind of quizzes", "what kind of quiz", "quiz list", "list quizzes"
                }
            }
        };

        // Feedback messages for different score ranges
        private readonly Dictionary<int, string> _scoreFeedback = new Dictionary<int, string>
        {
            { 100, "🏆 Perfect score! You're a cybersecurity expert! Your knowledge will help keep you and others safe online." },
            { 80, "🥇 Excellent work! You have strong cybersecurity awareness. Keep up the good habits!" },
            { 60, "🥈 Good job! You have a solid understanding of cybersecurity basics. A little more learning will make you even safer." },
            { 40, "🥉 Not bad! You're on your way to better cybersecurity practices. Keep learning to stay safer online." },
            { 20, "📚 You've got some cybersecurity knowledge, but there's room for improvement. Review the topics you missed." },
            { 0, "🔒 Time to boost your cybersecurity knowledge! Don't worry - everyone starts somewhere. Review the basics to stay safer online." }
        };

        // Varied positive feedback messages
        private readonly string[] _correctFeedback = new string[]
        {
            "✅ Correct! Well done!",
            "✅ That's right! Excellent choice.",
            "✅ Spot on! Great job.",
            "✅ Perfect! You know your stuff.",
            "✅ Absolutely correct! Impressive knowledge."
        };

        // Varied negative feedback messages
        private readonly string[] _incorrectFeedback = new string[]
        {
            "❌ Not quite right. The correct answer is",
            "❌ That's incorrect. The right answer is",
            "❌ Sorry, that's not correct. The answer is",
            "❌ Good try, but the correct answer is",
            "❌ Not this time. The correct answer is"
        };

        private readonly Random _random = new Random();

        public QuizManager(QuizService quizService, DisplayService displayService)
        {
            _quizService = quizService;
            _displayService = displayService;
            _quizInProgress = false;
        }

        public string ProcessQuizCommand(string input)
        {
            // If a quiz is in progress, process the answer
            if (_quizInProgress)
            {
                return ProcessAnswer(input);
            }

            input = input.ToLower().Trim();

            // Detect command type using NLP patterns
            string commandType = DetectQuizCommandType(input);

            if (commandType == null && !input.Contains("quiz"))
                return null; // Not a quiz-related command

            switch (commandType)
            {
                case "start_quiz":
                    // Check if a specific category was requested
                    foreach (var category in _quizService.GetAvailableCategories())
                    {
                        if (input.Contains(category.ToLower()))
                        {
                            return StartCategoryQuiz(category);
                        }
                    }

                    // Start a general quiz if no specific category
                    return StartQuiz();

                case "quiz_categories":
                    return GetAvailableCategories();

                default:
                    // If it's a quiz-related command but not handled above
                    if (input.Contains("quiz"))
                    {
                        return "🎮 To start a cybersecurity quiz, type 'start quiz' or 'quiz categories' to see specific topics.";
                    }
                    return null;
            }
        }

        private string DetectQuizCommandType(string input)
        {
            foreach (var pattern in _quizCommandPatterns)
            {
                if (pattern.Value.Any(phrase => input.Contains(phrase)))
                {
                    return pattern.Key;
                }
            }
            return null;
        }

        public string StartQuiz(int questionCount = 5)
        {
            _currentQuizQuestions = _quizService.GetRandomQuestions(questionCount);
            _currentQuestionIndex = 0;
            _correctAnswers = 0;
            _quizInProgress = true;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("🎮 Welcome to the Cybersecurity Quiz! 🎮");
            sb.AppendLine($"I'll ask you {_currentQuizQuestions.Count} questions about cybersecurity.");
            sb.AppendLine("Answer by typing the letter (A, B, C, D) or number (1, 2, 3, 4) of your choice.");
            sb.AppendLine();
            sb.AppendLine(GetCurrentQuestion());

            return sb.ToString();
        }

        public string StartCategoryQuiz(string category, int questionCount = 5)
        {
            _currentQuizQuestions = _quizService.GetQuestionsByCategory(category, questionCount);
            _currentQuestionIndex = 0;
            _correctAnswers = 0;
            _quizInProgress = true;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"🎮 Welcome to the {category} Quiz! 🎮");
            sb.AppendLine($"I'll ask you {_currentQuizQuestions.Count} questions about {category.ToLower()}.");
            sb.AppendLine("Answer by typing the letter (A, B, C, D) or number (1, 2, 3, 4) of your choice.");
            sb.AppendLine();
            sb.AppendLine(GetCurrentQuestion());

            return sb.ToString();
        }

        public string ProcessAnswer(string answer)
        {
            if (!_quizInProgress)
            {
                return "There's no active quiz. Type 'start quiz' to begin!";
            }

            QuizQuestion currentQuestion = _currentQuizQuestions[_currentQuestionIndex];
            bool isCorrect = currentQuestion.IsCorrectAnswer(answer);

            StringBuilder sb = new StringBuilder();

            if (isCorrect)
            {
                sb.AppendLine(GetRandomFeedback(true));
                _correctAnswers++;
            }
            else
            {
                sb.AppendLine($"{GetRandomFeedback(false)} {currentQuestion.GetCorrectAnswerLetter()}) {currentQuestion.GetCorrectAnswerText()}");
            }

            sb.AppendLine(currentQuestion.Explanation);

            _currentQuestionIndex++;

            if (_currentQuestionIndex < _currentQuizQuestions.Count)
            {
                sb.AppendLine();
                sb.AppendLine(GetCurrentQuestion());
            }
            else
            {
                sb.AppendLine();
                sb.AppendLine(GetQuizResults());
                _quizInProgress = false;
            }

            return sb.ToString();
        }

        public bool IsQuizInProgress()
        {
            return _quizInProgress;
        }

        public string GetAvailableCategories()
        {
            var categories = _quizService.GetAvailableCategories();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("📚 Available Quiz Categories:");

            foreach (var category in categories)
            {
                sb.AppendLine($"- {category}");
            }

            sb.AppendLine();
            sb.AppendLine("Type 'start quiz [category]' to begin a category quiz, or just 'start quiz' for a mixed quiz.");

            return sb.ToString();
        }

        private string GetCurrentQuestion()
        {
            if (_currentQuestionIndex >= _currentQuizQuestions.Count)
            {
                return "No more questions!";
            }

            QuizQuestion question = _currentQuizQuestions[_currentQuestionIndex];

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Question {_currentQuestionIndex + 1} of {_currentQuizQuestions.Count}:");
            sb.AppendLine(question.GetFormattedQuestion());

            return sb.ToString();
        }

        private string GetQuizResults()
        {
            int percentage = (_correctAnswers * 100) / _currentQuizQuestions.Count;
            string feedback = GetScoreFeedback(percentage);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("🏁 Quiz Complete! 🏁");
            sb.AppendLine($"You scored {_correctAnswers} out of {_currentQuizQuestions.Count} ({percentage}%)");
            sb.AppendLine();
            sb.AppendLine(feedback);
            sb.AppendLine();
            sb.AppendLine("Type 'start quiz' to try again with different questions!");

            return sb.ToString();
        }

        private string GetScoreFeedback(int percentage)
        {
            // Find the highest threshold that the score meets
            int threshold = _scoreFeedback.Keys
                .Where(key => percentage >= key)
                .OrderByDescending(key => key)
                .FirstOrDefault();

            return _scoreFeedback[threshold];
        }

        private string GetRandomFeedback(bool isCorrect)
        {
            string[] feedbackArray = isCorrect ? _correctFeedback : _incorrectFeedback;
            int index = _random.Next(feedbackArray.Length);
            return feedbackArray[index];
        }
    }
}
