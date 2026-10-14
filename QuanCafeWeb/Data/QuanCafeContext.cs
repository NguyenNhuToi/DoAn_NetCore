using Microsoft.EntityFrameworkCore;
using QuanCafeWeb.Models;

namespace QuanCafeWeb.Data
{
    public class QuanCafeContext : DbContext
    {
        public QuanCafeContext(DbContextOptions<QuanCafeContext> options) : base(options) { }

        // ===== DANH SÁCH CÁC BẢNG (DbSet) =====
        public DbSet<DanhMucMon> DanhMucMons { get; set; }
        public DbSet<Mon> Mons { get; set; }
        public DbSet<Ban> Bans { get; set; }
        public DbSet<TaiKhoan> TaiKhoans { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<ChiTietOrder> ChiTietOrders { get; set; }
        public DbSet<DatBan> DatBans { get; set; }             
        public DbSet<DanhGia> DanhGias { get; set; }            
        public DbSet<CaLamViec> CaLamViecs { get; set; }         

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ===== MAP TÊN BẢNG VỚI SQL SERVER =====
            modelBuilder.Entity<DanhMucMon>().ToTable("DanhMucMon");
            modelBuilder.Entity<Mon>().ToTable("Mon");
            modelBuilder.Entity<Ban>().ToTable("Ban");
            modelBuilder.Entity<TaiKhoan>().ToTable("TaiKhoan");
            modelBuilder.Entity<Order>().ToTable("Order");
            modelBuilder.Entity<ChiTietOrder>().ToTable("ChiTietOrder");
            modelBuilder.Entity<DatBan>().ToTable("DatBan");         
            modelBuilder.Entity<DanhGia>().ToTable("DanhGia");        
            modelBuilder.Entity<CaLamViec>().ToTable("CaLamViec");    

            // ===== KHÓA CHÍNH PHỨC HỢP =====
            modelBuilder.Entity<ChiTietOrder>()
                .HasKey(c => new { c.MaOrder, c.MaMon });

            // ===== DECIMAL PRECISION =====
            modelBuilder.Entity<Mon>()
                .Property(m => m.GiaBan).HasPrecision(18, 2);
            modelBuilder.Entity<Order>()
                .Property(o => o.TongTien).HasPrecision(18, 2);
            modelBuilder.Entity<ChiTietOrder>()
                .Property(c => c.DonGia).HasPrecision(18, 2);

            // ===== CHECK CONSTRAINT =====
            modelBuilder.Entity<DanhGia>()
                .ToTable(t => t.HasCheckConstraint("CK_DanhGia_SoSao", "[SoSao] BETWEEN 1 AND 5"));

            base.OnModelCreating(modelBuilder);
        }
    }
}