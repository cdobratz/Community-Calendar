# Organization Calendar Management System - Project Plan

## Project Overview
A GUI-based calendar management system for a residential organization with multiple houses, designed to manage events for 7 houses with 10 children each, plus staff events.

## System Requirements

### Organization Structure
- **Houses**: AV, SP, NL, LC, WH, HH, BP (7 total)
- **Capacity**: 10 children per house (~70 children total)
- **Staff**: Multiple roles with meeting requirements
- **Child Movement**: Rare but possible between houses

### Event Management
- **Event Types**: Meal, Campus Activity, Off Campus Activity, School Activity, Medical Appointment, Job
- **Categories**: Mandatory, Optional, House-specific
- **Scheduling**: Advanced planning with change capability
- **Recurring Events**: Support for repeating events
- **Time Slots**: Events require specific time scheduling

### User Access & Permissions
- **User Roles**: Admin, CM (Community Manager), HP (House Parent), Staff
- **Access Level**: All users can view and edit (demo version)
- **Future**: Microsoft Organization integration for production

### Technical Requirements
- **Platform**: Windows GUI application
- **Date Range**: 1 month back, 1 year forward
- **Printing**: Calendar export and printing capability
- **Multi-user**: Shared system for demo, network-ready for production

## Technology Stack Recommendations

### GUI Framework Options
1. **C# WinForms** (Recommended)
   - Native Windows integration
   - Easy to learn and implement
   - Good printing support
   - Microsoft ecosystem compatibility

2. **C++ with Qt** (Alternative)
   - Cross-platform capability
   - Rich GUI components
   - More complex but powerful

3. **Python with tkinter/PyQt** (Rapid Prototype)
   - Quick development
   - Easy to modify
   - Good for proof of concept

### Data Storage
- **SQLite Database** (Recommended for demo)
  - Self-contained, no server required
  - Good performance for small to medium data
  - Easy backup and transfer

## System Architecture

### Core Components

#### 1. Data Layer
```
- House Management
- Event Management  
- User Management
- Recurring Event Engine
- Data Persistence (SQLite)
```

#### 2. Business Logic Layer
```
- Calendar Engine
- Event Scheduling Logic
- Permission Management
- Recurring Event Processing
- Conflict Detection
```

#### 3. Presentation Layer
```
- Main Calendar View
- Event Management Dialogs
- House Filter Controls
- User Authentication
- Print/Export Interface
```

### Database Schema Design

#### Houses Table
```sql
Houses (
    house_id VARCHAR(2) PRIMARY KEY,  -- AV, SP, NL, etc.
    house_name VARCHAR(50),
    capacity INTEGER DEFAULT 10,
    current_occupancy INTEGER DEFAULT 0
)
```

#### Events Table
```sql
Events (
    event_id INTEGER PRIMARY KEY,
    title VARCHAR(100) NOT NULL,
    description TEXT,
    event_type VARCHAR(20), -- Meal, Campus Activity, etc.
    category VARCHAR(20),   -- Mandatory, Optional, House
    start_datetime DATETIME NOT NULL,
    end_datetime DATETIME,
    house_id VARCHAR(2),    -- NULL for community-wide events
    created_by VARCHAR(50),
    created_date DATETIME,
    is_recurring BOOLEAN DEFAULT FALSE,
    recurring_pattern VARCHAR(50),
    recurring_end_date DATE
)
```

#### Users Table
```sql
Users (
    user_id VARCHAR(50) PRIMARY KEY,
    username VARCHAR(50) UNIQUE,
    role VARCHAR(20), -- Admin, CM, HP, Staff
    house_assignment VARCHAR(2), -- For House Parents
    created_date DATETIME
)
```

#### Children Table (for future house assignment tracking)
```sql
Children (
    child_id INTEGER PRIMARY KEY,
    name VARCHAR(100),
    current_house VARCHAR(2),
    move_date DATE,
    move_reason TEXT
)
```

## Feature Specifications

### Phase 1 - Core Calendar (MVP)
1. **Basic Calendar Display**
   - Monthly grid view
   - Navigate between months
   - Display events on appropriate dates

2. **Event Management**
   - Add new events (basic form)
   - Edit existing events
   - Delete events
   - Event type and category selection

3. **House Filtering**
   - View all community events
   - Filter by specific house
   - Toggle between house and community view

### Phase 2 - Enhanced Features
1. **Time Slot Management**
   - Hourly time slots
   - Event duration display
   - Conflict detection and warnings

2. **Recurring Events**
   - Daily, weekly, monthly patterns
   - End date specification
   - Edit series vs. single occurrence

3. **User Authentication**
   - Simple login system
   - Role-based permissions
   - User session management

### Phase 3 - Advanced Features
1. **Printing & Export**
   - Monthly calendar printing
   - Event list exports
   - House-specific reports

2. **Advanced Scheduling**
   - Drag-and-drop event editing
   - Multi-day events
   - Event reminders/notifications

3. **Reporting & Analytics**
   - Event statistics
   - House activity reports
   - Attendance tracking

## User Interface Design

### Main Window Layout
```
+--------------------------------------------------+
|  File  View  Events  Reports  Help              |
+--------------------------------------------------+
| [Community View] [House: AV ▼] [Add Event]      |
+--------------------------------------------------+
| << Previous | August 2025 | Next >>             |
+--------------------------------------------------+
| Sun | Mon | Tue | Wed | Thu | Fri | Sat          |
+--------------------------------------------------+
|     |     |  1  |  2  |  3  |  4  |  5          |
|     |     | [M] | [CA]|     |     | [SA]        |
+--------------------------------------------------+
|  6  |  7  |  8  |  9  | 10  | 11  | 12          |
| [M] | [M] | [M] | [M] | [M] | [M] | [M]         |
+--------------------------------------------------+
| Status: Viewing Community Calendar              |
+--------------------------------------------------+
```

### Event Categories Color Coding
- **Mandatory**: Red background
- **Optional**: Blue background  
- **House-specific**: Green background
- **Recurring**: Bold border

## Development Timeline

### Week 1-2: Foundation
- Set up development environment
- Create basic project structure
- Design and implement database schema
- Create core data classes

### Week 3-4: Core Calendar
- Implement main calendar display
- Basic event CRUD operations
- House filtering functionality
- Navigation between months

### Week 5-6: Event Management
- Enhanced event forms
- Time slot implementation
- Basic recurring events
- Event validation and conflict detection

### Week 7-8: User Interface Polish
- Improve GUI design and usability
- Add keyboard shortcuts
- Implement drag-and-drop (if time permits)
- Error handling and user feedback

### Week 9-10: Advanced Features
- Printing functionality
- Data export capabilities
- User authentication system
- Testing and bug fixes

## Testing Strategy

### Test Data
- Create events for all 7 houses
- Mix of event types and categories
- Recurring and one-time events
- Edge cases (overlapping events, long titles)

### User Scenarios
1. **Community Manager**: Schedule all-community events
2. **House Parent**: Manage house-specific activities
3. **Staff Member**: Add medical appointments and job events
4. **Admin**: User management and system configuration

## Risk Assessment & Mitigation

### Technical Risks
- **GUI Complexity**: Start with simple forms, enhance incrementally
- **Data Corruption**: Regular backup mechanisms, data validation
- **Performance**: Optimize database queries, limit date ranges

### User Adoption Risks
- **Training Requirements**: Create user manual and training videos
- **Resistance to Change**: Involve users in design process
- **Data Migration**: Plan transition from current system

## Success Metrics

### Functionality Metrics
- All 7 houses can manage their calendars independently
- Community-wide events display correctly across all views
- Recurring events work reliably
- Printing produces usable calendar outputs

### Usability Metrics
- Users can add an event in under 2 minutes
- Navigation between months is intuitive
- House filtering works without confusion
- Error messages are clear and helpful

## Future Enhancements (Post-MVP)

### Integration Possibilities
- Microsoft Outlook calendar sync
- Mobile app companion
- SMS/email notifications
- Tablet interface for common areas

### Advanced Features
- Child assignment tracking
- Staff scheduling integration
- Budget tracking for events
- Photo attachments for events
- Attendance tracking and reports

## Conclusion

This project will significantly improve the organization's ability to coordinate activities across multiple houses while maintaining oversight of community-wide events. The phased approach ensures a working system is available quickly while allowing for future enhancements based on user feedback and changing needs.
