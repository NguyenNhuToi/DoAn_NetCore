using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanCafeWeb.Data;

namespace QuanCafeWeb.Controllers
{
    public class ReportController : Controller
    {
        private readonly QuanCafeContext _context;

        public ReportController(QuanCafeContext context) => _context = context;

        // GET: /Report
        public async Task<IActionResult> Index()
        {
            // ===== 1. Thống kê tổng quan =====
            var homNay = DateTime.Today;
            var dauThang = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            ViewBag.DoanhThuHomNay = await _context.Orders
                .Where(o => o.GioVao.Date == homNay)
                .SumAsync(o => (decimal?)o.TongTien) ?? 0;

            ViewBag.DoanhThuThang = await _context.Orders
                .Where(o => o.GioVao >= dauThang)
                .SumAsync(o => (decimal?)o.TongTien) ?? 0;

            ViewBag.TongDonHang = await _context.Orders.CountAsync();

            // ===== 2. Doanh thu 7 ngày gần nhất =====
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

            // ===== 3. Top 5 món bán chạy =====
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

            return View();
        }
    }
}