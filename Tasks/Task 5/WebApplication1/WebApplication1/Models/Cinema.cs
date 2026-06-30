using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Cinema
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Cinema name is required.")]
        [StringLength(150, ErrorMessage = "Cinema name cannot exceed 150 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Location is required.")]
        [StringLength(250, ErrorMessage = "Location cannot exceed 250 characters.")]
        public string Location { get; set; } = string.Empty;

        public List<Hall> Halls { get; set; } = new();
    }
}
