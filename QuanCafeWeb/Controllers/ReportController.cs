using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanCafeWeb.Data;
using QuanCafeWeb.Filters;

namespace QuanCafeWeb.Controllers
{
    [AuthorizeRole("QuanTri")]  // ⭐ CHỈ QUẢN TRỊ MỚI VÀO ĐƯỢC BÁO CÁO
    public class ReportController : Controller
    {
        private readonly QuanCafeContext _context;

        public ReportController(QuanCafeContext context) => _context = context;

        // GET: /Report
        public async Task<IActionResult> Index()
        {
            var homNay = DateTime.Today;
            var dauTuan = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            var dauThang = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var dauNam = new DateTime(DateTime.Now.Year, 1, 1);

            // ===== 1. DOANH THU TỔNG QUAN =====
            ViewBag.DoanhThuHomNay = await _context.Orders
                .Where(o => o.GioVao.Date == homNay)
                .SumAsync(o => (decimal?)o.TongTien) ?? 0;

            ViewBag.DoanhThuTuan = await _context.Orders
                .Where(o => o.GioVao >= dauTuan)
                .SumAsync(o => (decimal?)o.TongTien) ?? 0;

            ViewBag.DoanhThuThang = await _context.Orders
                .Where(o => o.GioVao >= dauThang)
                .SumAsync(o => (decimal?)o.TongTien) ?? 0;

            ViewBag.DoanhThuNam = await _context.Orders
                .Where(o => o.GioVao >= dauNam)
                .SumAsync(o => (decimal?)o.TongTien) ?? 0;

            // ===== 2. THỐNG KÊ ĐƠN HÀNG =====
            ViewBag.TongDonHang = await _context.Orders.CountAsync();
            ViewBag.DonHomNay = await _context.Orders.CountAsync(o => o.GioVao.Date == homNay);
            ViewBag.DonThang = await _context.Orders.CountAsync(o => o.GioVao >= dauThang);

            // ===== 3. DOANH THU 7 NGÀY GẦN NHẤT =====
            var labels = new List<string>();
            var data = new List<decimal>();

            for (int i = 6; i >= 0; i--)
            {
                var ngay = DateTime.Today.AddDays(-i);
                labels.Add(ngay.ToString("dd/MM"));

                var dt = await _context.Orders
                    .Where(o => o.GioVao.Date == ngay)
                    .SumAsync(o => (decimal?)o.TongTien) ?? 0;
                data.Add(dt);
            }

            ViewBag.Labels = labels;
            ViewBag.Data = data;

            // ===== 4. TOP 5 MÓN BÁN CHẠY =====
            var topMon = await _context.ChiTietOrders
                .Include(c => c.Mon)
                .GroupBy(c => c.Mon!.TenMon)
                .Select(g => new
                {
                    TenMon = g.Key,
                    SoLuong = g.Sum(x => x.SoLuong)
                })
                .OrderByDescending(x => x.SoLuong)
                .Take(5)
                .ToListAsync();

            ViewBag.TopMonLabels = topMon.Select(x => x.TenMon).ToList();
            ViewBag.TopMonData = topMon.Select(x => x.SoLuong).ToList();

            // ===== 5. TOP 5 BÀN DOANH THU CAO NHẤT =====
            var topBan = await _context.Orders
                .Where(o => o.MaBan != null)
                .Include(o => o.Ban)
                .GroupBy(o => o.Ban!.TenBan)
                .Select(g => new
                {
                    TenBan = g.Key,
                    TongTien = g.Sum(x => x.TongTien)
                })
                .OrderByDescending(x => x.TongTien)
                .Take(5)
                .ToListAsync();

            ViewBag.TopBanLabels = topBan.Select(x => x.TenBan).ToList();
            ViewBag.TopBanData = topBan.Select(x => x.TongTien).ToList();

            // ===== 6. DOANH THU THEO LOẠI ORDER =====
            ViewBag.DoanhThuTaiCho = await _context.Orders
                .Where(o => o.LoaiOrder == "Tại chỗ")
                .SumAsync(o => (decimal?)o.TongTien) ?? 0;

            ViewBag.DoanhThuMangDi = await _context.Orders
                .Where(o => o.LoaiOrder == "Mang đi")
                .SumAsync(o => (decimal?)o.TongTien) ?? 0;

            return View();
        }
    }
}