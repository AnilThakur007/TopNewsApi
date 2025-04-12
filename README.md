# Hacker News Viewer

## Overview
This project is a solution to view the newest stories from the Hacker News API. The application consists of an Angular front-end app and a C# .NET Core back-end RESTful API working together to deliver an efficient and user-friendly experience.

---

## Features

### Front-End (Angular)
1. **Newest Stories List:** Displays the latest stories from Hacker News.
2. **Story Details:** Each list item includes the title and a link to the news article (with handling for stories without hyperlinks).
3. **Search:** A search feature to allow users to find specific stories.
4. **Paging Mechanism:** Limits the number of stories shown per page to ensure smooth user experience.
5. **Automated Tests:** Ensures the code quality and functionality of the front-end.

### Back-End (C# .NET Core)
1. **Dependency Injection:** Makes use of built-in dependency injection for better modularity and testing.
2. **Caching:** Implements caching to store the newest stories for faster data retrieval.
3. **Automated Tests:** Ensures the reliability and correctness of the back-end code.

---

## Installation & Setup

### Prerequisites
- [Node.js](https://nodejs.org/)
- [Angular CLI](https://angular.io/cli)
- [.NET Core SDK](https://dotnet.microsoft.com/download)

### Steps
1. Clone the repository:
   ```bash
   git clone <repository-url>
