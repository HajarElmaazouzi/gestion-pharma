using gestion_pharma.Data;
using gestion_pharma.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace gestion_pharma
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // DbContexts
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddDbContext<ApplicationIdentityDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Identity
            builder.Services.AddIdentity<Userpers, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;

                // Password policy: keep it simple for admin-created accounts
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredUniqueChars = 1;
            })
            .AddEntityFrameworkStores<ApplicationIdentityDbContext>()
            .AddDefaultTokenProviders();

            // Configure Authentication
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Identity/Account/Login";
                options.LogoutPath = "/Identity/Account/Logout";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
            });

            // AJOUTEZ CES LIGNES - EmailSender factice pour le développement
            builder.Services.AddSingleton<IEmailSender, DummyEmailSender>();

            // Services
            builder.Services.AddScoped<FidelityService>();

            // MVC + Razor Pages
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var roles = new[] { "Admin", "Client", "Parapharmacien" };
                foreach (var role in roles)
                {
                    if (!roleManager.RoleExistsAsync(role).GetAwaiter().GetResult())
                    {
                        roleManager.CreateAsync(new IdentityRole(role)).GetAwaiter().GetResult();
                    }
                }
                // Optionnel : créer ici un utilisateur Admin sécurisé (hors UI publique).
            }

            // --- Seed roles & default admin ---
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                    var userManager = services.GetRequiredService<UserManager<Userpers>>();
                    string[] roles = new[] { "Admin", "Parapharmacien", "Client" };

                    foreach (var role in roles)
                    {
                        if (!roleManager.RoleExistsAsync(role).GetAwaiter().GetResult())
                        {
                            roleManager.CreateAsync(new IdentityRole(role)).GetAwaiter().GetResult();
                        }
                    }

                    // admin par défaut
                    var adminEmail = "admin@local";
                    if (userManager.FindByEmailAsync(adminEmail).GetAwaiter().GetResult() == null)
                    {
                        var adminUser = new Userpers
                        {
                            UserName = adminEmail,
                            Email = adminEmail,
                            Nom = "Administrateur",
                            EmailConfirmed = true
                        };
                        var createRes = userManager.CreateAsync(adminUser, "P@ssw0rd123!").GetAwaiter().GetResult();
                        if (createRes.Succeeded)
                        {
                            userManager.AddToRoleAsync(adminUser, "Admin").GetAwaiter().GetResult();
                        }
                    }
                }
                catch (Exception ex)
                {
                    // log si nécessaire
                    Console.WriteLine($"Erreur seed roles: {ex.Message}");
                }
            }
            // --- end seed ---

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorPages();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
    // Classe DummyEmailSender - à mettre dans le fichier Program.cs
    public class DummyEmailSender : Microsoft.AspNetCore.Identity.UI.Services.IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Ne fait rien, juste pour éviter l'erreur de dépendance
            return Task.CompletedTask;
        }
    }
}

