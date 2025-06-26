using System;
using System.Text;

namespace ChatbotPart3
{
    public class DisplayService
    {
        // FIX: Add method to resolve 'DisplayAsciiArt' error
        public void DisplayAsciiArt()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(GetAsciiArt());
            Console.ResetColor();
        }

        // FIX: Add method to resolve 'DisplayWelcomeMessage' error
        public void DisplayWelcomeMessage(string name)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine(GetWelcomeMessageBox(name));
            Console.ResetColor();
        }

        // FIX: Add method to resolve 'DisplayGoodbyeMessage' error
        public void DisplayGoodbyeMessage(string userName)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(GetGoodbyeMessage(userName));
            Console.ResetColor();
        }

        // FIX: Add method to resolve 'DisplayInvalidChoiceMessage' error
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
  ______   ______  _____ ____  ____  _____ ____ _   _ ____  ___ _______   __        .----.
 / ___\ \ / / __ )| ____|  _ \/ ___|| ____/ ___| | | |  _ \|_ _|_   _\ \ / /       / .--. \
| |    \ V /|  _ \|  _| | |_) \___ \|  _|| |   | | | | |_) || |  | |  \ V /       | |    | |
| |___  | | | |_) | |___|  _ < ___) | |__| |___| |_| |  _ < | |  | |   | |        | |____| |
 \____| |_| |____/|_____|_| \_\____/|_____\____|\___/|_| \_\___| |_|_ _|_|_       |  ____  |
   / \ \      / / \  |  _ \| ____| \ | | ____/ ___/ ___|  | __ ) / _ \_   _|      | |    | |
  / _ \ \ /\ / / _ \ | |_) |  _| |  \| |  _| \___ \___ \  |  _ \| | | || |        | | -- | |
 / ___ \ V  V / ___ \|  _ <| |___| |\  | |___ ___) |__) | | |_) | |_| || |        | |____| |
/_/   \_\_/\_/_/   \_\_| \_\_____|_| \_|_____|____/____/  |____/ \___/ |_|        |________|                                                                  
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
            return "I'm not sure I understand. Can you try rephrasing by typing a valid topic (e.g., phishing, password safety, suspicious links).\n";
        }

        // Returns tips displayed inside a box as a formatted string
        public string GetTipsBox(string[] lines)
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
    }
}
