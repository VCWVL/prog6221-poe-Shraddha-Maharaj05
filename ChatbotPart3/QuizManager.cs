using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ChatbotPart3
{
    public class QuizManager
    {
        private readonly QuizService _quizService; // Service to manage quiz questions
        private readonly DisplayService _displayService; // Service to handle display output
        private List<QuizQuestion> _currentQuizQuestions; // List of questions for the current quiz
        private int _currentQuestionIndex; // Index of the current question being answered
        private int _correctAnswers; // Count of correct answers given by the user
        private bool _quizInProgress; // Flag to check if a quiz is currently active

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

        private readonly Random _random = new Random(); // Random number generator for feedback selection

        // Constructor to initialize QuizManager with services
        public QuizManager(QuizService quizService, DisplayService displayService)
        {
            _quizService = quizService;
            _displayService = displayService;
            _quizInProgress = false; // Initialize quiz status
        }

        // Process user input for quiz commands
        public string ProcessQuizCommand(string input)
        {
            // If a quiz is in progress, process the answer
            if (_quizInProgress)
            {
                return ProcessAnswer(input);
            }

            input = input.ToLower().Trim(); // Normalize input

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
                            return StartCategoryQuiz(category); // Start quiz for the specific category
                        }
                    }

                    // Start a general quiz if no specific category
                    return StartQuiz();

                case "quiz_categories":
                    return GetAvailableCategories(); // Return available quiz categories

                default:
                    // If it's a quiz-related command but not handled above
                    if (input.Contains("quiz"))
                    {
                        return "🎮 To start a cybersecurity quiz, type 'start quiz' or 'quiz categories' to see specific topics.";
                    }
                    return null; // No valid command found
            }
        }

        // Detect the type of quiz command from user input
        private string DetectQuizCommandType(string input)
        {
            foreach (var pattern in _quizCommandPatterns)
            {
                if (pattern.Value.Any(phrase => input.Contains(phrase)))
                {
                    return pattern.Key; // Return the command type if a match is found
                }
            }
            return null; // No command type detected
        }

        // Start a general quiz with a specified number of questions
        public string StartQuiz(int questionCount = 5)
        {
            _currentQuizQuestions = _quizService.GetRandomQuestions(questionCount); // Get random questions
            _currentQuestionIndex = 0; // Reset question index
            _correctAnswers = 0; // Reset correct answers count
            _quizInProgress = true; // Set quiz status to in progress

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("🎮 Welcome to the Cybersecurity Quiz! 🎮");
            sb.AppendLine($"I'll ask you {_currentQuizQuestions.Count} questions about cybersecurity.");
            sb.AppendLine("Answer by typing the letter (A, B, C, D) or number (1, 2, 3, 4) of your choice.");
            sb.AppendLine();
            sb.AppendLine(GetCurrentQuestion()); // Display the first question

            return sb.ToString(); // Return the quiz introduction
        }

        // Start a quiz for a specific category
        public string StartCategoryQuiz(string category, int questionCount = 5)
        {
            _currentQuizQuestions = _quizService.GetQuestionsByCategory(category, questionCount); // Get questions for the category
            _currentQuestionIndex = 0; // Reset question index
            _correctAnswers = 0; // Reset correct answers count
            _quizInProgress = true; // Set quiz status to in progress

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"🎮 Welcome to the {category} Quiz! 🎮");
            sb.AppendLine($"I'll ask you {_currentQuizQuestions.Count} questions about {category.ToLower()}.");
            sb.AppendLine("Answer by typing the letter (A, B, C, D) or number (1, 2, 3, 4) of your choice.");
            sb.AppendLine();
            sb.AppendLine(GetCurrentQuestion()); // Display the first question

            return sb.ToString(); // Return the category quiz introduction
        }

        // Process the user's answer to the current question
        public string ProcessAnswer(string answer)
        {
            if (!_quizInProgress)
            {
                return "There's no active quiz. Type 'start quiz' to begin!"; // Prompt to start a quiz if none is active
            }

            QuizQuestion currentQuestion = _currentQuizQuestions[_currentQuestionIndex]; // Get the current question
            bool isCorrect = currentQuestion.IsCorrectAnswer(answer); // Check if the answer is correct

            StringBuilder sb = new StringBuilder();

            if (isCorrect)
            {
                sb.AppendLine(GetRandomFeedback(true)); // Provide positive feedback
                _correctAnswers++; // Increment correct answers count
            }
            else
            {
                sb.AppendLine($"{GetRandomFeedback(false)} {currentQuestion.GetCorrectAnswerLetter()}) {currentQuestion.GetCorrectAnswerText()}"); // Provide negative feedback with correct answer
            }

            sb.AppendLine(currentQuestion.Explanation); // Provide explanation for the answer

            _currentQuestionIndex++; // Move to the next question

            if (_currentQuestionIndex < _currentQuizQuestions.Count)
            {
                sb.AppendLine();
                sb.AppendLine(GetCurrentQuestion()); // Display the next question
            }
            else
            {
                sb.AppendLine();
                sb.AppendLine(GetQuizResults()); // Display quiz results if all questions have been answered
                _quizInProgress = false; // End the quiz
            }

            return sb.ToString(); // Return the response
        }

        // Check if a quiz is currently in progress
        public bool IsQuizInProgress()
        {
            return _quizInProgress; // Return the quiz status
        }

        // Get available quiz categories
        public string GetAvailableCategories()
        {
            var categories = _quizService.GetAvailableCategories(); // Retrieve available categories

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("📚 Available Quiz Categories:");

            foreach (var category in categories)
            {
                sb.AppendLine($"- {category}"); // List each category
            }

            sb.AppendLine();
            sb.AppendLine("Type 'start quiz [category]' to begin a category quiz, or just 'start quiz' for a mixed quiz.");

            return sb.ToString(); // Return the list of categories
        }

        // Get the current question to display
        private string GetCurrentQuestion()
        {
            if (_currentQuestionIndex >= _currentQuizQuestions.Count)
            {
                return "No more questions!"; // Return message if no questions are left
            }

            QuizQuestion question = _currentQuizQuestions[_currentQuestionIndex]; // Get the current question

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Question {_currentQuestionIndex + 1} of {_currentQuizQuestions.Count}:");
            sb.AppendLine(question.GetFormattedQuestion()); // Format and display the current question

            return sb.ToString(); // Return the current question
        }

        // Get the results of the quiz after completion
        private string GetQuizResults()
        {
            int percentage = (_correctAnswers * 100) / _currentQuizQuestions.Count; // Calculate score percentage
            string feedback = GetScoreFeedback(percentage); // Get feedback based on score

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("🏁 Quiz Complete! 🏁");
            sb.AppendLine($"You scored {_correctAnswers} out of {_currentQuizQuestions.Count} ({percentage}%)");
            sb.AppendLine();
            sb.AppendLine(feedback); // Provide feedback based on score
            sb.AppendLine();
            sb.AppendLine("Type 'start quiz' to try again with different questions!");

            return sb.ToString(); // Return the quiz results
        }

        // Get feedback based on the user's score percentage
        private string GetScoreFeedback(int percentage)
        {
            // Find the highest threshold that the score meets
            int threshold = _scoreFeedback.Keys
                .Where(key => percentage >= key)
                .OrderByDescending(key => key)
                .FirstOrDefault();

            return _scoreFeedback[threshold]; // Return feedback for the corresponding score
        }

        // Get random feedback based on whether the answer was correct or incorrect
        private string GetRandomFeedback(bool isCorrect)
        {
            string[] feedbackArray = isCorrect ? _correctFeedback : _incorrectFeedback; // Select feedback array based on correctness
            int index = _random.Next(feedbackArray.Length); // Get a random index
            return feedbackArray[index]; // Return random feedback
        }
    }
}
