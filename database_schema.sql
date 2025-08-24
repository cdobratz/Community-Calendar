-- Organization Calendar Management System Database Schema
-- SQLite Implementation

-- Houses table - stores information about each residential house
CREATE TABLE IF NOT EXISTS Houses (
    house_id VARCHAR(2) PRIMARY KEY,  -- AV, SP, NL, LC, WH, HH, BP
    house_name VARCHAR(50) NOT NULL,
    capacity INTEGER DEFAULT 10,
    current_occupancy INTEGER DEFAULT 0,
    house_parent VARCHAR(100),
    created_date DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Users table - system users with roles
CREATE TABLE IF NOT EXISTS Users (
    user_id VARCHAR(50) PRIMARY KEY,
    username VARCHAR(50) UNIQUE NOT NULL,
    password_hash VARCHAR(255), -- For future authentication
    role VARCHAR(20) NOT NULL, -- Admin, CM, HP, Staff
    house_assignment VARCHAR(2), -- For House Parents
    full_name VARCHAR(100),
    email VARCHAR(100),
    created_date DATETIME DEFAULT CURRENT_TIMESTAMP,
    last_login DATETIME,
    is_active BOOLEAN DEFAULT TRUE,
    FOREIGN KEY (house_assignment) REFERENCES Houses(house_id)
);

-- Events table - main event storage
CREATE TABLE IF NOT EXISTS Events (
    event_id INTEGER PRIMARY KEY AUTOINCREMENT,
    title VARCHAR(100) NOT NULL,
    description TEXT,
    event_type VARCHAR(25) NOT NULL, -- Meal, Campus Activity, Off Campus Activity, School Activity, Medical Appointment, Job
    category VARCHAR(20) NOT NULL,   -- Mandatory, Optional, House
    start_datetime DATETIME NOT NULL,
    end_datetime DATETIME,
    house_id VARCHAR(2),    -- NULL for community-wide events
    location VARCHAR(100),
    created_by VARCHAR(50) NOT NULL,
    created_date DATETIME DEFAULT CURRENT_TIMESTAMP,
    modified_by VARCHAR(50),
    modified_date DATETIME,
    is_recurring BOOLEAN DEFAULT FALSE,
    recurring_pattern VARCHAR(50), -- JSON string for complex patterns
    recurring_end_date DATE,
    parent_event_id INTEGER, -- For recurring event instances
    is_cancelled BOOLEAN DEFAULT FALSE,
    cancel_reason TEXT,
    teamup_import_id VARCHAR(50), -- For TeamUp migration tracking
    FOREIGN KEY (house_id) REFERENCES Houses(house_id),
    FOREIGN KEY (created_by) REFERENCES Users(user_id),
    FOREIGN KEY (modified_by) REFERENCES Users(user_id),
    FOREIGN KEY (parent_event_id) REFERENCES Events(event_id)
);

-- Children table - for tracking house assignments
CREATE TABLE IF NOT EXISTS Children (
    child_id INTEGER PRIMARY KEY AUTOINCREMENT,
    name VARCHAR(100) NOT NULL,
    current_house VARCHAR(2) NOT NULL,
    previous_house VARCHAR(2),
    move_date DATE,
    move_reason TEXT,
    birth_date DATE,
    enrollment_date DATE,
    is_active BOOLEAN DEFAULT TRUE,
    notes TEXT,
    created_date DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (current_house) REFERENCES Houses(house_id),
    FOREIGN KEY (previous_house) REFERENCES Houses(house_id)
);

-- Event_Attendees table - for tracking who should attend events
CREATE TABLE IF NOT EXISTS Event_Attendees (
    attendance_id INTEGER PRIMARY KEY AUTOINCREMENT,
    event_id INTEGER NOT NULL,
    child_id INTEGER,
    user_id VARCHAR(50),
    attendance_status VARCHAR(20) DEFAULT 'Expected', -- Expected, Present, Absent, Excused
    notes TEXT,
    marked_by VARCHAR(50),
    marked_date DATETIME,
    FOREIGN KEY (event_id) REFERENCES Events(event_id) ON DELETE CASCADE,
    FOREIGN KEY (child_id) REFERENCES Children(child_id),
    FOREIGN KEY (user_id) REFERENCES Users(user_id),
    FOREIGN KEY (marked_by) REFERENCES Users(user_id)
);

-- System_Settings table - for application configuration
CREATE TABLE IF NOT EXISTS System_Settings (
    setting_key VARCHAR(50) PRIMARY KEY,
    setting_value TEXT,
    description TEXT,
    modified_by VARCHAR(50),
    modified_date DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Insert default houses
INSERT OR IGNORE INTO Houses (house_id, house_name, capacity) VALUES
('AV', 'Aloe Vera', 10),
('SP', 'Scotty''s Place', 10),
('NL', 'Neely', 10),
('LC', 'Loren''s Cottage', 10),
('WH', 'Whiteman', 10),
('HH', 'Hemmer House', 10),
('BP', 'Bonnie''s Place', 10);

-- Insert default admin user (password should be hashed in production)
INSERT OR IGNORE INTO Users (user_id, username, role, full_name) VALUES
('admin', 'admin', 'Admin', 'System Administrator');

-- Insert default system settings
INSERT OR IGNORE INTO System_Settings (setting_key, setting_value, description) VALUES
('calendar_start_hour', '6', 'Calendar display start hour (24h format)'),
('calendar_end_hour', '23', 'Calendar display end hour (24h format)'),
('default_event_duration', '60', 'Default event duration in minutes'),
('backup_enabled', 'true', 'Enable automatic database backups'),
('teamup_last_sync', '', 'Last successful TeamUp synchronization timestamp');

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS idx_events_date_range ON Events(start_datetime, end_datetime);
CREATE INDEX IF NOT EXISTS idx_events_house ON Events(house_id);
CREATE INDEX IF NOT EXISTS idx_events_type ON Events(event_type);
CREATE INDEX IF NOT EXISTS idx_events_recurring ON Events(is_recurring, parent_event_id);
CREATE INDEX IF NOT EXISTS idx_children_house ON Children(current_house);
CREATE INDEX IF NOT EXISTS idx_attendance_event ON Event_Attendees(event_id);

-- Views for common queries
CREATE VIEW IF NOT EXISTS EventsWithDetails AS
SELECT 
    e.event_id,
    e.title,
    e.description,
    e.event_type,
    e.category,
    e.start_datetime,
    e.end_datetime,
    e.house_id,
    h.house_name,
    e.location,
    e.created_by,
    u.full_name as created_by_name,
    e.is_recurring,
    e.is_cancelled
FROM Events e
LEFT JOIN Houses h ON e.house_id = h.house_id
LEFT JOIN Users u ON e.created_by = u.user_id
WHERE e.is_cancelled = FALSE;

CREATE VIEW IF NOT EXISTS HouseOccupancy AS
SELECT 
    h.house_id,
    h.house_name,
    h.capacity,
    COUNT(c.child_id) as current_occupancy,
    h.house_parent
FROM Houses h
LEFT JOIN Children c ON h.house_id = c.current_house AND c.is_active = TRUE
GROUP BY h.house_id, h.house_name, h.capacity, h.house_parent;
