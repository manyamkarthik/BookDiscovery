# 📚 BookDiscovery

A modern web application for discovering and managing your reading journey, powered by the Open Library API.

🔗 **Live Demo:** [https://bookdiscovery-production.up.railway.app/](https://bookdiscovery-production.up.railway.app/)

## 🌟 Features

- **Book Search**: Search millions of books using the Open Library database
- **Reading Lists**: Create and manage personalized reading lists
- **Book Details**: View comprehensive information about books including:
  - Cover images
  - Author information
  - Publication details
  - ISBN numbers
  - Descriptions
- **Reading Progress**: Track which books you want to read, are currently reading, or have completed
- **Search History**: Keep track of your recent searches
- **User Dashboard**: Personal dashboard to manage all your reading activities

## 🛠️ Technologies

- **Framework**: ASP.NET Core 8.0 with Razor Pages
- **Database**: PostgreSQL
- **ORM**: Entity Framework Core
- **API Integration**: Open Library API
- **Deployment**: Railway
- **Frontend**: Bootstrap 5, HTML/CSS
- **Architecture**: MVC Pattern

## 🚀 Getting Started

### Prerequisites
- .NET 8.0 SDK
- PostgreSQL (for production) or SQL Server (for development)
- Visual Studio 2022 or VS Code

### Local Development
1. Clone the repository:
   ```bash
   git clone https://github.com/manyamkarthik/BookDiscovery.git
   cd BookDiscovery
   ```

2. Configure the database connection in `appsettings.json`

3. Run database migrations:
   ```bash
   dotnet ef database update
   ```

4. Start the application:
   ```bash
   dotnet run
   ```

5. Open https://localhost:5001 in your browser

### Deployment
The application is configured for deployment on Railway with PostgreSQL. The deployment process includes:
- Automatic database migrations
- Environment-based configuration
- Docker containerization

## 🏗️ Project Structure

```
BookDiscovery/
├── Controllers/          # MVC Controllers
├── Data/                # Database context and configurations
├── Models/              # Data models
├── Pages/               # Razor Pages
│   ├── Books/          # Book-related pages
│   ├── ReadingLists/   # Reading list management
│   └── Shared/         # Shared components
├── Services/            # Business logic services
├── wwwroot/            # Static files (CSS, JS, images)
└── Program.cs          # Application entry point
```

## 🔄 API Integration

The application integrates with the Open Library API to provide:
- Book search functionality
- Book metadata retrieval
- Cover image fetching
- Author information

## 📝 Database Schema

- **Books**: Stores book information
- **ReadingLists**: User-created reading lists
- **ReadingListBooks**: Junction table for many-to-many relationship
- **SearchHistories**: Tracks user searches

## 🚦 Environment Variables

Required environment variables for production:
- `DATABASE_URL`: PostgreSQL connection string
- `ASPNETCORE_ENVIRONMENT`: Set to "Production"
- `PORT`: Application port (automatically set by Railway)

## 🤝 Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🙏 Acknowledgments

- Open Library API for providing book data
- Railway for hosting services
- ASP.NET Core team for the excellent framework

---
Made with ❤️ by Manyam Karthik
