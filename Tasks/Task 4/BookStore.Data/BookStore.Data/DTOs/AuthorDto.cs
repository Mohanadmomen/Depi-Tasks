using System.ComponentModel.DataAnnotations;

namespace BookStore.Data.DTOs;

public record AuthorDto(int Id, string Name);

public record CreateAuthorDto(
    [Required][MinLength(2)] string Name
);
