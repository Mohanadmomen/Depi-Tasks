using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Migrate database
            await context.Database.MigrateAsync();

            // Seed Roles
            string[] roleNames = { "Admin", "Customer" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Seed Admin User
            string adminEmail = "admin@cinema.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(adminUser, "AdminPassword123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Seed Categories if empty
            if (!await context.Categories.AnyAsync())
            {
                var categories = new List<Category>
                {
                    new() { Name = "Action" },
                    new() { Name = "Comedy" },
                    new() { Name = "Drama" },
                    new() { Name = "Sci-Fi" },
                    new() { Name = "Horror" }
                };
                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
            }

            // Seed Cinemas and Halls if empty
            if (!await context.Cinemas.AnyAsync())
            {
                var cinemas = new List<Cinema>
                {
                    new()
                    {
                        Name = "Grand Plaza Cinema",
                        Location = "123 Main Street, Downtown",
                        Halls = new List<Hall>
                        {
                            new() { Name = "IMAX Hall", Capacity = 50 },
                            new() { Name = "Hall B (Standard)", Capacity = 30 }
                        }
                    },
                    new()
                    {
                        Name = "Metro Cinema",
                        Location = "456 Broadway Ave, Uptown",
                        Halls = new List<Hall>
                        {
                            new() { Name = "VIP Lounge", Capacity = 15 },
                            new() { Name = "Hall 1 (Standard)", Capacity = 40 }
                        }
                    }
                };
                await context.Cinemas.AddRangeAsync(cinemas);
                await context.SaveChangesAsync();
            }

            // Seed Movies if empty
            if (!await context.Movies.AnyAsync())
            {
                var actionCat = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Action");
                var scifiCat = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Sci-Fi");
                var dramaCat = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Drama");

                var movies = new List<Movie>
                {
                    new()
                    {
                        Title = "The Dark Knight",
                        Description = "When the menace known as the Joker wreaks havoc and chaos on the people of Gotham, Batman must accept one of the greatest psychological and physical tests of his ability to fight injustice.",
                        DurationMinutes = 152,
                        ReleaseDate = new DateTime(2008, 7, 18),
                        CategoryId = actionCat?.Id ?? 1,
                        PosterUrl = "/images/posters/dark_knight.jpg"
                    },
                    new()
                    {
                        Title = "Inception",
                        Description = "A thief who steals corporate secrets through the use of dream-sharing technology is given the inverse task of planting an idea into the mind of a C.E.O., but his tragic past may doom the project.",
                        DurationMinutes = 148,
                        ReleaseDate = new DateTime(2010, 7, 16),
                        CategoryId = scifiCat?.Id ?? 1,
                        PosterUrl = "/images/posters/inception.jpg"
                    },
                    new()
                    {
                        Title = "Interstellar",
                        Description = "When Earth becomes uninhabitable, a team of explorers travels through a wormhole in space in an attempt to ensure humanity's survival.",
                        DurationMinutes = 169,
                        ReleaseDate = new DateTime(2014, 11, 7),
                        CategoryId = scifiCat?.Id ?? 1,
                        PosterUrl = "/images/posters/interstellar.jpg"
                    }
                };
                await context.Movies.AddRangeAsync(movies);
                await context.SaveChangesAsync();
            }

            // Seed Showtimes if empty
            if (!await context.Showtimes.AnyAsync())
            {
                var movies = await context.Movies.ToListAsync();
                var halls = await context.Halls.ToListAsync();

                if (movies.Any() && halls.Any())
                {
                    var showtimes = new List<Showtime>
                    {
                        new()
                        {
                            MovieId = movies[0].Id,
                            HallId = halls[0].Id, // Grand Plaza Cinema - IMAX Hall (50 capacity)
                            StartTime = DateTime.Now.AddDays(1).Date.AddHours(14), // Tomorrow at 2:00 PM
                            Price = 15.00m
                        },
                        new()
                        {
                            MovieId = movies[0].Id,
                            HallId = halls[1].Id, // Grand Plaza Cinema - Hall B (30 capacity)
                            StartTime = DateTime.Now.AddDays(1).Date.AddHours(19), // Tomorrow at 7:00 PM
                            Price = 12.00m
                        },
                        new()
                        {
                            MovieId = movies[1].Id,
                            HallId = halls[0].Id, // IMAX Hall
                            StartTime = DateTime.Now.AddDays(2).Date.AddHours(18), // Day after tomorrow at 6:00 PM
                            Price = 18.00m
                        },
                        new()
                        {
                            MovieId = movies[2].Id,
                            HallId = halls[2].Id, // Metro Cinema - VIP Lounge (15 capacity)
                            StartTime = DateTime.Now.AddDays(3).Date.AddHours(20), // In 3 days at 8:00 PM
                            Price = 25.00m
                        }
                    };
                    await context.Showtimes.AddRangeAsync(showtimes);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
