using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using AP5PW_Helpdesk.Controllers;
using AP5PW_Helpdesk.Data;
using AP5PW_Helpdesk.Data.Repositories;
using AP5PW_Helpdesk.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AP5PW_Helpdesk.Tests
{
	public class CompaniesControllerTests
	{
		private static AppDbContext GetInMemoryDbContext()
		{
			var options = new DbContextOptionsBuilder<AppDbContext>()
				.UseInMemoryDatabase(databaseName: "Helpdesk_TestDB")
				.Options;
			return new AppDbContext(options);
		}

		[Fact]
		public async Task Index_ReturnsViewWithCompanies()
		{
			var db = GetInMemoryDbContext();
			db.Companies.AddRange(new List<Company>
			{
				new() { Id = 1,		Name = "TechCorp",		City = "Brno",	Street = "Hlavni 1",	Postcode = "60200" },
				new() { Id = 2,		Name = "FoodMaster",	City = "Praha", Street = "Nova 12",		Postcode = "11000" }
			});
			await db.SaveChangesAsync();

			var logger		= NullLogger<CompaniesController>.Instance;

			var repo		= new CompanyRepository(db);
			var controller	= new CompaniesController(repo, logger);

			var result = await controller.Index();

			var viewResult	= Assert.IsType<ViewResult>(result);
			var model		= Assert.IsType<List<AP5PW_Helpdesk.ViewModels.CompanyVM>>(viewResult.Model, exactMatch: false);

			Assert.Equal(2, model.Count);
			Assert.Contains(model, c => c.Name == "TechCorp");
			Assert.Contains(model, c => c.City == "Praha");
		}
	}
}
