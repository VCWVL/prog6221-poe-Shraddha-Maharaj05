using System.Media;

namespace ChatbotPart3
{
    public class GreetingService
    {
        public string GetWelcomeMessage()
        {
            return "Welcome to CyberBot!";
        }

        public void PlayWelcomeSound()
        {
            try
            {
                // Adjust path as needed or embed resource
                var player = new SoundPlayer("welcome.wav");
                player.Play();
            }
            catch
            {
                // Handle or ignore missing sound file gracefully
            }
        }
    }
}
