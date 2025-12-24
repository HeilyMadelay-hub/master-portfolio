using Business_School.Data;
using Business_School.Models;
using System.Security.Claims;

namespace Business_School.Helpers
{

    /*
    * What is a ClaimsPrincipal?
    *
    * The ClaimsPrincipal is the fundamental object in .NET and 
    * ASP.NET Core that represents the verified identity of the logged-in user, 
    * serving as the basis for access control and authorization. Essentially,
    * it acts as a container for one or more ClaimsIdentity (the "identity documents"),
    * and each identity is comprised of multiple Claims (verified declarations such as 
    * the user's role, name, or ID). The system accesses this object via HttpContext.
    * User to determine "who they are" (authentication) and "what they can do" (authorization)
    * within the application, making security more flexible and granular than the traditional
    * role-based model.
    * 
    * 
    */
    public static class RoleHelper
    {

        public const string Admin = "Admin";
        public const string DepartmentManager = "DepartmentManager";
        public const string ClubLeader = "ClubLeader";
        public const string Student = "Student";

        public static bool IsTheUserInAnyRole(this ClaimsPrincipal user, params string[] roles)
        {
            return roles.Any(role => user.IsInRole(role));
        }

        public static string CombineRolesWithAComaForUseInAuthorize(params string[] roles)
        {
            return string.Join(",", roles);
        }

        //Admin
        public static bool CanUserManageStudents(this ClaimsPrincipal user)
        {
            return user.IsInRole(Admin);
        }

        //Admin, DepartmentManager o ClubLeader
        public static bool CanManageClubs(this ClaimsPrincipal user)
        {

           return user.IsInRole(Admin) || user.IsInRole(DepartmentManager) || user.IsInRole(ClubLeader);

        }

        public static bool CanViewDepartment(this ClaimsPrincipal user)
        {
            return user.IsInRole(Admin) || user.IsInRole(DepartmentManager) || user.IsInRole(ClubLeader) || user.IsInRole(Student);
        }

    }
}