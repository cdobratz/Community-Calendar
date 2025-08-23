using System;
using System.Collections.Generic;
using OrganizationCalendar.Models;

namespace OrganizationCalendar.Data
{
    public static class DemoDataGenerator
    {
        public static void CreateDemoData(EventRepository eventRepository)
        {
            var demoEvents = GenerateDemoEvents();
            
            foreach (var evt in demoEvents)
            {
                try
                {
                    eventRepository.CreateEvent(evt);
                }
                catch (Exception ex)
                {
                    // Log error but continue with other events
                    System.Diagnostics.Debug.WriteLine($"Failed to create demo event '{evt.Title}': {ex.Message}");
                }
            }
        }

        private static List<Event> GenerateDemoEvents()
        {
            var events = new List<Event>();
            var random = new Random();
            var houses = HouseData.GetHouseIds();
            var today = DateTime.Today;

            // Generate events for the current month and next month
            var startDate = today.AddDays(-15);
            var endDate = today.AddDays(45);

            // Community-wide events
            events.AddRange(new[]
            {
                new Event
                {
                    Title = "Community Meeting",
                    Description = "Monthly community meeting for all houses",
                    EventType = EventType.CampusActivity,
                    Category = EventCategory.Mandatory,
                    StartDateTime = GetNextWeekday(today, DayOfWeek.Monday).AddHours(19),
                    EndDateTime = GetNextWeekday(today, DayOfWeek.Monday).AddHours(20, 30),
                    HouseId = null, // Community-wide
                    Location = "Main Hall",
                    CreatedBy = "admin"
                },
                new Event
                {
                    Title = "Fire Safety Drill",
                    Description = "Emergency evacuation drill for all residents",
                    EventType = EventType.CampusActivity,
                    Category = EventCategory.Mandatory,
                    StartDateTime = today.AddDays(7).AddHours(10),
                    EndDateTime = today.AddDays(7).AddHours(11),
                    HouseId = null,
                    Location = "All Buildings",
                    CreatedBy = "admin"
                },
                new Event
                {
                    Title = "Community BBQ",
                    Description = "End of month barbecue for all residents and staff",
                    EventType = EventType.CampusActivity,
                    Category = EventCategory.Optional,
                    StartDateTime = GetLastWeekdayOfMonth(today, DayOfWeek.Saturday).AddHours(17),
                    EndDateTime = GetLastWeekdayOfMonth(today, DayOfWeek.Saturday).AddHours(20),
                    HouseId = null,
                    Location = "Community Garden",
                    CreatedBy = "admin"
                }
            });

            // Daily meals (community-wide)
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                events.Add(new Event
                {
                    Title = "Breakfast",
                    Description = "Community breakfast",
                    EventType = EventType.Meal,
                    Category = EventCategory.Mandatory,
                    StartDateTime = date.AddHours(7),
                    EndDateTime = date.AddHours(8),
                    HouseId = null,
                    Location = "Dining Hall",
                    CreatedBy = "admin"
                });

                events.Add(new Event
                {
                    Title = "Lunch",
                    Description = "Community lunch",
                    EventType = EventType.Meal,
                    Category = EventCategory.Mandatory,
                    StartDateTime = date.AddHours(12),
                    EndDateTime = date.AddHours(13),
                    HouseId = null,
                    Location = "Dining Hall",
                    CreatedBy = "admin"
                });

                events.Add(new Event
                {
                    Title = "Dinner",
                    Description = "Community dinner",
                    EventType = EventType.Meal,
                    Category = EventCategory.Mandatory,
                    StartDateTime = date.AddHours(18),
                    EndDateTime = date.AddHours(19),
                    HouseId = null,
                    Location = "Dining Hall",
                    CreatedBy = "admin"
                });
            }

            // House-specific activities
            foreach (var houseId in houses)
            {
                var houseName = HouseData.GetHouseName(houseId);

                // Weekly house meetings
                var nextTuesday = GetNextWeekday(today, DayOfWeek.Tuesday);
                events.Add(new Event
                {
                    Title = $"{houseId} House Meeting",
                    Description = $"Weekly meeting for {houseName} residents",
                    EventType = EventType.CampusActivity,
                    Category = EventCategory.House,
                    StartDateTime = nextTuesday.AddHours(19),
                    EndDateTime = nextTuesday.AddHours(20),
                    HouseId = houseId,
                    Location = $"{houseName} Common Room",
                    CreatedBy = "admin"
                });

                // House cleaning
                var nextSaturday = GetNextWeekday(today, DayOfWeek.Saturday);
                events.Add(new Event
                {
                    Title = "House Cleaning",
                    Description = "Weekly house cleaning and maintenance",
                    EventType = EventType.CampusActivity,
                    Category = EventCategory.Mandatory,
                    StartDateTime = nextSaturday.AddHours(9),
                    EndDateTime = nextSaturday.AddHours(11),
                    HouseId = houseId,
                    Location = houseName,
                    CreatedBy = "admin"
                });

                // Random activities throughout the month
                for (int i = 0; i < 5; i++)
                {
                    var randomDate = startDate.AddDays(random.Next(0, (endDate - startDate).Days));
                    var activities = new[]
                    {
                        ("Movie Night", EventType.CampusActivity, EventCategory.Optional, "Common Room"),
                        ("Study Group", EventType.SchoolActivity, EventCategory.Optional, "Study Room"),
                        ("Game Night", EventType.CampusActivity, EventCategory.Optional, "Recreation Room"),
                        ("House Outing", EventType.OffCampusActivity, EventCategory.Optional, "Meet at Lobby"),
                        ("Life Skills Workshop", EventType.SchoolActivity, EventCategory.Mandatory, "Conference Room")
                    };

                    var activity = activities[random.Next(activities.Length)];
                    var startHour = random.Next(14, 20); // 2 PM to 8 PM

                    events.Add(new Event
                    {
                        Title = $"{activity.Item1}",
                        Description = $"{activity.Item1} for {houseName} residents",
                        EventType = activity.Item2,
                        Category = activity.Item3,
                        StartDateTime = randomDate.AddHours(startHour),
                        EndDateTime = randomDate.AddHours(startHour + random.Next(1, 3)),
                        HouseId = houseId,
                        Location = activity.Item4,
                        CreatedBy = "admin"
                    });
                }
            }

            // Medical appointments (various houses)
            for (int i = 0; i < 10; i++)
            {
                var randomDate = today.AddDays(random.Next(1, 30));
                var randomHouse = houses[random.Next(houses.Count)];
                var appointmentTypes = new[] { "Doctor Visit", "Dental Checkup", "Therapy Session", "Medication Review" };

                events.Add(new Event
                {
                    Title = appointmentTypes[random.Next(appointmentTypes.Length)],
                    Description = "Medical appointment",
                    EventType = EventType.MedicalAppointment,
                    Category = EventCategory.Mandatory,
                    StartDateTime = randomDate.AddHours(random.Next(9, 16)),
                    EndDateTime = randomDate.AddHours(random.Next(9, 16)).AddMinutes(30),
                    HouseId = randomHouse,
                    Location = "Medical Center",
                    CreatedBy = "admin"
                });
            }

            // Job activities
            for (int i = 0; i < 8; i++)
            {
                var randomDate = today.AddDays(random.Next(1, 30));
                var randomHouse = houses[random.Next(houses.Count)];
                var jobTypes = new[] { "Job Interview", "Work Training", "Career Counseling", "Resume Workshop" };

                events.Add(new Event
                {
                    Title = jobTypes[random.Next(jobTypes.Length)],
                    Description = "Employment-related activity",
                    EventType = EventType.Job,
                    Category = EventCategory.Mandatory,
                    StartDateTime = randomDate.AddHours(random.Next(9, 17)),
                    EndDateTime = randomDate.AddHours(random.Next(9, 17)).AddHours(1),
                    HouseId = randomHouse,
                    Location = "Career Center",
                    CreatedBy = "admin"
                });
            }

            return events;
        }

        private static DateTime GetNextWeekday(DateTime start, DayOfWeek dayOfWeek)
        {
            int daysUntilTarget = ((int)dayOfWeek - (int)start.DayOfWeek + 7) % 7;
            if (daysUntilTarget == 0) daysUntilTarget = 7; // Get next occurrence, not today
            return start.AddDays(daysUntilTarget);
        }

        private static DateTime GetLastWeekdayOfMonth(DateTime date, DayOfWeek dayOfWeek)
        {
            var lastDayOfMonth = new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
            var daysFromTarget = ((int)lastDayOfMonth.DayOfWeek - (int)dayOfWeek + 7) % 7;
            return lastDayOfMonth.AddDays(-daysFromTarget);
        }
    }
}
