# Community Calendar Management System

A Windows-based calendar management system designed for residential organizations with multiple houses. Built specifically for SACH (Services for At-risk Children & Housing) community management.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet)
![Windows Forms](https://img.shields.io/badge/Windows%20Forms-GUI-blue)
![SQLite](https://img.shields.io/badge/SQLite-Database-003B57?style=flat&logo=sqlite)
![License](https://img.shields.io/badge/License-MIT-green)

## 🏠 **Organization Structure**

**7 Houses Supported:**
- **AV** - Ashford Village
- **SP** - Spring Place  
- **NL** - North Lodge
- **LC** - Liberty Court
- **WH** - Westwood House
- **HH** - Heritage House
- **BP** - Brookside Place

## ✨ **Features**

### **Event Management**
- **6 Event Types**: Meal, Campus Activity, Off Campus Activity, School Activity, Medical Appointment, Job
- **3 Categories**: Mandatory, Optional, House-specific
- **Time Slots**: Full scheduling with start/end times
- **Recurring Events**: Daily, weekly, monthly patterns

### **Calendar Views**
- **Community View**: See all events across all houses
- **House View**: Filter events for specific houses
- **Monthly Grid**: Visual calendar with color-coded events
- **Navigation**: Easy month-to-month browsing

### **User Management**
- **4 User Roles**: Admin, Community Manager (CM), House Parent (HP), Staff
- **Permissions**: Role-based access control
- **House Assignments**: House Parents tied to specific houses

### **Data Integration**
- **TeamUp Import**: Import existing calendar data from TeamUp
- **SQLite Database**: Reliable local data storage
- **Export Options**: Print calendars and export event data

## 🎨 **Visual Design**

**Color Coding:**
- 🔴 **Red**: Mandatory events (required attendance)
- 🔵 **Blue**: Optional events (voluntary participation)
- 🟢 **Green**: House-specific events
- 🟡 **Yellow**: Today's date highlight

## 📋 **Requirements**

- **OS**: Windows 10/11
- **.NET**: 8.0 SDK or later
- **Memory**: 512 MB RAM minimum
- **Storage**: 100 MB free space

## 🚀 **Quick Start**

### **1. Prerequisites**
```bash
# Download and install .NET 8.0 SDK
# https://dotnet.microsoft.com/download/dotnet/8.0
```

### **2. Clone Repository**
```bash
git clone https://github.com/cdobratz/Community-Calendar.git
cd Community-Calendar
```

### **3. Build and Run**
```bash
# Navigate to the application folder
cd OrganizationCalendar

# Restore dependencies
dotnet restore

# Build the application
dotnet build

# Run the application
dotnet run
```

### **4. First Launch**
- Database will auto-initialize on first run
- Demo data will be populated for testing
- Default admin user: `admin` (no password for demo)

## 📁 **Project Structure**

```
Community-Calendar/
├── README.md                     # This file
├── PROJECT_PLAN.md              # Detailed development plan
├── TEAMUP_INTEGRATION.md        # TeamUp import guide
├── database_schema.sql          # SQLite database schema
├── OrganizationCalendar/        # Main C# application
│   ├── OrganizationCalendar.csproj
│   └── src/
│       ├── Program.cs           # Application entry point
│       ├── Models/              # Data models
│       │   ├── Event.cs
│       │   ├── House.cs
│       │   └── User.cs
│       ├── Data/                # Database layer
│       │   ├── DatabaseConnection.cs
│       │   ├── EventRepository.cs
│       │   └── DemoDataGenerator.cs
│       └── Forms/               # GUI components
│           └── MainCalendarForm.cs
└── cal.c                       # Original C calendar prototype
```

## 🔧 **Configuration**

### **Database Location**
By default, the SQLite database is created in the application directory as `organization_calendar.db`. 

### **Houses Configuration**
Houses are pre-configured in `Models/House.cs`. To modify:
1. Update `HouseData.DefaultHouses` list
2. Run database migration to update existing data

### **User Roles**
- **Admin**: Full system access, all houses
- **CM (Community Manager)**: Manage all houses and community events
- **HP (House Parent)**: Manage assigned house only
- **Staff**: View and basic editing permissions

## 📊 **Sample Data**

The system includes rich demo data:
- **Daily meals**: Breakfast, lunch, dinner
- **House meetings**: Weekly meetings for each house
- **Community events**: Monthly meetings, safety drills, social events
- **Activities**: Movie nights, study groups, outings
- **Appointments**: Medical, job-related activities

## 🔄 **TeamUp Migration**

To import existing TeamUp calendar data:

1. Export data from TeamUp as iCal (.ics) or CSV
2. Use **File > Import from TeamUp** menu
3. Map TeamUp calendars to house structure
4. Validate and import events

See [TEAMUP_INTEGRATION.md](TEAMUP_INTEGRATION.md) for detailed instructions.

## 🛣️ **Development Roadmap**

### **Phase 1 - MVP** ✅
- [x] Core calendar display
- [x] House filtering
- [x] Event management foundation
- [x] Database structure
- [ ] Add/Edit event dialogs
- [ ] Basic validation

### **Phase 2 - Enhanced Features**
- [ ] Recurring events
- [ ] User authentication
- [ ] Conflict detection
- [ ] TeamUp import utility

### **Phase 3 - Advanced Features**
- [ ] Printing and export
- [ ] Advanced scheduling
- [ ] Reporting and analytics
- [ ] Mobile companion app

## 🤝 **Contributing**

This project is specifically designed for SACH community needs. For feature requests or bug reports:

1. Open an issue describing the request
2. Include house/user role context
3. Provide specific use cases

## 📝 **License**

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🏗️ **Built With**

- **[.NET 8.0](https://dotnet.microsoft.com/)** - Application framework
- **[Windows Forms](https://docs.microsoft.com/en-us/dotnet/desktop/winforms/)** - GUI framework
- **[SQLite](https://www.sqlite.org/)** - Database engine
- **[System.Data.SQLite](https://www.nuget.org/packages/System.Data.SQLite/)** - SQLite ADO.NET provider

## 📞 **Support**

For technical support or questions about implementation:
- Create an issue on GitHub
- Include system info (Windows version, .NET version)
- Describe expected vs actual behavior

---

**Built with ❤️ for the SACH Community**
