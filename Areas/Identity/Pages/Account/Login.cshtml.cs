// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using gestion_pharma.Data;

namespace gestion_pharma.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<Userpers> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly UserManager<Userpers> _userManager;

        public LoginModel(SignInManager<Userpers> signInManager, ILogger<LoginModel> logger, UserManager<Userpers> userManager)
        {
            _signInManager = signInManager;
            _logger = logger;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            // Utiliser ReturnUrl de la propriété (du formulaire caché) si disponible
            returnUrl = returnUrl ?? this.ReturnUrl ?? Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");

                    // If a safe local returnUrl was provided and it's not the application root, honor it first
                    var appRoot = Url.Content("~/");
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) && !string.Equals(returnUrl, appRoot, StringComparison.OrdinalIgnoreCase))
                    {
                        return LocalRedirect(returnUrl);
                    }

                    // Récupérer l'utilisateur et ses rôles pour rediriger si pas de specific returnUrl
                    var user = await _userManager.FindByEmailAsync(Input.Email);
                    if (user != null)
                    {
                        var roles = await _userManager.GetRolesAsync(user);
                        if (roles.Contains("Client"))
                        {
                            // Client -> catalogue des produits
                            return LocalRedirect("~/Produits");
                        }
                        if (roles.Contains("Parapharmacien"))
                        {
                            // Parapharmacien -> zone de gestion (adapter selon vos routes)
                            return LocalRedirect("~/ParapharmacienDashboard");
                        }
                        if (roles.Contains("Admin"))
                        {
                            // Admin -> tableau de bord admin (ne devrait pas être auto-créé via UI)
                            return LocalRedirect("~/Admin/Index");
                        }
                    }

                    // fallback to app root
                    return LocalRedirect(appRoot);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
