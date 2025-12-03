using MiniAppGIBA.Base.Database;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.Admins;
using MiniAppGIBA.Entities.Memberships;
using MiniAppGIBA.Base.Helper;
using Microsoft.EntityFrameworkCore;

namespace MiniAppGIBA.Base.DataSeeder
{
    public static class UserAdmin
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider, ApplicationDbContext context)
        {
            using var scope = serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var rolesRepository = unitOfWork.GetRepository<Roles>();
            var membershipRepository = unitOfWork.GetRepository<Membership>();

            // --- 1. Seed Roles ---
            var roleNames = new[] { "GIBA" };

            foreach (var roleName in roleNames)
            {
                var existingRole = await rolesRepository.AsQueryable()
                    .FirstOrDefaultAsync(r => r.Name == roleName);

                if (existingRole == null)
                {
                    var role = new Roles
                    {
                        Name = roleName,
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    };
                    await rolesRepository.AddAsync(role);
                    Console.WriteLine($"[ROLE SEEDED] Created role: {roleName}");
                }
                else
                {
                    Console.WriteLine($"[INFO] Role already exists: {roleName}");
                }
            }

            await unitOfWork.SaveChangesAsync();

            // --- 2. Seed Default GIBA (Super Admin) User ---
            var gibaRole = await rolesRepository.AsQueryable()
                .FirstOrDefaultAsync(r => r.Name == "GIBA");

            if (gibaRole == null)
            {
                Console.WriteLine($"[ERROR] GIBA role not found. Please seed roles first.");
                return;
            }

            var superAdminUsername = "superadmin@miniappbkasim.com";
            var existingSuperAdmin = await membershipRepository.AsQueryable()
                .FirstOrDefaultAsync(m => m.Username == superAdminUsername);

            if (existingSuperAdmin == null)
            {
                var superAdmin = new Membership
                {
                    UserZaloId = Guid.NewGuid().ToString("N"),
                    UserZaloName = "Super Administrator",
                    Fullname = "Super Administrator",
                    Username = superAdminUsername,
                    Password = AuthHelper.HashPassword("Admin@2025"),
                    PhoneNumber = "0000000001",
                    RoleId = gibaRole.Id,
                    Slug = superAdminUsername.ToLower().Replace(" ", "-"),
                    CreatedDate = DateTime.UtcNow
                };

                await membershipRepository.AddAsync(superAdmin);
                await unitOfWork.SaveChangesAsync();
                Console.WriteLine($"[USER SEEDED] Created GIBA (Super Admin) user: {superAdmin.Username}");
            }
            else
            {
                Console.WriteLine($"[INFO] GIBA (Super Admin) user already exists: {existingSuperAdmin.Username}");
            }
        }
    }
}
