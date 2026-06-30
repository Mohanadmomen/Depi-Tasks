using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Hall
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Hall name is required.")]
        [StringLength(100, ErrorMessage = "Hall name cannot exceed 100 characters.")]
        [Display(Name = "Hall Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Seat capacity is required.")]
        [Range(1, 1000, ErrorMessage = "Seat capacity must be between 1 and 1000.")]
        [Display(Name = "Seat Capacity")]
        public int Capacity { get; set; }

        [Required(ErrorMessage = "Cinema is required.")]
        [Display(Name = "Cinema")]
        public int CinemaId { get; set; }

        public Cinema? Cinema { get; set; }

        public List<Showtime> Showtimes { get; set; } = new();
    }
}
