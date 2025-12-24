using Business_School.Models;
using Business_School.Models.JoinTables;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Business_School.Data
{
    // Password must comply with ASP.NET Core Identity default policy:
    // - At least 6 characters
    // - At least one uppercase, one lowercase, one digit, and one special character
    // ? "Student123!" meets all requirements ("Student123" would fail)

    public static class DataSeeder
    {
        public static async Task SeedAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<int>> roleManager,
            ApplicationDbContext db)
        {
            // -----------------------------
            // 1) ROLES
            // -----------------------------
            string[] roles = { "Admin", "DepartmentManager", "ClubLeader", "Student", "User" };
            foreach (var r in roles)
            {
                if (!await roleManager.RoleExistsAsync(r))
                    await roleManager.CreateAsync(new IdentityRole<int>(r));
            }

            // -----------------------------
            // 2) USERS (idempotente)
            // -----------------------------
            // Returns the created or existing ApplicationUser
            async Task<ApplicationUser> EnsureUser(string email, string fullName, string password, string role)
            {
                var u = await userManager.FindByEmailAsync(email);
                if (u != null) return u;

                var newU = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = fullName,
                    EmailConfirmed = true
                };

                var res = await userManager.CreateAsync(newU, password);
                if (!res.Succeeded)
                {
                    // collect errors to help debugging during dev
                    var errors = string.Join("; ", res.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Error creating user {email}: {errors}");
                }

                await userManager.AddToRoleAsync(newU, role);
                // reload to get assigned Id
                return await userManager.FindByEmailAsync(email);
            }

            var admin = await EnsureUser("admin@businessschool.com", "System Administrator", "Admin123!", "Admin");

            var manager_hr = await EnsureUser("manager.hr@businessschool.com", "Laura Gómez", "Manager456!", "DepartmentManager");
            var manager_finance = await EnsureUser("manager.finance@businessschool.com", "Carlos Duarte", "Manager456!", "DepartmentManager");
            var manager_it = await EnsureUser("manager.it@businessschool.com", "Ana Morales", "Manager456!", "DepartmentManager");

            var leader_sports = await EnsureUser("leader.sports@businessschool.com", "Diego Ruiz", "Leader456!", "ClubLeader");
            var leader_music = await EnsureUser("leader.music@businessschool.com", "Sara Medina", "Leader456!", "ClubLeader");

            var s01 = await EnsureUser("student01@businessschool.com", "María Pérez", "Student123!", "Student");
            var s02 = await EnsureUser("student02@businessschool.com", "Jorge López", "Student123!", "Student");
            var s03 = await EnsureUser("student03@businessschool.com", "Elena Díaz", "Student123!", "Student");
            var s04 = await EnsureUser("student04@businessschool.com", "Ricardo Torres", "Student123!", "Student");
            var s05 = await EnsureUser("student05@businessschool.com", "Claudia Ramos", "Student123!", "Student");

            var user01 = await EnsureUser("user01@businessschool.com", "Usuario General 1", "User123!", "User");
            var user02 = await EnsureUser("user02@businessschool.com", "Usuario General 2", "User123!", "User");
            var user03 = await EnsureUser("user03@businessschool.com", "Usuario General 3", "User123!", "User");

            // -----------------------------
            // 3) ASSIGN MANAGERS TO DEPARTMENTS
            //    (we agreed these mappings)
            // -----------------------------
            // Dept 1 -> manager.finance
            // Dept 2 -> manager.hr
            // Dept 3 -> manager.it
            var dept1 = await db.Departments.FirstOrDefaultAsync(d => d.Id == 1);
            var dept2 = await db.Departments.FirstOrDefaultAsync(d => d.Id == 2);
            var dept3 = await db.Departments.FirstOrDefaultAsync(d => d.Id == 3);

            bool changed = false;
            if (dept1 != null && dept1.ManagerUserId != manager_finance.Id)
            {
                dept1.ManagerUserId = manager_finance.Id;
                changed = true;
            }
            if (dept2 != null && dept2.ManagerUserId != manager_hr.Id)
            {
                dept2.ManagerUserId = manager_hr.Id;
                changed = true;
            }
            if (dept3 != null && dept3.ManagerUserId != manager_it.Id)
            {
                dept3.ManagerUserId = manager_it.Id;
                changed = true;
            }

            if (changed)
                await db.SaveChangesAsync();

            // -----------------------------
            // 3b) ASSIGN STUDENTS TO DEPARTMENTS (previously missing → chart showed zeros)
            // -----------------------------
            // Simple distribution; adjust as needed.
            async Task AssignDeptIfNull(ApplicationUser stu, int? deptId)
            {
                if (deptId.HasValue && stu.DepartmentId != deptId.Value)
                {
                    stu.DepartmentId = deptId.Value;
                    await userManager.UpdateAsync(stu);
                }
            }

            await AssignDeptIfNull(s01, dept1?.Id);
            await AssignDeptIfNull(s02, dept2?.Id);
            await AssignDeptIfNull(s03, dept3?.Id);
            await AssignDeptIfNull(s04, dept2?.Id);
            await AssignDeptIfNull(s05, dept1?.Id);

            // -----------------------------
            // 4) ASSIGN CLUB LEADERS (mappings decided)
            //    Finance Club (1)   -> leader_sports
            //    Marketing (2)      -> leader_music
            //    Startup League (3) -> leader_sports
            //    Women in Business (4) -> leader_music
            // -----------------------------
            var club1 = await db.Clubs.FirstOrDefaultAsync(c => c.Id == 1);
            var club2 = await db.Clubs.FirstOrDefaultAsync(c => c.Id == 2);
            var club3 = await db.Clubs.FirstOrDefaultAsync(c => c.Id == 3);
            var club4 = await db.Clubs.FirstOrDefaultAsync(c => c.Id == 4);

            changed = false;
            if (club1 != null && club1.LeaderId != leader_sports.Id) { club1.LeaderId = leader_sports.Id; changed = true; }
            if (club2 != null && club2.LeaderId != leader_music.Id) { club2.LeaderId = leader_music.Id; changed = true; }
            if (club3 != null && club3.LeaderId != leader_sports.Id) { club3.LeaderId = leader_sports.Id; changed = true; }
            if (club4 != null && club4.LeaderId != leader_music.Id) { club4.LeaderId = leader_music.Id; changed = true; }

            // Set initial capacities (open seats) if not configured
            if (club1 != null && club1.Capacity == null) { club1.Capacity = 30; changed = true; }
            if (club2 != null && club2.Capacity == null) { club2.Capacity = 25; changed = true; }
            if (club3 != null && club3.Capacity == null) { club3.Capacity = 40; changed = true; }
            if (club4 != null && club4.Capacity == null) { club4.Capacity = 50; changed = true; }

            if (changed)
                await db.SaveChangesAsync();

            // -----------------------------
            // 5) STUDENT-CLUB MEMBERSHIPS
            //    s01 -> 1,2
            //    s02 -> 3
            //    s03 -> 1,4
            //    s04 -> 2
            //    s05 -> 3,4
            // -----------------------------
            // Obtén los Ids de clubes existentes en la base de datos
            var clubIdsExistentes = await db.Clubs.Select(c => c.Id).ToListAsync();

            async Task AddStudentClubIfNotExists(int studentId, int clubId, DateTime joinedAt, bool isLeader = false, int points = 0)
            {
                // Solo agrega la relación si el club existe
                if (!clubIdsExistentes.Contains(clubId))
                    return;

                var exists = await db.StudentClubs.AnyAsync(sc => sc.UserStudentId == studentId && sc.ClubId == clubId);
                if (!exists)
                {
                    // Enforce capacity at seeding time
                    var club = await db.Clubs.Include(c => c.StudentClubs).FirstOrDefaultAsync(c => c.Id == clubId);
                    if (club?.Capacity != null && club.StudentClubs.Count >= club.Capacity.Value)
                        return; // skip if full

                    db.StudentClubs.Add(new StudentClub
                    {
                        UserStudentId = studentId,
                        ClubId = clubId,
                        JoinedAt = joinedAt,
                        IsLeader = isLeader,
                        PointsFromThisClub = points
                    });
                }
            }

            // ensure users have Ids
            var idMap = new Dictionary<string, int>
            {
                ["student01@businessschool.com"] = s01.Id,
                ["student02@businessschool.com"] = s02.Id,
                ["student03@businessschool.com"] = s03.Id,
                ["student04@businessschool.com"] = s04.Id,
                ["student05@businessschool.com"] = s05.Id,
            };

            var now = DateTime.UtcNow.Date;

            await AddStudentClubIfNotExists(idMap["student01@businessschool.com"], 1, new DateTime(2025, 9, 15));
            await AddStudentClubIfNotExists(idMap["student01@businessschool.com"], 2, new DateTime(2025, 9, 16));
            await AddStudentClubIfNotExists(idMap["student02@businessschool.com"], 3, new DateTime(2025, 9, 20));
            await AddStudentClubIfNotExists(idMap["student03@businessschool.com"], 1, new DateTime(2025, 9, 10));
            await AddStudentClubIfNotExists(idMap["student03@businessschool.com"], 4, new DateTime(2025, 10, 1));
            await AddStudentClubIfNotExists(idMap["student04@businessschool.com"], 2, new DateTime(2025, 10, 5));
            await AddStudentClubIfNotExists(idMap["student05@businessschool.com"], 3, new DateTime(2025, 9, 5));
            await AddStudentClubIfNotExists(idMap["student05@businessschool.com"], 4, new DateTime(2025, 9, 6));

            // Save student-club additions
            await db.SaveChangesAsync();

            // -----------------------------
            // 6) Ensure every user has at least role "User" (safe)
            // -----------------------------
            var allUsers = userManager.Users.ToList();
            foreach (var u in allUsers)
            {
                var rolesAssigned = await userManager.GetRolesAsync(u);
                if (!rolesAssigned.Any())
                {
                    await userManager.AddToRoleAsync(u, "User");
                }
            }

            // done
        }
    }
}
