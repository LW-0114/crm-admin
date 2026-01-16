# CRM Admin – Training & Onboarding Module (Prototype)

## Overview
This project is a prototype Admin and Training Management module built as part of my Level 4 Software Developer apprenticeship. The aim is to explore how training, onboarding, and basic admin functionality could be handled outside of our existing CRM, while keeping the design flexible for future integration.

The application is intentionally built as a **stand-alone system** using mock user data and a local/Azure SQL database, allowing features to be developed and tested without depending on live production systems.

---

## Key Features
- User login using cookie authentication (mock users)
- Role-based access (Admin vs standard users)
- Admin Users page:
  - View users
  - Activate / deactivate user access
- Training videos:
  - View available videos
  - Sign off videos
  - Track completion status
- Onboarding records display
- Training sessions display
- Dashboard with summary counts
- Health check endpoints for monitoring
- Basic unit tests for core logic

---

## Technology Stack
- ASP.NET Core MVC
- C#
- Dapper
- SQL Server (LocalDB / Azure SQL)
- ASP.NET Authentication (Cookies)
- Bootstrap (UI)
- Git & GitHub for version control

---

## Running the Project Locally

### Prerequisites
- .NET SDK installed
- SQL Server LocalDB (or Azure SQL)
- Visual Studio / VS Code (optional)

### Steps
1. Clone the repository
2. Open the project folder:
3. Update `appsettings.Development.json` with your connection string
4. Run database schema and seed scripts
5. Run the app:
6. Browse to: http://localhost:<port>

---

## Test Login Details (Development)
- Username: any user from `MockMaximizerUsers`
- Password: `Passw0rd!`

Admin users will see additional admin navigation options.

---

## Notes on Design Decisions
- Mock users are used to avoid direct dependency on the existing CRM.
- User status (Active / Inactive) is stored separately to allow future integration.
- Features are built in small, testable steps.
- Cloud deployment was explored but local development was prioritised to reduce blockers.

---

## Future Enhancements
- Replace mock authentication with SSO or CRM-based authentication
- Automatic licence revocation after inactivity
- Outlook calendar integration for training bookings
- Expanded admin reporting
- Improved UI styling and accessibility

---

## Author
Developed as part of a Level 4 Software Developer Apprenticeship.
