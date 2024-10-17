using ProgPoePart2_6212.Models;
using System.Reflection.Metadata;

namespace ProgPoePart2_6212.Models
{
    // Models/LecturerClaim.cs
    public class LecturerClaim
    {
        public int Id { get; set; }

        public User User { get; set; }  // Navigation property
        public required String Name { get; set; }
        public int HoursWorked { get; set; }
        public decimal HourlyRate { get; set; }
        public string Notes { get; set; }
        public ClaimStatus Status { get; set; } = ClaimStatus.PendingVerification; // Default to PendingVerification

        // Navigation property for associated documents
        public ICollection<SuppDocument> Documents { get; set; }
    }

    public enum ClaimStatus
    {
        PendingVerification, PendingApproval, Approved, Rejected             
    }
}