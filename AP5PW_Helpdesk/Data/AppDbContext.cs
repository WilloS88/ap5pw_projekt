using AP5PW_Helpdesk.Entities;
using Microsoft.EntityFrameworkCore;

namespace AP5PW_Helpdesk.Data

{
	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

		public DbSet<Company> Companies => Set<Company>();
		public DbSet<Role> Roles => Set<Role>();
		public DbSet<User> Users => Set<User>();
		public DbSet<Goods> Goods => Set<Goods>();
		public DbSet<Order> Orders => Set<Order>();
		public DbSet<OrderedGoods> OrderedGoods => Set<OrderedGoods>();
		public DbSet<Warehouse> Warehouses => Set<Warehouse>();
		public DbSet<WarehouseGood> WarehousesGoods => Set<WarehouseGood>();

		protected override void OnModelCreating(ModelBuilder m)
		{
			base.OnModelCreating(m);

			// unique username
			m.Entity<User>()
				.HasIndex(u => u.UserName)
				.IsUnique();

			// money precision
			m.Entity<Goods>()
				.Property(g => g.Price)
				.HasColumnType("decimal(18,2)");

			// User -> Role (many-to-one)
			m.Entity<User>()
				.HasOne(u => u.Role)
				.WithMany(r => r.Users)
				.HasForeignKey(u => u.RoleId)
				.OnDelete(DeleteBehavior.Restrict);

			// User -> Company (many-to-one)
			m.Entity<User>()
				.HasOne(u => u.Company)
				.WithMany(c => c.Users)
				.HasForeignKey(u => u.CompanyId)
				.OnDelete(DeleteBehavior.Cascade);

			// Order -> User
			m.Entity<Order>()
				.HasOne(o => o.User)
				.WithMany(u => u.Orders)
				.HasForeignKey(o => o.UserId)
				.OnDelete(DeleteBehavior.Restrict);

			// Order -> Company
			m.Entity<Order>()
				.HasOne(o => o.Company)
				.WithMany(c => c.Orders)
				.HasForeignKey(o => o.CompanyId)
				.OnDelete(DeleteBehavior.Restrict);

			// OrderedGoods -> Order
			m.Entity<OrderedGoods>()
				.HasOne(og => og.Order)
				.WithMany(o => o.OrderedGoods)
				.HasForeignKey(og => og.OrderId)
				.OnDelete(DeleteBehavior.Cascade);

			// OrderedGoods -> Goods
			m.Entity<OrderedGoods>()
				.HasOne(og => og.Goods)
				.WithMany(g => g.OrderedGoods)
				.HasForeignKey(og => og.GoodsId)
				.OnDelete(DeleteBehavior.Restrict);

			// Warehouse -> Company
			m.Entity<Warehouse>()
				.HasOne(w => w.Company)
				.WithMany(c => c.Warehouses)
				.HasForeignKey(w => w.CompanyId)
				.OnDelete(DeleteBehavior.Cascade);

			// WarehousesGoods (link s extra sloupcem Quantity)
			m.Entity<WarehouseGood>()
				.HasOne(wg => wg.Warehouse)
				.WithMany(w => w.WarehousesGoods)
				.HasForeignKey(wg => wg.WarehouseId)
				.OnDelete(DeleteBehavior.Cascade);

			m.Entity<WarehouseGood>()
				.HasOne(wg => wg.Goods)
				.WithMany(g => g.WarehousesGoods)
				.HasForeignKey(wg => wg.GoodsId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}
