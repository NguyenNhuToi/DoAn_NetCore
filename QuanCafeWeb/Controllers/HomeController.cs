using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanCafeWeb.Data;
using QuanCafeWeb.Models;
using QuanCafeWeb.ViewModels;

namespace QuanCafeWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly QuanCafeContext _context;

        public HomeController(QuanCafeContext context) => _context = context;

        // ===== TRANG CHỦ (DASHBOARD) =====
        public async Task<IActionResult> Index()
        {
            var vm = new DashboardViewModel();

            // ===== 1. THỐNG KÊ SỐ LƯỢNG =====
            vm.SoMon = await _context.Mons.CountAsync();
            vm.SoBan = await _context.Bans.CountAsync();
            vm.SoOrder = await _context.Orders.CountAsync();
            vm.SoNhanVien = await _context.TaiKhoans.CountAsync(t => t.VaiTro == "NhanVien");

            // ===== 2. DOANH THU =====
            var homNay = DateTime.Today;
            var dauTuan = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            var dauThang = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var dauNam = new DateTime(DateTime.Now.Year, 1, 1);

            vm.DoanhThuHomNay = await _context.Orders
                .Where(o => o.GioVao.Date == homNay)
                .SumAsync(o => (decimal?)o.TongTien) ?? 0;

            vm.DoanhThuTuan = await _context.Orders
                .Where(o => o.GioVao >= dauTuan)
                .SumAsync(o => (decimal?)o.TongTien) ?? 0;

            vm.DoanhThuThang = await _context.Orders
                .Where(o => o.GioVao >= dauThang)
                .SumAsync(o => (decimal?)o.TongTien) ?? 0;

            vm.DoanhThuNam = await _context.Orders
                .Where(o => o.GioVao >= dauNam)
                .SumAsync(o => (decimal?)o.TongTien) ?? 0;

            // ===== 3. BIỂU ĐỒ 7 NGÀY =====
            for (int i = 6; i >= 0; i--)
            {
                var ngay = DateTime.Today.AddDays(-i);
                vm.Labels7Ngay.Add(ngay.ToString("dd/MM"));
                var dt = await _context.Orders
                    .Where(o => o.GioVao.Date == ngay)
                    .SumAsync(o => (decimal?)o.TongTien) ?? 0;
                vm.Data7Ngay.Add(dt);
            }

            // ===== 4. TOP 5 MÓN BÁN CHẠY =====
            var topMon = await _context.ChiTietOrders
                .Include(c => c.Mon)
                .GroupBy(c => c.Mon!.TenMon)
                .Select(g => new { TenMon = g.Key, SoLuong = g.Sum(x => x.SoLuong) })
                .OrderByDescending(x => x.SoLuong)
                .Take(5)
                .ToListAsync();

            vm.TopMonTen = topMon.Select(x => x.TenMon).ToList();
            vm.TopMonSoLuong = topMon.Select(x => x.SoLuong).ToList();

            // ===== 5. ĐƠN HÀNG GẦN ĐÂY =====
            vm.DonHangGanDay = await _context.Orders
                .Include(o => o.Ban)
                .OrderByDescending(o => o.GioVao)
                .Take(5)
                .ToListAsync();

            // ===== 6. BÀN ĐANG PHỤC VỤ =====
            vm.BanDangPhucVu = await _context.Bans
                .Where(b => b.TrangThai == "Đang phục vụ")
                .ToListAsync();

            return View(vm);
        }

        // ⭐ ===== XEM THỰC ĐƠN (KHÁCH HÀNG - KHÔNG CẦN LOGIN) =====
        public async Task<IActionResult> Menu(int? maDM, string? searchString)
        {
            var query = _context.Mons
                .Include(m => m.DanhMucMon)
                .Where(m => m.TrangThai);  // Chỉ hiện món còn bán

            // Lọc theo danh mục
            if (maDM.HasValue && maDM.Value > 0)
                query = query.Where(m => m.MaDM == maDM.Value);

            // Tìm theo tên
            if (!string.IsNullOrWhiteSpace(searchString))
                query = query.Where(m => m.TenMon.Contains(searchString));

            ViewBag.DanhMucs = await _context.DanhMucMons.ToListAsync();
            ViewBag.CurrentMaDM = maDM;
            ViewBag.SearchString = searchString;

            var mons = await query.OrderBy(m => m.MaDM).ToListAsync();
            return View(mons);
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}