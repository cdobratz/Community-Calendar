using System;
using System.Data.SQLite;
using System.IO;

namespace OrganizationCalendar.Data
{
    public class DatabaseConnection : IDisposable
    {
        private readonly string _connectionString;
        private SQLiteConnection? _connection;
        private bool _disposed = false;

        public DatabaseConnection(string? databasePath = null)
        {
            // Default to local database file in application directory
            if (string.IsNullOrEmpty(databasePath))
            {
                var appDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? "";
                databasePath = Path.Combine(appDirectory, "organization_calendar.db");
            }

            _connectionString = $"Data Source={databasePath};Version=3;";
        }

        public SQLiteConnection Connection
        {
            get
            {
                if (_connection == null)
                {
                    _connection = new SQLiteConnection(_connectionString);
                    _connection.Open();
                }
                else if (_connection.State != System.Data.ConnectionState.Open)
                {
                    _connection.Open();
                }
                return _connection;
            }
        }

        public void InitializeDatabase()
        {
            try
            {
                var schemaPath = Path.Combine(
                    Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? "",
                    "..", "..", "..", "..", "database_schema.sql");

                if (File.Exists(schemaPath))
                {
                    var schema = File.ReadAllText(schemaPath);
                    ExecuteNonQuery(schema);
                }
                else
                {
                    // Fallback: Create basic schema if file not found
                    CreateBasicSchema();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to initialize database: {ex.Message}", ex);
            }
        }

        private void CreateBasicSchema()
        {
            var schema = @"
                -- Houses table
                CREATE TABLE IF NOT EXISTS Houses (
                    house_id VARCHAR(2) PRIMARY KEY,
                    house_name VARCHAR(50) NOT NULL,
                    capacity INTEGER DEFAULT 10,
                    current_occupancy INTEGER DEFAULT 0,
                    house_parent VARCHAR(100),
                    created_date DATETIME DEFAULT CURRENT_TIMESTAMP
                );

                -- Users table
                CREATE TABLE IF NOT EXISTS Users (
                    user_id VARCHAR(50) PRIMARY KEY,
                    username VARCHAR(50) UNIQUE NOT NULL,
                    password_hash VARCHAR(255),
                    role VARCHAR(20) NOT NULL,
                    house_assignment VARCHAR(2),
                    full_name VARCHAR(100),
                    email VARCHAR(100),
                    created_date DATETIME DEFAULT CURRENT_TIMESTAMP,
                    last_login DATETIME,
                    is_active BOOLEAN DEFAULT TRUE,
                    FOREIGN KEY (house_assignment) REFERENCES Houses(house_id)
                );

                -- Events table
                CREATE TABLE IF NOT EXISTS Events (
                    event_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    title VARCHAR(100) NOT NULL,
                    description TEXT,
                    event_type VARCHAR(25) NOT NULL,
                    category VARCHAR(20) NOT NULL,
                    start_datetime DATETIME NOT NULL,
                    end_datetime DATETIME,
                    house_id VARCHAR(2),
                    location VARCHAR(100),
                    created_by VARCHAR(50) NOT NULL,
                    created_date DATETIME DEFAULT CURRENT_TIMESTAMP,
                    modified_by VARCHAR(50),
                    modified_date DATETIME,
                    is_recurring BOOLEAN DEFAULT FALSE,
                    recurring_pattern VARCHAR(50),
                    recurring_end_date DATE,
                    parent_event_id INTEGER,
                    is_cancelled BOOLEAN DEFAULT FALSE,
                    cancel_reason TEXT,
                    teamup_import_id VARCHAR(50),
                    FOREIGN KEY (house_id) REFERENCES Houses(house_id),
                    FOREIGN KEY (created_by) REFERENCES Users(user_id),
                    FOREIGN KEY (modified_by) REFERENCES Users(user_id),
                    FOREIGN KEY (parent_event_id) REFERENCES Events(event_id)
                );

                -- Insert default houses
                INSERT OR IGNORE INTO Houses (house_id, house_name, capacity) VALUES
                ('AV', 'Ashford Village', 10),
                ('SP', 'Spring Place', 10),
                ('NL', 'North Lodge', 10),
                ('LC', 'Liberty Court', 10),
                ('WH', 'Westwood House', 10),
                ('HH', 'Heritage House', 10),
                ('BP', 'Brookside Place', 10);

                -- Insert default admin user
                INSERT OR IGNORE INTO Users (user_id, username, role, full_name) VALUES
                ('admin', 'admin', 'Admin', 'System Administrator');

                -- Create indexes
                CREATE INDEX IF NOT EXISTS idx_events_date_range ON Events(start_datetime, end_datetime);
                CREATE INDEX IF NOT EXISTS idx_events_house ON Events(house_id);
                CREATE INDEX IF NOT EXISTS idx_events_type ON Events(event_type);";

            ExecuteNonQuery(schema);
        }

        public int ExecuteNonQuery(string sql, params SQLiteParameter[] parameters)
        {
            using var command = new SQLiteCommand(sql, Connection);
            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }
            return command.ExecuteNonQuery();
        }

        public SQLiteDataReader ExecuteReader(string sql, params SQLiteParameter[] parameters)
        {
            var command = new SQLiteCommand(sql, Connection);
            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }
            return command.ExecuteReader();
        }

        public object? ExecuteScalar(string sql, params SQLiteParameter[] parameters)
        {
            using var command = new SQLiteCommand(sql, Connection);
            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }
            return command.ExecuteScalar();
        }

        public SQLiteTransaction BeginTransaction()
        {
            return Connection.BeginTransaction();
        }

        public void TestConnection()
        {
            try
            {
                var result = ExecuteScalar("SELECT 1");
                if (result?.ToString() != "1")
                {
                    throw new InvalidOperationException("Database connection test failed");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Database connection failed: {ex.Message}", ex);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _connection?.Close();
                    _connection?.Dispose();
                }
                _disposed = true;
            }
        }

        ~DatabaseConnection()
        {
            Dispose(false);
        }
    }
}
