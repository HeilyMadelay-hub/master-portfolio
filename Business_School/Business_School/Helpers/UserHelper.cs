using Business_School.Data;
using Business_School.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Business_School.Helpers
{

    public static class UserHelper
    {

        //Gets the UserId(NameIdentifier claim) from the authenticated user

        public static string? GetUserId(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(ClaimTypes.NameIdentifier);
        }


        //Get the actual user as student, including theirs clubs, departaments and events
        public static async Task<ApplicationUser?> GetCurrentStudentAsync(
           this ClaimsPrincipal user,
           ApplicationDbContext context)
        {
            var userIdStr = user.GetUserId();
            if (userIdStr == null) return null;

            if (!int.TryParse(userIdStr, out int userId)) return null;

            return await context.Users
                .Include(u => u.ClubMemberships)          // Relationship with StudentClub
                    .ThenInclude(sc => sc.Club)           // Load from Clubs
                        .ThenInclude(c => c.Department)   // Load Department from Club
                .Include(u => u.ClubMemberships)
                    .ThenInclude(sc => sc.Club)
                        .ThenInclude(c => c.EventClubs)   // Events of the club
                            .ThenInclude(ec => ec.Event)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId); // Looking by id
        }


        public static bool IsStudent(this ClaimsPrincipal user)
        {
            return user.IsInRole(RoleHelper.Student);
        }

        public static bool IsAdmin(this ClaimsPrincipal user)
        {
            return user.IsInRole(RoleHelper.Admin);
        }

        public static bool IsClubLeader(this ClaimsPrincipal user)
        {
            return user.IsInRole(RoleHelper.ClubLeader);
        }

        
        public static bool IsDepartmentManager(this ClaimsPrincipal user)
        {
            return user.IsInRole(RoleHelper.DepartmentManager);
        }


    }
}
