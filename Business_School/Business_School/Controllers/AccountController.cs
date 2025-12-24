// AccountController: handles user authentication (login, register, logout).
// Protected actions require [Authorize]. Administrative tasks (roles/users) are handled by the Admin/Role controller.
// Without logging in you cannot test endpoints decorated with [Authorize].

using Business_School.Data;
using Business_School.Models;
using Business_School.Models.ViewModels;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Business_School.Controllers
{

    //[Authorize] NO SE PONE POR LA COOKIE
    public class AccountController : Controller
    {

        private readonly UserManager<ApplicationUser> _userManager; //User administration
        private readonly SignInManager<ApplicationUser> _signInManager;// Admin session
        private readonly ApplicationDbContext _context;

        public AccountController(UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;

            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            // Ensure ReturnUrl is preserved in the model so the hidden field posts it back
            return View(new LoginVM { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM model, string? returnUrl = null)
        {
            // Prefer the value coming from the model, which is posted by the hidden field
            var targetUrl = string.IsNullOrWhiteSpace(model.ReturnUrl) ? returnUrl : model.ReturnUrl;

            ViewData["ReturnUrl"] = targetUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                isPersistent: model.RememberMe,
                lockoutOnFailure: false
            );

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                // If there is a valid local returnUrl, go there first
                if (!string.IsNullOrEmpty(targetUrl) && Url.IsLocalUrl(targetUrl))
                {
                    return LocalRedirect(targetUrl);
                }

                // Otherwise, redirect based on role
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    return RedirectToAction("Admin", "Dashboard");
                }
                if (await _userManager.IsInRoleAsync(user, "DepartmentManager"))
                {
                    return RedirectToAction("DepartmentManager", "Dashboard");
                }
                if (await _userManager.IsInRoleAsync(user, "ClubLeader"))
                {
                    return RedirectToAction("ClubLeader", "Dashboard");
                }
                if (await _userManager.IsInRoleAsync(user, "Student"))
                {
                    return RedirectToAction("Student", "Dashboard");
                }

                // Default fallback
                return RedirectToAction("Index", "Dashboard");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Account locked out. Please try again later.");
                return View(model);
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt. Please check your email and password.");
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;


            // Prepare Department dropdown for the first time
            // We query Departments, order them, and convert to a SelectList
            // so the view can render the dropdown.

            var departments = _context.Departments
            .OrderBy(d => d.Name)
            .Select(d => new { d.Id, d.Name })
            .ToList();

            var model = new RegisterVM();
            model.DepartmentList = new SelectList(departments, "Id", "Name");

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVM model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            // -----------------------------
            // Invalid ModelState
            // -----------------------------
            // If the form has validation errors (e.g., required fields missing, password too short),
            // we need to repopulate the Department dropdown before returning the view,
            // because the SelectList items are lost automatically on postback.
            if (!ModelState.IsValid)
            {
                var deps = _context.Departments
                .OrderBy(d => d.Name)
                .Select(d => new { d.Id, d.Name })
                .ToList();

                model.DepartmentList = new SelectList(deps, "Id", "Name");

                return View(model);
            }

            // -----------------------------
            // Create user
            // -----------------------------
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = $"{model.FirstName} {model.LastName}",
                DepartmentId = model.DepartmentId,
                Level = model.Level
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            // -----------------------------
            // User created successfully
            // -----------------------------
            // If user creation succeeds:
            // - Assign "User" role
            // - Sign in the user
            // - Redirect to returnUrl or Index
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");
                await _signInManager.SignInAsync(user, isPersistent: model.RememberMe);

                TempData["Success"] = "Account created successfully!";

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return LocalRedirect(returnUrl);

                return RedirectToAction("Index", "Account");
            }

            // -----------------------------
            // Failed to create user
            // -----------------------------
            // If CreateAsync fails (e.g., duplicate email, invalid password):
            // - Add errors to ModelState
            // - Repopulate the Department dropdown so the form can be
            // redisplayed without losing the user's selection
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            var departments = _context.Departments
            .OrderBy(d => d.Name)
            .Select(d => new { d.Id, d.Name })
            .ToList();

            model.DepartmentList = new SelectList(departments, "Id", "Name");

            return View(model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {

            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return RedirectToAction("Login");

        }


    }

}