using System;
using System.Collections.Generic;
using System.Linq;

namespace OrganizationCalendar.Models
{
    public class House
    {
        public string HouseId { get; set; } = string.Empty;
        
        public string HouseName { get; set; } = string.Empty;
        
        public int Capacity { get; set; } = 10;
        
        public int CurrentOccupancy { get; set; } = 0;
        
        public string? HouseParent { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Computed properties
        public bool IsAtCapacity => CurrentOccupancy >= Capacity;
        
        public int AvailableSpaces => Math.Max(0, Capacity - CurrentOccupancy);
        
        public double OccupancyPercentage => Capacity > 0 ? (double)CurrentOccupancy / Capacity * 100 : 0;

        // Display properties
        public string DisplayName => $"{HouseId} - {HouseName}";
        
        public string OccupancyDisplay => $"{CurrentOccupancy}/{Capacity}";

        // Methods
        public bool CanAcceptMoreChildren()
        {
            return CurrentOccupancy < Capacity;
        }

        public override string ToString()
        {
            return DisplayName;
        }

        public override bool Equals(object? obj)
        {
            if (obj is House other)
            {
                return HouseId == other.HouseId;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HouseId?.GetHashCode() ?? 0;
        }
    }

    // Static helper class for predefined houses
    public static class HouseData
    {
        public static readonly List<House> DefaultHouses = new List<House>
        {
            new House { HouseId = "AV", HouseName = "Ashford Village", Capacity = 10 },
            new House { HouseId = "SP", HouseName = "Spring Place", Capacity = 10 },
            new House { HouseId = "NL", HouseName = "North Lodge", Capacity = 10 },
            new House { HouseId = "LC", HouseName = "Liberty Court", Capacity = 10 },
            new House { HouseId = "WH", HouseName = "Westwood House", Capacity = 10 },
            new House { HouseId = "HH", HouseName = "Heritage House", Capacity = 10 },
            new House { HouseId = "BP", HouseName = "Brookside Place", Capacity = 10 }
        };

        public static House? GetHouseById(string houseId)
        {
            return DefaultHouses.FirstOrDefault(h => h.HouseId == houseId);
        }

        public static string GetHouseName(string houseId)
        {
            var house = GetHouseById(houseId);
            return house?.HouseName ?? "Unknown House";
        }

        public static List<string> GetHouseIds()
        {
            return DefaultHouses.Select(h => h.HouseId).ToList();
        }

        public static List<string> GetHouseDisplayNames()
        {
            return DefaultHouses.Select(h => h.DisplayName).ToList();
        }
    }
}
