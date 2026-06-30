using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Movie
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Poster Image")]
        public string? PosterUrl { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        public Category? Category { get; set; }

        [Required(ErrorMessage = "Duration is required.")]
        [Range(1, 600, ErrorMessage = "Duration must be between 1 and 600 minutes.")]
        [Display(Name = "Duration (Minutes)")]
        public int DurationMinutes { get; set; }

        [Required(ErrorMessage = "Release Date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Release Date")]
        public DateTime ReleaseDate { get; set; }

        public List<Showtime> Showtimes { get; set; } = new();
    }
}
