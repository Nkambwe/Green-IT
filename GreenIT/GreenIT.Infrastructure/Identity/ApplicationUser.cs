using GreenIT.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace GreenIT.Infrastructure.Identity {
    public class ApplicationUser : IdentityUser {
        public string FullName { get; set; } = string.Empty;

        public int? DepartmentId { get; set; }

        public Department? Department { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
