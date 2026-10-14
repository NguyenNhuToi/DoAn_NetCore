using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanCafeWeb.Data;
using QuanCafeWeb.Filters;
using QuanCafeWeb.Models;

namespace QuanCafeWeb.Controllers
{
    [AuthorizeRole("QuanTri")]
    public class CaLamViecController : Controller
    {
        private readonly QuanCafeContext _context;
        public CaLamViecController(QuanCafeContext context) => _context = context;

        // ===== DANH SÁCH CA =====
        public async Task<IActionResult> Index(int? maNV, string? trangThai, DateTime? tuNgay, DateTime? denNgay)
        {
            var query = _context.CaLamViecs.Include(c => c.TaiKhoan).AsQueryable();

            if (maNV.HasValue && maNV.Value > 0)
                query = query.Where(c => c.MaTK == maNV.Value);

            if (trangThai == "danglam")
                query = query.Where(c => c.GioRa == null);
            else if (trangThai == "daxong")
                query = query.Where(c => c.GioRa != null);

            if (tuNgay.HasValue)
                query = query.Where(c => c.GioVao.Date >= tuNgay.Value.Date);
            if (denNgay.HasValue)
                query = query.Where(c => c.GioVao.Date <= denNgay.Value.Date);

            ViewBag.NhanViens = new SelectList(
                await _context.TaiKhoans.Where(t => t.VaiTro != "KhachHang").ToListAsync(),
                "MaTK", "HoTen", maNV);
            ViewBag.CurrentMaNV = maNV;
            ViewBag.TrangThai = trangThai;
            ViewBag.TuNgay = tuNgay?.ToString("yyyy-MM-dd");
            ViewBag.DenNgay = denNgay?.ToString("yyyy-MM-dd");

            var list = await query.OrderByDescending(c => c.GioVao).ToListAsync();
            return View(list);
        }

        // ===== CHECK-IN =====
        [HttpGet]
        public async Task<IActionResult> CheckIn()
        {
            ViewBag.NhanViens = new SelectList(
                await _context.TaiKhoans
                    .Where(t => t.VaiTro != "KhachHang" && t.TrangThai)
                    .ToListAsync(),
                "MaTK", "HoTen");
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckIn(int maTK, string? ghiChu)
        {
            bool dangLam = await _context.CaLamViecs
                .AnyAsync(c => c.MaTK == maTK && c.GioRa == null);

            if (dangLam)
            {
                TempData["Error"] = "🚫 Nhân viên này đang có ca mở, phải check-out trước!";
                return RedirectToAction(nameof(Index));
            }

            var ca = new CaLamViec
            {
                MaTK = maTK,
                GioVao = DateTime.Now,
                GhiChu = ghiChu
            };
            _context.CaLamViecs.Add(ca);
            await _context.SaveChangesAsync();
            TempData["Success"] = "✅ Check-in thành công!";
            return RedirectToAction(nameof(Index));
        }
        // ===== CHECK-OUT =====
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckOut(int id)
        {
            var ca = await _context.CaLamViecs.FindAsync(id);
            if (ca != null && ca.GioRa == null)
            {
                ca.GioRa = DateTime.Now;
                await _context.SaveChangesAsync();
                TempData["Success"] = "✅ Check-out thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        // ===== XÓA CA =====
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Xoa(int id)
        {
            var ca = await _context.CaLamViecs.FindAsync(id);
            if (ca != null)
            {
                _context.CaLamViecs.Remove(ca);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa ca làm việc!";
            }
            return RedirectToAction(nameof(Index));
        }

        // ===== THỐNG KÊ =====
        public async Task<IActionResult> ThongKe(DateTime? tuNgay, DateTime? denNgay)
        {
            if (!tuNgay.HasValue) tuNgay = DateTime.Today.AddDays(-30);
            if (!denNgay.HasValue) denNgay = DateTime.Today;

            var rawData = await _context.CaLamViecs
                .Include(c => c.TaiKhoan)
                .Where(c => c.GioVao.Date >= tuNgay.Value.Date
                         && c.GioVao.Date <= denNgay.Value.Date
                         && c.GioRa != null)
                .ToListAsync();

            var thongKe = rawData
                .GroupBy(c => c.TaiKhoan?.HoTen ?? "N/A")
                .Select(g => new
                {
                    HoTen = g.Key,
                    SoCa = g.Count(),
                    TongGio = g.Sum(c => (c.GioRa!.Value - c.GioVao).TotalHours)
                })
                .OrderByDescending(x => x.TongGio)
                .ToList();

            ViewBag.TuNgay = tuNgay.Value.ToString("yyyy-MM-dd");
            ViewBag.DenNgay = denNgay.Value.ToString("yyyy-MM-dd");
            return View(thongKe);
        }
    }
}
