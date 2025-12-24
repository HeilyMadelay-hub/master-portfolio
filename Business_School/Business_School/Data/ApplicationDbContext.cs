using Business_School.Models;
using Business_School.Models.JoinTables;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Reflection.Emit;
using System.Runtime.Intrinsics.X86;

namespace Business_School.Data
{
    // We use:
    // IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    //
    // ✔ Why does it have to be EXACTLY like this?
    // ---------------------------------------------------------------------------
    //
    // 1) APPLICATIONUSER IS OUR CUSTOM USER CLASS
    // ------------------------------------------------
    // We have created the ApplicationUser class, which inherits from IdentityUser<int>.
    // Therefore, the DbContext must declare this exact type as the main Identity user model.
    //
    // 2) WE USE INTEGER (int) PRIMARY KEYS, NOT string
    // ------------------------------------------------
    // By specifying the <int> parameter, Identity will generate all its tables 
    // (Users, Roles, Claims, Tokens, UserLogins…) using integer primary keys. 
    // This keeps the entire database consistent.
    //
    // 3) WE USE ROLES WITH INTEGER TYPE → IdentityRole<int>
    // -----------------------------------------------------
    // We don’t want the default string-based roles. Our solution uses IdentityRole<int>,
    // and the DbContext must explicitly specify it.
    //
    // ---------------------------------------------------------------------------
    // INCORRECT EXAMPLES (DO NOT USE):
    // ---------------------------------------------------------------------------
    //
    // ❌ public class ApplicationDbContext : IdentityDbContext
    // → uses strings for IDs, breaks compatibility.
    //
    // ❌ public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    // → missing the Role type and primary key type.
    //
    // ❌ public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, int>
    // → IdentityRole uses string by default, does not match our int PK.
    //
    // ---------------------------------------------------------------------------
    // THE CORRECT WAY (YOUR CASE):
    // ---------------------------------------------------------------------------

    //public class ApplicationDbContext
    //    : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    //{
    //    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    //        : base(options) { }

    //    // DbSets go here
    //}

    //---------------------------------------------------------------------------
    // DIRECT ADVANTAGES:
    // ---------------------------------------------------------------------------
    //✔ All Identity tables will use integer IDs(total consistency).
    //✔ ApplicationUser fits perfectly into the Identity system.
    //✔ No foreign key conflicts with StudentId, OrganizerId, LeaderId, etc.
    //✔ This is the standard configuration for projects using ASP.NET Core Identity
    //   with integer primary keys.

    //---------------------------------------------------------------------------
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {

        // We include all DbSets, even those of intermediate tables, because they contain extra fields.
        // We only omit DbSets for tables that do not have any additional data.

        public DbSet<Department> Departments { get; set; } = null!;
        public DbSet<Club> Clubs { get; set; } = null!;
        public DbSet<Event> Events { get; set; } = null!;
        public DbSet<Achievement> Achievements { get; set; } = null!;

        public DbSet<StudentClub> StudentClubs { get; set; } = null!;
        public DbSet<StudentAchievement> StudentAchievements { get; set; } = null!;
        public DbSet<EventClub> EventClubs { get; set; } = null!;
        public DbSet<EventAttendance> EventAttendances { get; set; } = null!;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ================================================================
        // SEEDING IN EF CORE + IDENTITY – EXPLANATION
        // ================================================================

        // 1️⃣ Simple tables (no passwords, no Identity):
        //    - Department, Club, Event, EventClub
        //    - Can be inserted directly using builder.Entity<...>.HasData()
        //    - EF Core simply generates INSERTs in the database
        //    - Example:
        //      builder.Entity<Department>().HasData(
        //          new Department { Id = 1, Name = "Finance" }
        //      );

        // 2️⃣ Identity (ApplicationUser, Roles):
        //    - CANNOT be inserted with HasData()
        //    - Identity requires:
        //       • Hashing the password
        //       • Creating a SecurityStamp
        //       • Properly configuring UserName, Email, etc.
        //    - That’s why we use UserManager and RoleManager
        //    - Example:
        //      var admin = new ApplicationUser { UserName = "admin@x.com", Email = "admin@x.com" };
        //      await userManager.CreateAsync(admin, "Admin123!"); // ✅ Hashes the password
        //      await userManager.AddToRoleAsync(admin, "Admin");   // ✅ Assigns role

        // 3️⃣ Summary:
        //    ┌───────────────┐     ┌─────────────────────────┐
        //    │ HasData()     │     │ UserManager / RoleManager│
        //    │ (OnModelCreating) │   │                         │
        //    └───────┬───────┘     └─────────┬───────────────┘
        //            │                           │
        //  Simple tables inserted          Users and roles inserted
        //  directly into the DB            correctly with Identity

        // 4️⃣ Execution flow on app startup:
        //    • First, Migrations are applied → creates tables
        //    • Then, OnModelCreating runs → seeds simple tables
        //    • Finally, in Program.cs, DataSeeder runs → creates roles and users


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ==============================================================
            // 1. Composite Keys
            // ==============================================================

            builder.Entity<StudentClub>()
                .HasKey(sc => new { sc.UserStudentId, sc.ClubId });

            builder.Entity<EventClub>()
                .HasKey(ec => new { ec.EventId, ec.ClubId });

            builder.Entity<EventAttendance>()
                .HasKey(ea => new { ea.EventId, ea.UserStudentId });

            builder.Entity<StudentAchievement>()
                .HasKey(sa => new { sa.UserStudentId, sa.AchievementId });

            // ==============================================================
            // 2. Relationships
            // ==============================================================

            builder.Entity<ApplicationUser>()
                .HasOne(u => u.Department)
                .WithMany(d => d.Students)
                .HasForeignKey(u => u.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Department>()
                .HasOne(d => d.ManagerUser)
                .WithMany()
                .HasForeignKey(d => d.ManagerUserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Club>()
                .HasOne(c => c.Department)
                .WithMany(d => d.Clubs)
                .HasForeignKey(c => c.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Event>()
                .HasOne(e => e.Department)
                .WithMany(d => d.Events)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Event>()
                .HasOne(e => e.Organizer)
                .WithMany()
                .HasForeignKey(e => e.OrganizerId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<StudentClub>()
                .HasOne(sc => sc.UserStudent)
                .WithMany(u => u.ClubMemberships)
                .HasForeignKey(sc => sc.UserStudentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<EventAttendance>()
                .HasOne(ea => ea.UserStudent)
                .WithMany(u => u.EventAttendances)
                .HasForeignKey(ea => ea.UserStudentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StudentAchievement>()
                .HasOne(sa => sa.UserStudent)
                .WithMany(u => u.Achievements)
                .HasForeignKey(sa => sa.UserStudentId)
                .OnDelete(DeleteBehavior.Cascade);

            // ==============================================================
            // 3. Properties Config
            // ==============================================================

            builder.Entity<Department>()
                .HasIndex(d => d.Email)
                .IsUnique();

            // ==============================================================
            // 4. SAFE SEEDING (solo tablas sin FK a AspNetUsers)
            // ==============================================================

            builder.Entity<Department>().HasData(
                new Department { Id = 1, Name = "Finance & Accounting", PhoneNumber = "601-1001", Email = "finance@businessschool.com", OfficeLocation = "Building A - Room 201" },
                new Department { Id = 2, Name = "Marketing & Sales", PhoneNumber = "601-1002", Email = "marketing@businessschool.com", OfficeLocation = "Building B - Room 105" },
                new Department { Id = 3, Name = "Entrepreneurship", PhoneNumber = "601-1003", Email = "entrepreneur@businessschool.com", OfficeLocation = "Building C - Room 301" }
            );

            builder.Entity<Club>().HasData(
                new Club { Id = 1, Name = "Finance Club", Description = "Investment and stock market", DepartmentId = 1 },
                new Club { Id = 2, Name = "Marketing Masters", Description = "Digital marketing and branding", DepartmentId = 2 },
                new Club { Id = 3, Name = "Startup League", Description = "Pitch your business idea", DepartmentId = 3 },
                new Club { Id = 4, Name = "Women in Business", Description = "Empowerment and networking", DepartmentId = 2 }
            );

            builder.Entity<Event>().HasData(
                new Event { Id = 1, Title = "Investment Workshop", Description = "Learn about stocks", StartDate = new DateTime(2025, 12, 10, 18, 0, 0), EndDate = new DateTime(2025, 12, 10, 20, 0, 0), Capacity = 50, DefaultPointsReward = 20, DepartmentId = 1 },
                new Event { Id = 2, Title = "Digital Marketing Trends 2026", Description = "TikTok and AI", StartDate = new DateTime(2025, 12, 15, 17, 30, 0), EndDate = new DateTime(2025, 12, 15, 19, 0, 0), Capacity = 80, DefaultPointsReward = 15, DepartmentId = 2 },
                new Event { Id = 3, Title = "Pitch Night", Description = "Present your startup", StartDate = new DateTime(2025, 12, 20, 19, 0, 0), EndDate = new DateTime(2025, 12, 20, 22, 0, 0), Capacity = 40, DefaultPointsReward = 30, DepartmentId = 3 },
                new Event { Id = 4, Title = "Women Leadership Panel", Description = "Inspiring talks", StartDate = new DateTime(2026, 1, 8, 18, 0, 0), EndDate = new DateTime(2026, 1, 8, 20, 0, 0), Capacity = null, DefaultPointsReward = 25, DepartmentId = 2 },
                new Event { Id = 5, Title = "Crypto & Blockchain Basics", Description = "Intro to Web3", StartDate = new DateTime(2026, 1, 15, 17, 0, 0), EndDate = new DateTime(2026, 1, 15, 18, 30, 0), Capacity = 60, DefaultPointsReward = 20, DepartmentId = 1 },
                new Event { Id = 6, Title = "Startup Weekend", Description = "Build your MVP", StartDate = new DateTime(2026, 1, 25, 9, 0, 0), EndDate = new DateTime(2026, 1, 26, 18, 0, 0), Capacity = 30, DefaultPointsReward = 50, DepartmentId = 3 }
            );

            builder.Entity<EventClub>().HasData(
                new EventClub { EventId = 1, ClubId = 1 },
                new EventClub { EventId = 2, ClubId = 2 },
                new EventClub { EventId = 3, ClubId = 3 },
                new EventClub { EventId = 4, ClubId = 4 },
                new EventClub { EventId = 5, ClubId = 1 },
                new EventClub { EventId = 6, ClubId = 3 }
            );
        }


    }
}
