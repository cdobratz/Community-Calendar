# TeamUp Calendar Integration Guide

## TeamUp Calendar Information
- **URL**: https://teamup.com/kszfossy9fif4gu8ei
- **Calendar Name**: SACH Calendars

## Data Migration Options

### Option 1: Manual Export/Import (Recommended for Initial Setup)
TeamUp provides several export formats:
1. **iCal (.ics) Export**
   - Most compatible format
   - Contains event details, dates, times, descriptions
   - Can be processed programmatically

2. **CSV Export**
   - Spreadsheet format
   - Easy to parse and validate
   - Good for bulk data processing

3. **Excel Export**
   - Similar to CSV but with formatting
   - Can contain additional metadata

### Option 2: TeamUp API Integration (For Ongoing Sync)
TeamUp offers API access for:
- Real-time synchronization
- Automated data updates
- Bi-directional sync capability

## Implementation Plan

### Phase 1: Manual Data Import
1. **Export Current Data from TeamUp**
   ```
   - Go to TeamUp calendar
   - Use Export function (Settings > Export)
   - Download as iCal (.ics) format
   - Save to project folder
   ```

2. **Create Import Utility**
   ```csharp
   // C# class to parse iCal files
   public class TeamUpImporter
   {
       public List<Event> ParseICalFile(string filePath)
       public void ImportToDatabase(List<Event> events)
       public void MapTeamUpToOrgStructure(Event teamUpEvent)
   }
   ```

3. **Data Mapping Strategy**
   ```
   TeamUp Field -> Our System Field
   - Event Title -> Events.title
   - Description -> Events.description
   - Start Date/Time -> Events.start_datetime
   - End Date/Time -> Events.end_datetime
   - Calendar/Category -> Events.house_id (needs mapping)
   - Event Type -> Events.event_type (needs mapping)
   ```

### Phase 2: Automated Synchronization
1. **TeamUp API Integration**
   - Register for TeamUp API access
   - Implement OAuth authentication
   - Create sync service

2. **Sync Strategy**
   - Daily automatic sync
   - Conflict resolution rules
   - Change tracking and auditing

## Data Mapping Challenges

### House Assignment Mapping
TeamUp calendars may not directly map to your house structure:
- **Need manual mapping table**: TeamUp Calendar Name -> House ID
- **Example mappings**:
  ```
  "Ashford Village Activities" -> "AV"
  "Spring Place Events" -> "SP"
  "Community Wide" -> NULL (community events)
  ```

### Event Type Classification
TeamUp events need to be classified into your event types:
- **Meal**: Look for keywords like "breakfast", "lunch", "dinner"
- **Campus Activity**: Activities happening on-site
- **Off Campus Activity**: Field trips, outings
- **School Activity**: Educational events
- **Medical Appointment**: Healthcare related
- **Job**: Work-related activities

### Category Assignment
Map TeamUp events to Mandatory/Optional/House categories:
- **Default**: Optional (unless specified)
- **Keywords for Mandatory**: "required", "mandatory", "attendance required"
- **House-specific**: Events with specific house names

## Implementation Code Structure

### 1. iCal Parser Class
```csharp
public class ICalParser
{
    public List<CalendarEvent> ParseFile(string filePath)
    {
        // Parse .ics file format
        // Extract VEVENT components
        // Convert to internal Event objects
    }
}
```

### 2. TeamUp Event Mapping
```csharp
public class TeamUpEventMapper
{
    private Dictionary<string, string> houseMapping;
    private Dictionary<string, string> eventTypeMapping;
    
    public Event MapToInternalEvent(CalendarEvent teamUpEvent)
    {
        // Apply business rules for mapping
        // Assign house, event type, category
        // Handle recurring events
    }
}
```

### 3. Import Validation
```csharp
public class ImportValidator
{
    public ValidationResult ValidateEvent(Event evt)
    {
        // Check required fields
        // Validate date ranges
        // Check for conflicts
        // Verify house assignments
    }
}
```

## User Interface for Import

### Import Dialog Features
1. **File Selection**: Browse for iCal/CSV files
2. **Mapping Configuration**: 
   - TeamUp calendar to house mapping
   - Event type classification rules
3. **Preview and Validation**:
   - Show events to be imported
   - Highlight potential issues
   - Allow manual corrections
4. **Import Progress**: 
   - Progress bar
   - Success/error reporting
   - Rollback capability

### Import Workflow
```
1. Select TeamUp export file
2. Preview imported events
3. Configure house/type mappings
4. Validate all events
5. Resolve conflicts/errors
6. Execute import
7. Generate import report
```

## Technical Requirements

### Required Libraries
- **iCal.NET**: For parsing iCal files
- **System.Data.SQLite**: Database connectivity
- **Newtonsoft.Json**: For API integration

### Database Changes
The schema already includes:
- `teamup_import_id` field for tracking imported events
- `created_by` field to mark imported events
- Audit trail fields for tracking changes

## Testing Strategy

### Test Data Preparation
1. **Export sample data from TeamUp**
2. **Create test scenarios**:
   - Various event types
   - Recurring events
   - Multi-day events
   - Events with special characters
   - Overlapping events

### Validation Tests
1. **Data Integrity**: All imported events match source
2. **Mapping Accuracy**: House and type assignments are correct
3. **Date Handling**: Time zones and recurring events work properly
4. **Conflict Resolution**: Duplicate events are handled appropriately

## Migration Timeline

### Week 1: Setup and Analysis
- Export current TeamUp data
- Analyze data structure and patterns
- Create mapping tables

### Week 2: Import Development
- Build iCal parser
- Create mapping logic
- Develop import UI

### Week 3: Testing and Validation
- Test with real TeamUp data
- Validate mappings and rules
- Fix issues and edge cases

### Week 4: Production Import
- Perform actual data migration
- Verify all events imported correctly
- Document any manual adjustments needed

## Future Enhancements

### Real-time Synchronization
- TeamUp API integration
- Webhook support for instant updates
- Conflict resolution for simultaneous edits

### Export Back to TeamUp
- Allow exporting events from our system
- Maintain bi-directional sync
- Support for calendar sharing with external users
