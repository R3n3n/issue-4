using ITElectiveSSO.Models;
using ITELECTIVE_SSO.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Tests
{
    public class UserManagementTests
    {
        private static UserManager<ApplicationUser> BuildUserManager(string dbName)
        {
            var services = new ServiceCollection();

            services.AddLogging();

            services.AddDbContext<SsoDbContext>(options =>
                options.UseInMemoryDatabase(dbName));

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;

                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            })
            .AddEntityFrameworkStores<SsoDbContext>()
            .AddDefaultTokenProviders();

            var provider = services.BuildServiceProvider();
            return provider.GetRequiredService<UserManager<ApplicationUser>>();
        }

        [Fact]
        public async Task CreateUser_Succeeds_WithValidData()
        {
            var userManager = BuildUserManager(Guid.NewGuid().ToString());

            var user = new ApplicationUser
            {
                UserName = "student1@itelectivesso.local",
                Email = "student1@itelectivesso.local",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user, "Passw0rd");

            var created = await userManager.FindByEmailAsync("student1@itelectivesso.local");

            Assert.True(result.Succeeded);
            Assert.NotNull(created);
            Assert.True(created!.IsActive);
        }

        [Fact]
        public async Task CreateUser_Rejected_WhenEmailAlreadyExists()
        {
            var userManager = BuildUserManager(Guid.NewGuid().ToString());

            var firstUser = new ApplicationUser
            {
                UserName = "duplicate@itelectivesso.local",
                Email = "duplicate@itelectivesso.local",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var secondUser = new ApplicationUser
            {
                UserName = "duplicate@itelectivesso.local",
                Email = "duplicate@itelectivesso.local",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var firstResult = await userManager.CreateAsync(firstUser, "Passw0rd");
            var secondResult = await userManager.CreateAsync(secondUser, "Passw0rd");

            Assert.True(firstResult.Succeeded);
            Assert.False(secondResult.Succeeded);
        }
    }
}