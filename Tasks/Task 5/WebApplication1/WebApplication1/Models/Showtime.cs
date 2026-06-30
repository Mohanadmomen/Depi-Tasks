using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Showtime
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Movie is required.")]
        [Display(Name = "Movie")]
        public int MovieId { get; set; }

        public Movie? Movie { get; set; }

        [Required(ErrorMessage = "Hall is required.")]
        [Display(Name = "Hall")]
        public int HallId { get; set; }

        public Hall? Hall { get; set; }

        [Required(ErrorMessage = "Start Time is required.")]
        [Display(Name = "Start Time")]
        [DataType(DataType.DateTime)]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, 1000.00, ErrorMessage = "Price must be between 0.01 and 1000.00.")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        public List<Booking> Bookings { get; set; } = new();
    }
}
