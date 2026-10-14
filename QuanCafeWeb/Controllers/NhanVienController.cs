using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanCafeWeb.Data;
using QuanCafeWeb.Filters;
using QuanCafeWeb.Models;

namespace QuanCafeWeb.Controllers
{
    [AuthorizeRole("QuanTri")]
    public class NhanVienController : Controller
    {
        private readonly QuanCafeContext _context;
        public NhanVienController(QuanCafeContext context) => _context = context;

        // ===== DANH SÁCH NHÂN VIÊN =====
        public async Task<IActionResult> Index(string? searchString, string? vaiTro)
        {
            var query = _context.TaiKhoans
                .Where(t => t.VaiTro != "KhachHang")
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
                query = query.Where(t =>
                    (t.HoTen != null && t.HoTen.Contains(searchString)) ||
                    t.TenDangNhap.Contains(searchString));

            if (!string.IsNullOrWhiteSpace(vaiTro))
                query = query.Where(t => t.VaiTro == vaiTro);

            ViewBag.SearchString = searchString;
            ViewBag.VaiTro = vaiTro;

            var list = await query.OrderBy(t => t.VaiTro).ThenBy(t => t.HoTen).ToListAsync();
            return View(list);
        }

        // ===== THÊM NHÂN VIÊN =====
        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaiKhoan tk, string matKhauGoc)
        {
            bool trung = await _context.TaiKhoans.AnyAsync(t => t.TenDangNhap == tk.TenDangNhap);
            if (trung)
                ModelState.AddModelError("TenDangNhap", "Tên đăng nhập đã tồn tại!");

            if (string.IsNullOrWhiteSpace(matKhauGoc))
                ModelState.AddModelError("", "Vui lòng nhập mật khẩu!");

            if (ModelState.IsValid)
            {
                tk.MatKhau = BCrypt.Net.BCrypt.HashPassword(matKhauGoc);
                tk.TrangThai = true;
                _context.TaiKhoans.Add(tk);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm nhân viên thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(tk);
        }

        // ===== SỬA NHÂN VIÊN =====
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var tk = await _context.TaiKhoans.FindAsync(id);
            if (tk == null) return NotFound();
            return View(tk);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TaiKhoan tk, string? matKhauMoi)
        {
            if (id != tk.MaTK) return NotFound();

            var existing = await _context.TaiKhoans.FindAsync(id);
            if (existing == null) return NotFound();

            existing.HoTen = tk.HoTen;
            existing.VaiTro = tk.VaiTro;
            existing.TrangThai = tk.TrangThai;

            if (!string.IsNullOrWhiteSpace(matKhauMoi))
                existing.MatKhau = BCrypt.Net.BCrypt.HashPassword(matKhauMoi);

            await _context.SaveChangesAsync();
            TempData["Success"] = "Cập nhật nhân viên thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ===== KHÓA NHÂN VIÊN =====
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var tk = await _context.TaiKhoans.FindAsync(id);
            if (tk == null) return NotFound();

            if (id == HttpContext.Session.GetInt32("MaTK"))
            {
                TempData["Error"] = "🚫 Không thể khóa chính mình!";
                return RedirectToAction(nameof(Index));
            }
            return View(tk);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tk = await _context.TaiKhoans.FindAsync(id);
            if (tk == null) return NotFound();

            if (id == HttpContext.Session.GetInt32("MaTK"))
            {
                TempData["Error"] = "🚫 Không thể khóa chính mình!";
                return RedirectToAction(nameof(Index));
            }

            tk.TrangThai = false;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã khóa tài khoản nhân viên!";
            return RedirectToAction(nameof(Index));
        }

        // ===== MỞ KHÓA =====
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MoKhoa(int id)
        {
            var tk = await _context.TaiKhoans.FindAsync(id);
            if (tk != null)
            {
                tk.TrangThai = true;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã mở khóa!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
