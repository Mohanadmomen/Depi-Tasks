using System.ComponentModel.DataAnnotations;

namespace BookStore.Data.DTOs;

public record CategoryDto(int Id, string Name);

public record CreateCategoryDto(
    [Required][MinLength(2)] string Name
);
