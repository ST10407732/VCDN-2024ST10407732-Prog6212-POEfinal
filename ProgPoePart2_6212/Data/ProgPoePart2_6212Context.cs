using ProgPoePart2_6212.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using System.Reflection.Metadata;
// Update the namespace

namespace ProgPoePart2_6212.Data
{
    public class ProgPoePart2_6212Context : IdentityDbContext<User>
    {
        public ProgPoePart2_6212Context(DbContextOptions<ProgPoePart2_6212Context> options) : base(options)
        {
        }

        public DbSet<LecturerClaim> LecturerClaims { get; set; }
       public DbSet<SuppDocument> Documents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Specify the store type for HourlyRate
            modelBuilder.Entity<LecturerClaim>()
                .Property(c => c.HourlyRate)
                .HasColumnType("decimal(18,2)"); // Adjust precision and scale as needed
        }
    }
}