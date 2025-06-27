using System;
using System.Collections.Generic;

namespace ChatbotPart3
{
    public class QuizQuestion
    {
        public string Question { get; set; }
        public string[] Options { get; set; }
        public int CorrectOptionIndex { get; set; }
        public string Explanation { get; set; }
        public string Category { get; set; }

        public QuizQuestion(string question, string[] options, int correctOptionIndex, string explanation, string category = "General")
        {
            Question = question;
            Options = options;
            CorrectOptionIndex = correctOptionIndex;
            Explanation = explanation;
            Category = category;
        }

        public bool IsCorrectAnswer(string answer)
        {
            // Check if answer is a letter (A, B, C, D)
            if (answer.Length == 1 && char.IsLetter(answer[0]))
            {
                int index = char.ToUpper(answer[0]) - 'A';
                return index == CorrectOptionIndex;
            }

            // Check if answer is a number (1, 2, 3, 4)
            if (int.TryParse(answer, out int numericAnswer))
            {
                return numericAnswer - 1 == CorrectOptionIndex;
            }

            // Check if answer matches the text of the correct option
            return answer.Trim().Equals(Options[CorrectOptionIndex], StringComparison.OrdinalIgnoreCase);
        }

        public string GetFormattedQuestion()
        {
            string result = $"Question: {Question}\n\n";

            for (int i = 0; i < Options.Length; i++)
            {
                result += $"{(char)('A' + i)}) {Options[i]}\n";
            }

            return result;
        }

        public string GetCorrectAnswerText()
        {
            return Options[CorrectOptionIndex];
        }

        public string GetCorrectAnswerLetter()
        {
            return ((char)('A' + CorrectOptionIndex)).ToString();
        }
    }
}
