using System;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Booking
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Showtime is required.")]
        [Display(Name = "Showtime")]
        public int ShowtimeId { get; set; }

        public Showtime? Showtime { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public ApplicationUser? User { get; set; }

        [Required(ErrorMessage = "Number of seats is required.")]
        [Range(1, 10, ErrorMessage = "You can book between 1 and 10 seats per booking.")]
        [Display(Name = "Number of Seats")]
        public int SeatsBooked { get; set; }

        [Required]
        [Display(Name = "Booking Time")]
        public DateTime BookingTime { get; set; }

        [Required]
        [Display(Name = "Total Price")]
        [DataType(DataType.Currency)]
        public decimal TotalPrice { get; set; }

        [Display(Name = "Is Cancelled")]
        public bool IsCancelled { get; set; } = false;
    }
}
