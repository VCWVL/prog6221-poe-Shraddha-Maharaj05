using ChatbotPart3;
using System;
using System.Text;
using System.Threading.Tasks;

namespace ChatbotPart3
{
    public class DisplayService
    {
        // method to resolve 'DisplayAsciiArt' error
        public void DisplayAsciiArt()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(GetAsciiArt());
            Console.ResetColor();
        }

        // method to resolve 'DisplayWelcomeMessage' error
        public async Task DisplayWelcomeMessageAsync(string name)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            await TypeOutMessage(GetWelcomeMessageBox(name));
            Console.ResetColor();
        }

        // method to resolve 'DisplayGoodbyeMessage' error
        public void DisplayGoodbyeMessage(string userName)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(GetGoodbyeMessage(userName));
            Console.ResetColor();
        }

        // method to resolve 'DisplayInvalidChoiceMessage' error
        public void DisplayInvalidChoiceMessage()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(GetInvalidChoiceMessage());
            Console.ResetColor();
        }

        // Returns the ASCII art banner as a string
        public string GetAsciiArt()
        {
            return @"
   ______   ______  _____ ____  ____  _____ ____ _   _ ____  ___ _______   __
  / ___\ \ / / __ )| ____|  _ \/ ___|| ____/ ___| | | |  _ \|_ _|_   _\ \ / /
 | |    \ V /|  _ \|  _| | |_) \___ \|  _|| |   | | | | |_) || |  | |  \ V / 
 | |___  | | | |_) | |___|  _ < ___) | |__| |___| |_| |  _ < | |  | |   | |  
  \____| |_| |____/|_____|_| \_\____/|_____\____|\___/|_| \_\___| |_|   |_|
     _ _        _ _   _____ ________  __ _______________       _____   _ _ _ _ _ 
    / \ \      / / \  |  _ \| ____| \ | | ____/ ___/ ___|      | __ ) / _ \_   _|
   / _ \ \ /\ / / _ \ | |_) |  _| |  \| |  _| \___ \___ \      |  _ \| | | || |  
  / ___ \ V  V / ___ \|  _ <| |___| |\  | |___ ___) |__) |     | |_) | |_| || |  
 /_/   \_\_/\_/_/   \_\_| \_\_____|_| \_|_____|____/____/      |____/ \___/ |_|  
";
        }

        // Returns a formatted welcome message box as a string
        public string GetWelcomeMessageBox(string name)
        {
            string welcomeMessage = $" Hello, {name}! I'm your Cybersecurity Awareness bot.";
            string learnMessage = " What would you like to learn about?";

            int boxWidth = Math.Max(welcomeMessage.Length, learnMessage.Length) + 4;
            string border = new string('═', boxWidth);

            var sb = new StringBuilder();
            sb.AppendLine($"╔{border}╗");
            sb.AppendLine($"║ {welcomeMessage.PadRight(boxWidth - 2)} ║");
            sb.AppendLine($"║ {learnMessage.PadRight(boxWidth - 2)} ║");
            sb.AppendLine($"╚{border}╝");
            sb.AppendLine();

            return sb.ToString();
        }

        // Returns a goodbye message as a string
        public string GetGoodbyeMessage(string userName)
        {
            return $"Thank you for chatting, {userName}! Come back soon! :)";
        }

        // Returns invalid choice message string
        public string GetInvalidChoiceMessage()
        {
            return "I'm not sure I understand. Can you try rephrasing by typing a valid topic (e.g., phishing, password safety, suspicious links) or task command (e.g., add task, view tasks).\n";
        }

        // Returns tips displayed inside a box as a formatted string
        public string GetTipsBox(string[] lines, ConsoleColor borderColor = ConsoleColor.Cyan)
        {
            int width = 0;
            foreach (string line in lines)
                if (line.Length > width) width = line.Length;

            width += 4; // padding
            string border = new string('═', width);

            var sb = new StringBuilder();
            sb.AppendLine($"╔{border}╗");
            foreach (string line in lines)
            {
                sb.AppendLine($"║ {line.PadRight(width - 2)} ║");
            }
            sb.AppendLine($"╚{border}╝\n");

            return sb.ToString();
        }

        // Display task information in a formatted box
        public string GetTaskBox(CyberTask task)
        {
            string[] lines = {
                $"Title: {task.Title}",
                $"Description: {task.Description}",
                $"Status: {(task.IsCompleted ? "Completed" : "Pending")}",
                $"Reminder: {(task.ReminderDate.HasValue ? task.ReminderDate.Value.ToShortDateString() : "None")}"
            };

            ConsoleColor color = task.IsCompleted ? ConsoleColor.Green :
                                (task.ReminderDate.HasValue && task.ReminderDate.Value.Date <= DateTime.Now.Date) ?
                                ConsoleColor.Red : ConsoleColor.Cyan;

            return GetTipsBox(lines, color);
        }

        // Method to simulate typing out a message
        private async Task TypeOutMessage(string message)
        {
            foreach (char c in message)
            {
                Console.Write(c);
                await Task.Delay(50); // Adjust the delay for typing speed
            }
            Console.WriteLine(); // Move to the next line after the message
        }
    }
}
