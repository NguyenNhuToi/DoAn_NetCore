using Microsoft.EntityFrameworkCore;
using QuanCafeWeb.Models;

namespace QuanCafeWeb.Data
{
    public class QuanCafeContext : DbContext
    {
        public QuanCafeContext(DbContextOptions<QuanCafeContext> options) : base(options) { }

        public DbSet<DanhMucMon> DanhMucMons { get; set; }
        public DbSet<Mon> Mons { get; set; }
        public DbSet<Ban> Bans { get; set; }
        public DbSet<TaiKhoan> TaiKhoans { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<ChiTietOrder> ChiTietOrders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ✅ MAP TÊN BẢNG TRONG DB (số ít) VỚI DBSET TRONG C# (số nhiều)
            modelBuilder.Entity<DanhMucMon>().ToTable("DanhMucMon");
            modelBuilder.Entity<Mon>().ToTable("Mon");
            modelBuilder.Entity<Ban>().ToTable("Ban");
            modelBuilder.Entity<TaiKhoan>().ToTable("TaiKhoan");
            modelBuilder.Entity<Order>().ToTable("Order");
            modelBuilder.Entity<ChiTietOrder>().ToTable("ChiTietOrder");

            // Khóa chính phức hợp cho ChiTietOrder
            modelBuilder.Entity<ChiTietOrder>()
                .HasKey(c => new { c.MaOrder, c.MaMon });

            // Decimal precision
            modelBuilder.Entity<Mon>()
                .Property(m => m.GiaBan).HasPrecision(18, 2);
            modelBuilder.Entity<Order>()
                .Property(o => o.TongTien).HasPrecision(18, 2);
            modelBuilder.Entity<ChiTietOrder>()
                .Property(c => c.DonGia).HasPrecision(18, 2);

            base.OnModelCreating(modelBuilder);
        }
    }
}