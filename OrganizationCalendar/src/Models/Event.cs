using System;
using System.ComponentModel;

namespace OrganizationCalendar.Models
{
    public enum EventType
    {
        [Description("Meal")]
        Meal,
        [Description("Campus Activity")]
        CampusActivity,
        [Description("Off Campus Activity")]
        OffCampusActivity,
        [Description("School Activity")]
        SchoolActivity,
        [Description("Medical Appointment")]
        MedicalAppointment,
        [Description("Job")]
        Job
    }

    public enum EventCategory
    {
        [Description("Mandatory")]
        Mandatory,
        [Description("Optional")]
        Optional,
        [Description("House")]
        House
    }

    public enum AttendanceStatus
    {
        [Description("Expected")]
        Expected,
        [Description("Present")]
        Present,
        [Description("Absent")]
        Absent,
        [Description("Excused")]
        Excused
    }

    public class Event
    {
        public int EventId { get; set; }
        
        public string Title { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        public EventType EventType { get; set; }
        
        public EventCategory Category { get; set; }
        
        public DateTime StartDateTime { get; set; }
        
        public DateTime? EndDateTime { get; set; }
        
        public string? HouseId { get; set; }
        
        public string Location { get; set; } = string.Empty;
        
        public string CreatedBy { get; set; } = string.Empty;
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public string? ModifiedBy { get; set; }
        
        public DateTime? ModifiedDate { get; set; }
        
        public bool IsRecurring { get; set; } = false;
        
        public string? RecurringPattern { get; set; }
        
        public DateTime? RecurringEndDate { get; set; }
        
        public int? ParentEventId { get; set; }
        
        public bool IsCancelled { get; set; } = false;
        
        public string? CancelReason { get; set; }
        
        public string? TeamUpImportId { get; set; }

        // Navigation properties (for display purposes)
        public string HouseName { get; set; } = string.Empty;
        
        public string CreatedByName { get; set; } = string.Empty;

        // Computed properties
        public TimeSpan Duration
        {
            get
            {
                if (EndDateTime.HasValue)
                {
                    return EndDateTime.Value - StartDateTime;
                }
                return TimeSpan.FromHours(1); // Default 1 hour
            }
        }

        public string DisplayTime
        {
            get
            {
                if (EndDateTime.HasValue)
                {
                    return $"{StartDateTime:HH:mm} - {EndDateTime.Value:HH:mm}";
                }
                return StartDateTime.ToString("HH:mm");
            }
        }

        public string DisplayDate
        {
            get
            {
                return StartDateTime.ToString("MMM dd, yyyy");
            }
        }

        public string DisplayDateTime
        {
            get
            {
                return StartDateTime.ToString("MMM dd, yyyy HH:mm");
            }
        }

        public bool IsAllDay
        {
            get
            {
                return StartDateTime.TimeOfDay == TimeSpan.Zero &&
                       (!EndDateTime.HasValue || EndDateTime.Value.TimeOfDay == TimeSpan.Zero);
            }
        }

        public bool IsMultiDay
        {
            get
            {
                return EndDateTime.HasValue && EndDateTime.Value.Date > StartDateTime.Date;
            }
        }

        public bool IsToday
        {
            get
            {
                return StartDateTime.Date == DateTime.Today;
            }
        }

        public bool IsUpcoming
        {
            get
            {
                return StartDateTime > DateTime.Now;
            }
        }

        public bool IsPast
        {
            get
            {
                var endTime = EndDateTime ?? StartDateTime.AddHours(1);
                return endTime < DateTime.Now;
            }
        }

        public bool IsCurrentlyActive
        {
            get
            {
                var now = DateTime.Now;
                var endTime = EndDateTime ?? StartDateTime.AddHours(1);
                return now >= StartDateTime && now <= endTime;
            }
        }

        // Methods
        public Event Clone()
        {
            return new Event
            {
                EventId = 0, // New event gets new ID
                Title = this.Title,
                Description = this.Description,
                EventType = this.EventType,
                Category = this.Category,
                StartDateTime = this.StartDateTime,
                EndDateTime = this.EndDateTime,
                HouseId = this.HouseId,
                Location = this.Location,
                CreatedBy = this.CreatedBy,
                IsRecurring = false, // Cloned events are not recurring by default
                RecurringPattern = null,
                RecurringEndDate = null,
                ParentEventId = this.IsRecurring ? this.EventId : this.ParentEventId
            };
        }

        public bool ConflictsWith(Event other)
        {
            if (other == null) return false;
            if (this.EventId == other.EventId) return false;
            if (this.IsCancelled || other.IsCancelled) return false;

            var thisEnd = this.EndDateTime ?? this.StartDateTime.AddHours(1);
            var otherEnd = other.EndDateTime ?? other.StartDateTime.AddHours(1);

            // Check for time overlap
            return this.StartDateTime < otherEnd && thisEnd > other.StartDateTime;
        }

        public override string ToString()
        {
            var houseInfo = !string.IsNullOrEmpty(HouseId) ? $"[{HouseId}] " : "[Community] ";
            return $"{houseInfo}{Title} - {DisplayDateTime}";
        }

        public override bool Equals(object? obj)
        {
            if (obj is Event other)
            {
                return EventId == other.EventId;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return EventId.GetHashCode();
        }
    }
}
