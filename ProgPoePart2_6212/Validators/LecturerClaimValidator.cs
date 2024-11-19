using FluentValidation;
using ProgPoePart2_6212.Models;


namespace ProgPoePart2_6212.Validators
{
 
        public class LecturerClaimValidator : AbstractValidator<LecturerClaim>
        {
            public LecturerClaimValidator()
            {
                // Validate HoursWorked
                RuleFor(claim => claim.HoursWorked)
                    .GreaterThan(0).WithMessage("Hours worked must be greater than 0.")
                    .LessThanOrEqualTo(200).WithMessage("Hours worked must be less than or equal to 200.");

                // Validate HourlyRate
                RuleFor(claim => claim.HourlyRate)
                    .GreaterThan(0).WithMessage("Hourly rate must be greater than 0.")
                    .LessThanOrEqualTo(500).WithMessage("Hourly rate must be less than or equal to 500.");

                //// Validate TotalAmount
                //RuleFor(claim => claim.TotalAmount)
                //    .Equal(claim => claim.HoursWorked * claim.HourlyRate)
                //    .WithMessage("Total amount must match the calculated hours worked * hourly rate.");
            }
        }
    }