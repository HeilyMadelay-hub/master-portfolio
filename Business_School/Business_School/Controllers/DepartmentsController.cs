using Business_School.Data;
using Business_School.Models;
using Business_School.Models.JoinTables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;
using Business_School.ViewModels;

namespace Business_School.Controllers
{
    [Authorize(Roles= "Admin,DepartmentManager ")]
    public class DepartmentsController : Controller
    {


        private readonly ApplicationDbContext _db;

        public DepartmentsController(ApplicationDbContext db){

            _db = db;

        }

        private string? NormalizeReturnUrl(string? returnUrl)
        {
            if (string.IsNullOrWhiteSpace(returnUrl)) return null;

            // Accept local relative URLs
            if (Url.IsLocalUrl(returnUrl)) return returnUrl;

            // If absolute but same origin, convert to relative path+query
            if (Uri.TryCreate(returnUrl, UriKind.Absolute, out var absolute))
            {
                var req = HttpContext.Request;
                var sameHost = string.Equals(absolute.Scheme, req.Scheme, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(absolute.Host, req.Host.Host, StringComparison.OrdinalIgnoreCase)
                    && (absolute.Port == req.Host.Port || !absolute.IsDefaultPort && req.Host.Port == null);
                if (sameHost)
                {
                    var relative = absolute.PathAndQuery;
                    if (Url.IsLocalUrl(relative)) return relative;
                }
            }
            // Otherwise, reject
            return null;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        { 
            // Bring the collection that waits the view(IEnumerable<Department>)
            var departments = await _db.Departments
                .Include(d => d.ManagerUser)
                .Include(d => d.Clubs)
                .Include(d => d.Students)
                .AsNoTracking()
                .ToListAsync();

            return View(departments);


        }


        [HttpGet]
        [Authorize(Roles = "Admin, DepartmentManager")]
        public async Task<IActionResult> Create()
        {

            var users = await GetAssignableManagersAsync();

            var vm = new DepartmentFormViewModel
            {
                Department = new Department(),  // department is empty (new)
                Managers = users                // list is loaded!
            };

            return View(vm);  
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, DepartmentManager")]
        public async Task<IActionResult> Create(DepartmentFormViewModel vm)
        {
            var exists = await _db.Departments
            .AnyAsync(d => d.Email == vm.Department.Email);

            if (exists)
            {
                ModelState.AddModelError("Department.Email",
                    "Este email ya está registrado en otro departamento.");
                vm.Managers = await GetAssignableManagersAsync();
                return View(vm);
            }

            if (!ModelState.IsValid || !TryValidateModel(vm.Department) )
            {
                // recargar lista de managers si falla la validación
                vm.Managers = await GetAssignableManagersAsync();
                return View(vm);
            }

            _db.Departments.Add(vm.Department);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Departamento creado correctamente.";
            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id, string? returnUrl = null)
        {
            var department = await _db.Departments
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null)
                return NotFound();

            //We have to pass the user list for the drop down
            var users = await GetAssignableManagersAsync();

            var vm = new DepartmentFormViewModel
            {
                Department = department,
                Managers = users,
                ReturnUrl = NormalizeReturnUrl(returnUrl)
            };

            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DepartmentFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Managers = await GetAssignableManagersAsync();
                return View(vm);
            }

            var departmentToUpdate = await _db.Departments.FindAsync(vm.Department.Id);
            if (departmentToUpdate == null)
                return NotFound();

            
            departmentToUpdate.Name = vm.Department.Name;
            departmentToUpdate.PhoneNumber = vm.Department.PhoneNumber;
            departmentToUpdate.Email = vm.Department.Email;
            departmentToUpdate.OfficeLocation = vm.Department.OfficeLocation;
            departmentToUpdate.ManagerUserId = vm.Department.ManagerUserId;

            await _db.SaveChangesAsync();

            TempData["Success"] = "Departamento actualizado correctamente.";

            var normalizedReturn = NormalizeReturnUrl(vm.ReturnUrl);
            if (!string.IsNullOrEmpty(normalizedReturn))
                return LocalRedirect(normalizedReturn);

            return RedirectToAction("Index");
        }


        //if you check the data base→ use async/await

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var department = await _db.Departments
                .Include(d => d.Clubs)
                .Include(d => d.Students)
                .Include(d => d.ManagerUser)
                .Include(d => d.Events)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null)
                return NotFound();

          

            return View(department);
        }



        // DELETE - Solo Admin
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]  
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var department = await _db.Departments
                .Include(d => d.Clubs)
                .Include(d => d.Students)
                .Include(d=>d.ManagerUser)
                .Include(d => d.Events)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null)
                return NotFound();

            if (department.Clubs.Any() || department.Students.Any() || department.Events.Any())
            {
                TempData["Error"] = "No se puede eliminar un departamento con clubs, estudiantes o eventos asociados.";
                return RedirectToAction("Index");
            }

            _db.Departments.Remove(department);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Departamento eliminado correctamente.";
            return RedirectToAction("Index");
        }


        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var department = await _db.Departments
                 .Include(d => d.ManagerUser)
                .Include(d => d.Clubs)
                    .ThenInclude(c => c.StudentClubs)
                        .ThenInclude(sc => sc.UserStudent)
                .Include(d => d.Events)
                .AsNoTracking() 
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null)
                return NotFound();

            // Calculate the number of the students
            var uniqueStudents = (department.Clubs ?? Enumerable.Empty<Club>())
                .SelectMany(c => c.StudentClubs ?? Enumerable.Empty<StudentClub>())
                .Select(sc => sc.UserStudent)
                .Where(u => u != null)
                .DistinctBy(s => s.Id)
                .ToList();

            ViewData["UniqueStudentsCount"] = uniqueStudents.Count;
            ViewData["UniqueStudents"] = uniqueStudents; 

            return View(department);
        }

        private async Task<List<ApplicationUser>> GetAssignableManagersAsync()
        {
            // 1. Find the Admin role
            var adminRole = await _db.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Name == "Admin");

            var adminUserIds = new List<int>();

            if (adminRole != null)
            {
                // Get the IDs of all users who have the Admin role
                adminUserIds = await _db.UserRoles
                    .AsNoTracking()
                    .Where(ur => ur.RoleId == adminRole.Id)
                    .Select(ur => ur.UserId)
                    .ToListAsync();
            }

            // 2. Emails that must be EXCLUDED from the dropdown (generic test users)
            var excludedEmails = new List<string>
            {
                "user01@businessschool.com",
                "user02@businessschool.com",
                "user03@businessschool.com"
            };

            // 3. Return ONLY valid assignable users
            //    - Exclude Admins
            //    - Exclude generic test users
            var users = await _db.Users
                .AsNoTracking()
                .Where(u =>
                    !adminUserIds.Contains(u.Id) &&   // exclude Admin users
                    !excludedEmails.Contains(u.Email)) // exclude test/demo users
                .OrderBy(u => u.FullName)
                .ToListAsync();

            return users;
        }




    }

}