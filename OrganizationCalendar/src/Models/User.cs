using System;
using System.ComponentModel;

namespace OrganizationCalendar.Models
{
    public enum UserRole
    {
        [Description("Administrator")]
        Admin,
        [Description("Community Manager")]
        CM,
        [Description("House Parent")]
        HP,
        [Description("Staff")]
        Staff
    }

    public class User
    {
        public string UserId { get; set; } = string.Empty;
        
        public string Username { get; set; } = string.Empty;
        
        public string? PasswordHash { get; set; }
        
        public UserRole Role { get; set; }
        
        public string? HouseAssignment { get; set; }
        
        public string FullName { get; set; } = string.Empty;
        
        public string? Email { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public DateTime? LastLogin { get; set; }
        
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public string? HouseAssignmentName { get; set; }

        // Computed properties
        public string RoleDescription
        {
            get
            {
                return Role switch
                {
                    UserRole.Admin => "Administrator",
                    UserRole.CM => "Community Manager",
                    UserRole.HP => "House Parent",
                    UserRole.Staff => "Staff",
                    _ => "Unknown"
                };
            }
        }

        public string DisplayName => !string.IsNullOrEmpty(FullName) ? FullName : Username;

        public bool IsHouseParent => Role == UserRole.HP;
        
        public bool IsAdministrator => Role == UserRole.Admin;
        
        public bool IsCommunityManager => Role == UserRole.CM;

        public bool CanManageAllHouses => Role == UserRole.Admin || Role == UserRole.CM;
        
        public bool CanManageHouse(string houseId)
        {
            if (CanManageAllHouses) return true;
            if (Role == UserRole.HP && HouseAssignment == houseId) return true;
            return false;
        }

        public string AssignmentDisplay
        {
            get
            {
                if (string.IsNullOrEmpty(HouseAssignment))
                {
                    return "All Houses";
                }
                return HouseAssignmentName ?? HouseAssignment;
            }
        }

        public bool HasRecentActivity
        {
            get
            {
                if (!LastLogin.HasValue) return false;
                return LastLogin.Value > DateTime.Now.AddDays(-7);
            }
        }

        // Methods
        public bool CanEditEvent(Event eventItem)
        {
            if (!IsActive) return false;
            if (IsAdministrator || IsCommunityManager) return true;
            if (eventItem.CreatedBy == UserId) return true;
            if (IsHouseParent && eventItem.HouseId == HouseAssignment) return true;
            return false;
        }

        public bool CanViewEvent(Event eventItem)
        {
            if (!IsActive) return false;
            // All users can view all events for now
            return true;
        }

        public void UpdateLastLogin()
        {
            LastLogin = DateTime.Now;
        }

        public override string ToString()
        {
            return $"{DisplayName} ({RoleDescription})";
        }

        public override bool Equals(object? obj)
        {
            if (obj is User other)
            {
                return UserId == other.UserId;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return UserId?.GetHashCode() ?? 0;
        }
    }
}
