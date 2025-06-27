ReadMe for CyberBot WPF Application
Overview
CyberBot is a WPF application designed to enhance cybersecurity awareness through interactive conversations. It provides users with information on various cybersecurity topics, manages tasks, and conducts quizzes to test knowledge.

Table of Contents
Setup Instructions
Usage
Features
Example Interaction
Contributing
License
Setup Instructions
Prerequisites

Ensure you have .NET Framework installed (version 4.7.2 or higher).
Visual Studio (2019 or later) for building and running the application.

Clone the Repository
git clone https://github.com/prog6221-part-2-Shraddha-Maharaj05/CyberBot.git
Open the Project

Open the solution file CyberBot.sln in Visual Studio.
Build the Project

Right-click on the solution in Solution Explorer and select "Build".
Run the Application

Press F5 or click on the "Start" button to run the application.

Usage

Upon launching, the user is greeted and prompted to enter their name.
The bot will ask a series of questions to assess the user's cybersecurity knowledge and interests.
Users can interact with the bot by typing commands or questions related to cybersecurity topics, task management, or quizzes.

Commands

- Learning Topics: Ask about topics like "phishing", "password safety", "suspicious links", "privacy", "social engineering", and "identity theft".
- Task Management: Use commands like "add task", "view tasks", "complete task", and "delete task".
- Quizzes: Start a quiz by typing "start quiz" or inquire about quiz categories with "quiz categories".
- Activity Log: View recent actions by typing "show activity log".
 
Features

- Interactive Chat: Engages users in conversation to provide information and assistance.
- Task Management: Allows users to create, view, complete, and delete tasks.
- Quizzes: Tests users' knowledge on various cybersecurity topics with feedback.
- Activity Logging: Keeps track of user interactions and activities for better personalization.

  Example Interaction

  Bot: Welcome to CyberBot! I'm your Cybersecurity Awareness bot.
What would you like to do next?
- Learn about: phishing, password safety, suspicious links, privacy, social engineering, identity theft
- Task management: add task, view tasks, complete task, delete task
- Take a quiz: start quiz, quiz categories
Type 'exit' to quit.

User: I need to remember to update my passwords tomorrow

Bot: Task added: Update passwords
Update passwords to be strong and unique across all your accounts.
Reminder set for 6/28/2025

What else would you like to do?

User: Can you remind me to enable two-factor authentication in 3 days?

Bot: Task added: Enable two-factor authentication
Enable two-factor authentication on your important accounts for added security.
Reminder set for 6/30/2025

What else would you like to do?

User: Show me my tasks

Bot: Your Cybersecurity Tasks:
1. Update passwords - Update passwords to be strong and unique across all your accounts.
Pending | Reminder: 6/28/2025 (1 day remaining)

2. Enable two-factor authentication - Enable two-factor authentication on your important accounts for added security.
   Pending | Reminder: 6/30/2025 (3 days remaining)

What else would you like to do?

User: I finished the first task

Bot: Marked "Update passwords" as completed.

What else would you like to do?

User: Tell me about email scams

Bot: [Phishing information displayed]

🛡 Context tip: Spear phishing uses personal info to trick you. Always double-check email authenticity.
Want to test your knowledge? Type 'start quiz phishing' to take a quiz on this topic!

What would you like to do next?
- Learn about: phishing, password safety, suspicious links, privacy, social engineering, identity theft
- Task management: add task, view tasks, complete task, delete task
- Take a quiz: start quiz, quiz categories
Type 'exit' to quit.

User: What have you done for me?

Bot: Here's a summary of recent actions:
Tasks: 2 total (1 completed, 1 pending)
Tasks with reminders:
1. Update passwords - Completed, reminder on 6/28/2025
2. Enable two-factor authentication - Pending, reminder on 6/30/2025

Topics you've asked about: 1
Most frequently discussed topics:
- phishing (1 times)

What would you like to do next?
- Learn about: phishing, password safety, suspicious links, privacy, social engineering, identity theft
- Task management: add task, view tasks, complete task, delete task
- Take a quiz: start quiz, quiz categories
Type 'exit' to quit.

User: Test my knowledge about phishing

Bot: Welcome to the Phishing Quiz!
I'll ask you 5 questions about phishing.
Answer by typing the letter (A, B, C, D) or number (1, 2, 3, 4) of your choice.

Question 1 of 5:
Question: What should you do if you receive an email asking for your password?
A) Reply with your password
B) Delete the email
C) Report the email as phishing
D) Ignore it

User: C

Bot: Correct! Well done!
Reporting phishing emails helps protect you and others from scams. Never share your password via email.

Question 2 of 5:
[Quiz continues...]

What would you like to do next?
- Learn about: phishing, password safety, suspicious links, privacy, social engineering, identity theft
- Task management: add task, view tasks, complete task, delete task
- Take a quiz: start quiz, quiz categories
Type 'exit' to quit.

User: exit

Bot:Thank you for chatting, {userName}! Come back soon! :)

Contributing
Contributions are welcome! Please follow these steps:

Fork the repository.
Create a new branch (git checkout -b feature-branch).
Make your changes and commit them (git commit -m 'Add new feature').
Push to the branch (git push origin feature-branch).
Create a pull request.
License
This project is licensed under the MIT License - see the LICENSE file for details.
