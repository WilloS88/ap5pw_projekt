// Seed/DbSeeder.cs
using AP5PW_Helpdesk.Data;
using AP5PW_Helpdesk.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AP5PW_Helpdesk.Seed
{
	public static class DbSeeder
	{
		public static async Task SeedAsync(IServiceProvider services)
		{
			using var scope		= services.CreateScope();
			var db				= scope.ServiceProvider.GetRequiredService<Data.AppDbContext>();
			var hasher			= scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

			await db.Database.MigrateAsync();

			// Roles
			var roleNames = new[] { Roles.Admin, Roles.Moderator, Roles.User };
			foreach (var r in roleNames)
			{
				if (!await db.Roles.AnyAsync(x => x.Name == r))
					db.Roles.Add(new Role { Name = r });
			}
			await db.SaveChangesAsync();

			// Admin user
			if (!await db.Users.Include(u => u.Role).AnyAsync(u => u.Role.Name == Roles.Admin))
			{
				var adminRole = await db.Roles.FirstAsync(r => r.Name == Roles.Admin);
				var admin = new User
				{
					UserName = "admin",
					RoleId = adminRole.Id,
					Password = ""
				};
				admin.Password = hasher.HashPassword(admin, "admin123"); // change after first login
				db.Users.Add(admin);
				await db.SaveChangesAsync();
			}
		}
	}
}
