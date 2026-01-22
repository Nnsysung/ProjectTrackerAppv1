using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectTrackerApp.Data
{

    public class ApplicationUser : IdentityUser
    {

        public string? FullName { get; set; }

        public DateTime? DateRegistered { get; set; }

        public DateTime? LastLogin { get; set; }

        public bool ForcePasswordChange { get; set; } = false;

        public string? AvatarUrl { get; set; }

        public bool? Suspended { get; set; } = false;

        public string? ConfirmCode { get; set; }

        public string? MapCode { get; set; }

        public bool LockedOut { get; set; } = false;
        public bool Deleted { get; set; } = false;
        public DateTime? DateDeleted { get; set; }


        [NotMapped]
        public string FirstName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.FullName))
                    return "";

                var parts = this.FullName.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                return parts.Length > 0 ? parts[0] : "";
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    return;

                var parts = this.FullName?.Split(" ", StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
                if (parts.Length > 1)
                {
                    this.FullName = value + " " + string.Join(" ", parts.Skip(1));
                }
                else
                {
                    this.FullName = value;
                }
            }
        }





        [NotMapped]
        public string LastName
        {
            get
            {
                if (string.IsNullOrEmpty(this.FullName))
                    return "";

                var split = this.FullName.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                return split.Length > 1 ? split[^1] : "";
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    return;

                if (string.IsNullOrEmpty(this.FullName))
                {
                    this.FullName = value;
                }
                else
                {
                    var parts = this.FullName.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 1)
                    {
                        this.FullName = string.Join(" ", parts.Take(parts.Length - 1)) + " " + value;
                    }
                    else
                    {
                        this.FullName = value;
                    }
                }
            }
        }

    }
}
