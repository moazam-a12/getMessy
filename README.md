# GetMessy - Mess Management System

A comprehensive ASP.NET Core web application for managing mess/cafeteria operations including user management, menu planning, attendance tracking, billing, and payment processing.

## 🚀 Features

- **User Management**: Role-based access control (Admin/User)
- **Menu Management**: Dynamic menu creation and updates
- **Attendance Tracking**: Daily attendance monitoring
- **Billing System**: Automated bill generation
- **Payment Integration**: Stripe payment gateway integration
- **PDF Generation**: Export bills as PDF documents
- **JWT Authentication**: Secure token-based authentication
- **Responsive UI**: TailwindCSS-based modern interface
- **AJAX Operations**: Seamless user experience without page reloads

## 📋 Prerequisites

- **.NET 8 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Node.js 18+** - [Download](https://nodejs.org/) (for frontend asset building)
- **SQL Server** - Local instance or cloud (Azure SQL, AWS RDS, etc.)
- **Stripe Account** - For payment processing (test keys for development)

## 🛠️ Installation & Setup

### 1. Clone the Repository
```bash
git clone <your-repo-url>
cd GetMessy
```

### 2. Configure Application Settings
Create `appsettings.json` from the template:
```bash
cp appsettings.Template.json appsettings.json
```

Edit `appsettings.json` with your configuration:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=YOUR_SERVER;Initial Catalog=YOUR_DB;User Id=YOUR_USER;Password=YOUR_PASSWORD"
  },
  "JwtSettings": {
    "SecretKey": "YOUR_SECRET_KEY_MIN_32_CHARACTERS",
    "Issuer": "MessManagementSystem",
    "Audience": "MessManagementSystemUsers",
    "ExpiryInHours": 24
  },
  "Stripe": {
    "PublishableKey": "pk_test_YOUR_KEY",
    "SecretKey": "sk_test_YOUR_KEY",
    "WebhookSecret": "whsec_YOUR_SECRET"
  }
}
```

### 3. Database Setup
Run the SQL setup script to create the database schema:
```bash
# Use SQL Server Management Studio, Azure Data Studio, or command line
sqlcmd -S YOUR_SERVER -d YOUR_DATABASE -i MSSQL_DATABASE_SETUP.sql
```

Or use EF Core migrations:
```bash
dotnet ef database update
```

### 4. Install Dependencies
```bash
# Restore .NET packages
dotnet restore

# Install Node.js packages (for frontend tooling)
npm install
```

### 5. Build Frontend Assets (Optional)
If you're modifying TailwindCSS styles:
```bash
npm run build
```

### 6. Run the Application
```bash
dotnet run
```

The application will start at `https://localhost:5001` (or the port specified in launchSettings.json)

## 📁 Project Structure

```
GetMessy/
├── Controllers/          # MVC Controllers
│   ├── AdminController.cs
│   ├── LoginController.cs
│   ├── RegisterController.cs
│   ├── UserMenuController.cs
│   ├── PaymentController.cs
│   └── ...
├── Models/              # Data models and ViewModels
│   ├── User.cs
│   ├── Menu.cs
│   ├── Bill.cs
│   ├── Attendance.cs
│   └── ...
├── Services/           # Business logic services
│   ├── JwtService.cs
│   ├── PdfService.cs
│   └── StripeService.cs
├── Views/             # Razor views
│   ├── Admin/
│   ├── User/
│   ├── Login/
│   └── Shared/
├── Migrations/        # EF Core database migrations
├── wwwroot/          # Static files (CSS, JS, images)
│   ├── css/
│   ├── js/
│   └── lib/
├── Program.cs        # Application entry point
├── appsettings.Template.json  # Configuration template
└── GetMessy.csproj   # Project file
```

## 🔒 Security Notes

**⚠️ IMPORTANT: This repository does NOT contain sensitive credentials**

- `appsettings.json` is excluded from version control
- Create your own `appsettings.json` from `appsettings.Template.json`
- Never commit real database passwords, JWT secrets, or Stripe API keys
- Use environment variables in production deployments

## 🎨 Frontend Technologies

- **TailwindCSS** - Utility-first CSS framework
- **Bootstrap 5** - Component library
- **jQuery** - DOM manipulation and AJAX
- **jQuery Validation** - Form validation

## 🗄️ Database

The application uses **Microsoft SQL Server** with Entity Framework Core for data access.

**Main Tables:**
- Users
- Menu
- Bills
- Attendance

See `MSSQL_DATABASE_SETUP.sql` for complete schema.

## 📚 API Documentation

Refer to `API_Documentation.md` for detailed API endpoints and usage.

## 🚢 Deployment

Deployment guides are available in `DEPLOYMENT.md`. The application can be deployed to:
- Azure App Service
- AWS Elastic Beanstalk
- IIS (Windows Server)
- Linux with Nginx/Apache reverse proxy

## 🧪 Development

### Running in Development Mode
```bash
dotnet watch run
```

### Build for Production
```bash
dotnet publish -c Release -o ./publish
```

## 📚 Complete Documentation

For comprehensive project documentation including architecture, API endpoints, deployment guides, and development workflow, see **[PROJECT_COMPLETE_GUIDE.md](PROJECT_COMPLETE_GUIDE.md)**.

This consolidated guide contains:
- Detailed system architecture and database design
- Complete API documentation with examples
- Step-by-step deployment instructions
- AJAX operations and frontend implementation
- Payment integration with Stripe
- Development workflow and contribution guidelines

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

This project is created for educational purposes as part of a university project.

## 👨‍💻 Authors

- University Project - Enterprise and Application Development
- UET - Semester 7

## 🐛 Known Issues

- Vendor libraries in `wwwroot/lib/` must be restored via npm/libman
- First-time setup requires manual database initialization

## 📞 Support

For issues and questions, please open an issue in the GitHub repository.

---

**Happy Coding! 🎉**
