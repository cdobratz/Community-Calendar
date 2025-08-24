using System;
using System.Collections.Generic;
using System.Data.SQLite;
using OrganizationCalendar.Models;

namespace OrganizationCalendar.Data
{
    public class EventRepository
    {
        private readonly DatabaseConnection _database;

        public EventRepository(DatabaseConnection database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
        }

        public List<Event> GetAllEvents()
        {
            var events = new List<Event>();
            var sql = @"
                SELECT 
                    e.event_id, e.title, e.description, e.event_type, e.category,
                    e.start_datetime, e.end_datetime, e.house_id, e.location,
                    e.created_by, e.created_date, e.modified_by, e.modified_date,
                    e.is_recurring, e.recurring_pattern, e.recurring_end_date,
                    e.parent_event_id, e.is_cancelled, e.cancel_reason, e.teamup_import_id,
                    h.house_name, u.full_name as created_by_name
                FROM Events e
                LEFT JOIN Houses h ON e.house_id = h.house_id
                LEFT JOIN Users u ON e.created_by = u.user_id
                WHERE e.is_cancelled = 0
                ORDER BY e.start_datetime";

            using var reader = _database.ExecuteReader(sql);
            while (reader.Read())
            {
                events.Add(MapEventFromReader(reader));
            }

            return events;
        }

        public List<Event> GetEventsByDateRange(DateTime startDate, DateTime endDate)
        {
            var events = new List<Event>();
            var sql = @"
                SELECT 
                    e.event_id, e.title, e.description, e.event_type, e.category,
                    e.start_datetime, e.end_datetime, e.house_id, e.location,
                    e.created_by, e.created_date, e.modified_by, e.modified_date,
                    e.is_recurring, e.recurring_pattern, e.recurring_end_date,
                    e.parent_event_id, e.is_cancelled, e.cancel_reason, e.teamup_import_id,
                    h.house_name, u.full_name as created_by_name
                FROM Events e
                LEFT JOIN Houses h ON e.house_id = h.house_id
                LEFT JOIN Users u ON e.created_by = u.user_id
                WHERE e.is_cancelled = 0 
                  AND e.start_datetime >= @startDate 
                  AND e.start_datetime < @endDate
                ORDER BY e.start_datetime";

            var parameters = new[]
            {
                new SQLiteParameter("@startDate", startDate),
                new SQLiteParameter("@endDate", endDate)
            };

            using var reader = _database.ExecuteReader(sql, parameters);
            while (reader.Read())
            {
                events.Add(MapEventFromReader(reader));
            }

            return events;
        }

        public List<Event> GetEventsByHouse(string houseId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var events = new List<Event>();
            var sql = @"
                SELECT 
                    e.event_id, e.title, e.description, e.event_type, e.category,
                    e.start_datetime, e.end_datetime, e.house_id, e.location,
                    e.created_by, e.created_date, e.modified_by, e.modified_date,
                    e.is_recurring, e.recurring_pattern, e.recurring_end_date,
                    e.parent_event_id, e.is_cancelled, e.cancel_reason, e.teamup_import_id,
                    h.house_name, u.full_name as created_by_name
                FROM Events e
                LEFT JOIN Houses h ON e.house_id = h.house_id
                LEFT JOIN Users u ON e.created_by = u.user_id
                WHERE e.is_cancelled = 0 
                  AND (e.house_id = @houseId OR e.house_id IS NULL)";

            var parameters = new List<SQLiteParameter>
            {
                new SQLiteParameter("@houseId", houseId)
            };

            if (startDate.HasValue)
            {
                sql += " AND e.start_datetime >= @startDate";
                parameters.Add(new SQLiteParameter("@startDate", startDate.Value));
            }

            if (endDate.HasValue)
            {
                sql += " AND e.start_datetime < @endDate";
                parameters.Add(new SQLiteParameter("@endDate", endDate.Value));
            }

            sql += " ORDER BY e.start_datetime";

            using var reader = _database.ExecuteReader(sql, parameters.ToArray());
            while (reader.Read())
            {
                events.Add(MapEventFromReader(reader));
            }

            return events;
        }

        public Event? GetEventById(int eventId)
        {
            var sql = @"
                SELECT 
                    e.event_id, e.title, e.description, e.event_type, e.category,
                    e.start_datetime, e.end_datetime, e.house_id, e.location,
                    e.created_by, e.created_date, e.modified_by, e.modified_date,
                    e.is_recurring, e.recurring_pattern, e.recurring_end_date,
                    e.parent_event_id, e.is_cancelled, e.cancel_reason, e.teamup_import_id,
                    h.house_name, u.full_name as created_by_name
                FROM Events e
                LEFT JOIN Houses h ON e.house_id = h.house_id
                LEFT JOIN Users u ON e.created_by = u.user_id
                WHERE e.event_id = @eventId";

            var parameters = new[] { new SQLiteParameter("@eventId", eventId) };

            using var reader = _database.ExecuteReader(sql, parameters);
            if (reader.Read())
            {
                return MapEventFromReader(reader);
            }

            return null;
        }

        public int CreateEvent(Event eventItem)
        {
            var sql = @"
                INSERT INTO Events (
                    title, description, event_type, category, start_datetime, end_datetime,
                    house_id, location, created_by, is_recurring, recurring_pattern,
                    recurring_end_date, parent_event_id, teamup_import_id
                ) VALUES (
                    @title, @description, @eventType, @category, @startDateTime, @endDateTime,
                    @houseId, @location, @createdBy, @isRecurring, @recurringPattern,
                    @recurringEndDate, @parentEventId, @teamupImportId
                );
                SELECT last_insert_rowid();";

            var parameters = new[]
            {
                new SQLiteParameter("@title", eventItem.Title),
                new SQLiteParameter("@description", eventItem.Description),
                new SQLiteParameter("@eventType", eventItem.EventType.ToString()),
                new SQLiteParameter("@category", eventItem.Category.ToString()),
                new SQLiteParameter("@startDateTime", eventItem.StartDateTime),
                new SQLiteParameter("@endDateTime", (object?)eventItem.EndDateTime ?? DBNull.Value),
                new SQLiteParameter("@houseId", (object?)eventItem.HouseId ?? DBNull.Value),
                new SQLiteParameter("@location", eventItem.Location),
                new SQLiteParameter("@createdBy", eventItem.CreatedBy),
                new SQLiteParameter("@isRecurring", eventItem.IsRecurring),
                new SQLiteParameter("@recurringPattern", (object?)eventItem.RecurringPattern ?? DBNull.Value),
                new SQLiteParameter("@recurringEndDate", (object?)eventItem.RecurringEndDate ?? DBNull.Value),
                new SQLiteParameter("@parentEventId", (object?)eventItem.ParentEventId ?? DBNull.Value),
                new SQLiteParameter("@teamupImportId", (object?)eventItem.TeamUpImportId ?? DBNull.Value)
            };

            var result = _database.ExecuteScalar(sql, parameters);
            return Convert.ToInt32(result);
        }

        public bool UpdateEvent(Event eventItem)
        {
            var sql = @"
                UPDATE Events SET
                    title = @title,
                    description = @description,
                    event_type = @eventType,
                    category = @category,
                    start_datetime = @startDateTime,
                    end_datetime = @endDateTime,
                    house_id = @houseId,
                    location = @location,
                    modified_by = @modifiedBy,
                    modified_date = @modifiedDate,
                    is_recurring = @isRecurring,
                    recurring_pattern = @recurringPattern,
                    recurring_end_date = @recurringEndDate
                WHERE event_id = @eventId";

            var parameters = new[]
            {
                new SQLiteParameter("@title", eventItem.Title),
                new SQLiteParameter("@description", eventItem.Description),
                new SQLiteParameter("@eventType", eventItem.EventType.ToString()),
                new SQLiteParameter("@category", eventItem.Category.ToString()),
                new SQLiteParameter("@startDateTime", eventItem.StartDateTime),
                new SQLiteParameter("@endDateTime", (object?)eventItem.EndDateTime ?? DBNull.Value),
                new SQLiteParameter("@houseId", (object?)eventItem.HouseId ?? DBNull.Value),
                new SQLiteParameter("@location", eventItem.Location),
                new SQLiteParameter("@modifiedBy", (object?)eventItem.ModifiedBy ?? DBNull.Value),
                new SQLiteParameter("@modifiedDate", DateTime.Now),
                new SQLiteParameter("@isRecurring", eventItem.IsRecurring),
                new SQLiteParameter("@recurringPattern", (object?)eventItem.RecurringPattern ?? DBNull.Value),
                new SQLiteParameter("@recurringEndDate", (object?)eventItem.RecurringEndDate ?? DBNull.Value),
                new SQLiteParameter("@eventId", eventItem.EventId)
            };

            var rowsAffected = _database.ExecuteNonQuery(sql, parameters);
            return rowsAffected > 0;
        }

        public bool DeleteEvent(int eventId)
        {
            var sql = "DELETE FROM Events WHERE event_id = @eventId";
            var parameters = new[] { new SQLiteParameter("@eventId", eventId) };

            var rowsAffected = _database.ExecuteNonQuery(sql, parameters);
            return rowsAffected > 0;
        }

        public bool CancelEvent(int eventId, string reason, string cancelledBy)
        {
            var sql = @"
                UPDATE Events SET
                    is_cancelled = 1,
                    cancel_reason = @reason,
                    modified_by = @cancelledBy,
                    modified_date = @modifiedDate
                WHERE event_id = @eventId";

            var parameters = new[]
            {
                new SQLiteParameter("@reason", reason),
                new SQLiteParameter("@cancelledBy", cancelledBy),
                new SQLiteParameter("@modifiedDate", DateTime.Now),
                new SQLiteParameter("@eventId", eventId)
            };

            var rowsAffected = _database.ExecuteNonQuery(sql, parameters);
            return rowsAffected > 0;
        }

        public List<Event> GetConflictingEvents(Event eventItem)
        {
            var events = new List<Event>();
            var endDateTime = eventItem.EndDateTime ?? eventItem.StartDateTime.AddHours(1);

            var sql = @"
                SELECT 
                    e.event_id, e.title, e.description, e.event_type, e.category,
                    e.start_datetime, e.end_datetime, e.house_id, e.location,
                    e.created_by, e.created_date, e.modified_by, e.modified_date,
                    e.is_recurring, e.recurring_pattern, e.recurring_end_date,
                    e.parent_event_id, e.is_cancelled, e.cancel_reason, e.teamup_import_id,
                    h.house_name, u.full_name as created_by_name
                FROM Events e
                LEFT JOIN Houses h ON e.house_id = h.house_id
                LEFT JOIN Users u ON e.created_by = u.user_id
                WHERE e.is_cancelled = 0 
                  AND e.event_id != @eventId
                  AND (e.house_id = @houseId OR e.house_id IS NULL OR @houseId IS NULL)
                  AND e.start_datetime < @endDateTime
                  AND COALESCE(e.end_datetime, datetime(e.start_datetime, '+1 hour')) > @startDateTime";

            var parameters = new[]
            {
                new SQLiteParameter("@eventId", eventItem.EventId),
                new SQLiteParameter("@houseId", (object?)eventItem.HouseId ?? DBNull.Value),
                new SQLiteParameter("@startDateTime", eventItem.StartDateTime),
                new SQLiteParameter("@endDateTime", endDateTime)
            };

            using var reader = _database.ExecuteReader(sql, parameters);
            while (reader.Read())
            {
                events.Add(MapEventFromReader(reader));
            }

            return events;
        }

        private static Event MapEventFromReader(SQLiteDataReader reader)
        {
            return new Event
            {
                EventId = reader.GetInt32(0), // event_id
                Title = reader.GetString(1), // title
                Description = reader.IsDBNull(2) ? "" : reader.GetString(2), // description
                EventType = Enum.Parse<EventType>(reader.GetString(3)), // event_type
                Category = Enum.Parse<EventCategory>(reader.GetString(4)), // category
                StartDateTime = reader.GetDateTime(5), // start_datetime
                EndDateTime = reader.IsDBNull(6) ? null : reader.GetDateTime(6), // end_datetime
                HouseId = reader.IsDBNull(7) ? null : reader.GetString(7), // house_id
                Location = reader.IsDBNull(8) ? "" : reader.GetString(8), // location
                CreatedBy = reader.GetString(9), // created_by
                CreatedDate = reader.GetDateTime(10), // created_date
                ModifiedBy = reader.IsDBNull(11) ? null : reader.GetString(11), // modified_by
                ModifiedDate = reader.IsDBNull(12) ? null : reader.GetDateTime(12), // modified_date
                IsRecurring = reader.GetBoolean(13), // is_recurring
                RecurringPattern = reader.IsDBNull(14) ? null : reader.GetString(14), // recurring_pattern
                RecurringEndDate = reader.IsDBNull(15) ? null : reader.GetDateTime(15), // recurring_end_date
                ParentEventId = reader.IsDBNull(16) ? null : reader.GetInt32(16), // parent_event_id
                IsCancelled = reader.GetBoolean(17), // is_cancelled
                CancelReason = reader.IsDBNull(18) ? null : reader.GetString(18), // cancel_reason
                TeamUpImportId = reader.IsDBNull(19) ? null : reader.GetString(19), // teamup_import_id
                HouseName = reader.IsDBNull(20) ? "" : reader.GetString(20), // house_name
                CreatedByName = reader.IsDBNull(21) ? "" : reader.GetString(21) // created_by_name
            };
        }
    }
}
