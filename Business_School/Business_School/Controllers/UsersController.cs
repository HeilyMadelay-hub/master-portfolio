using Business_School.Data;
using Business_School.Helpers;
using Business_School.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Business_School.Controllers
{
    [Authorize(Roles = RoleHelper.Admin + "," + RoleHelper.DepartmentManager + "," + RoleHelper.ClubLeader)]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _db;
        public UsersController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id, int? returnDepartmentId)
        {
            if (id == null) return NotFound();

            var user = await _db.Users
            .Include(u => u.Department)
            .Include(u => u.ClubMemberships)
            .ThenInclude(sc => sc.Club)
            .FirstOrDefaultAsync(u => u.Id == id.Value);

            if (user == null) return NotFound();

            // Determine department to return to:
            //1. Explicit returnDepartmentId from link
            //2. Manager's assigned department (Department where this user is ManagerUser)
            //3. User.DepartmentId (if set)
            //4. First club's department (fallback)
            int? managerDept = await _db.Departments.Where(d => d.ManagerUserId == user.Id).Select(d => (int?)d.Id).FirstOrDefaultAsync();
            var fallbackClubDept = user.ClubMemberships.Select(sc => (int?)sc.Club.DepartmentId).FirstOrDefault();
            var deptToReturn = returnDepartmentId ?? managerDept ?? user.DepartmentId ?? fallbackClubDept;
            ViewData["ReturnDepartmentId"] = deptToReturn;

            return View(user);
        }
    }
}
