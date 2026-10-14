using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanCafeWeb.Data;
using QuanCafeWeb.Models;

namespace QuanCafeWeb.Controllers
{
    public class DanhGiaController : Controller
    {
        private readonly QuanCafeContext _context;
        public DanhGiaController(QuanCafeContext context) => _context = context;

        // ===== DANH SÁCH ĐÁNH GIÁ (Quản trị xem tất cả) =====
        public async Task<IActionResult> Index(int? maMon)
        {
            var query = _context.DanhGias.Include(d => d.Mon).AsQueryable();

            if (maMon.HasValue)
                query = query.Where(d => d.MaMon == maMon.Value);

            ViewBag.MaMon = maMon;
            ViewBag.Mons = await _context.Mons.ToListAsync();

            var list = await query.OrderByDescending(d => d.NgayTao).ToListAsync();
            return View(list);
        }

        // ===== FORM ĐÁNH GIÁ (Khách hàng - không cần đăng nhập) =====
        [HttpGet]
        public async Task<IActionResult> Create(int maMon)
        {
            var mon = await _context.Mons.FindAsync(maMon);
            if (mon == null) return NotFound();

            // Kiểm tra đã đánh giá chưa (dựa trên Session/IP)
            ViewBag.Mon = mon;
            return View(new DanhGia { MaMon = maMon, SoSao = 5 });
        }

        // ===== XỬ LÝ ĐÁNH GIÁ =====
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DanhGia danhGia)
        {
            if (ModelState.IsValid)
            {
                danhGia.NgayTao = DateTime.Now;
                _context.DanhGias.Add(danhGia);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cảm ơn bạn đã đánh giá!";
                return RedirectToAction(nameof(CamOn));
            }

            ViewBag.Mon = await _context.Mons.FindAsync(danhGia.MaMon);
            return View(danhGia);
        }

        public IActionResult CamOn() => View();

        // ===== XÓA ĐÁNH GIÁ (Chỉ Quản trị) =====
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var dg = await _context.DanhGias.FindAsync(id);
            if (dg != null)
            {
                _context.DanhGias.Remove(dg);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa đánh giá!";
            }
            return RedirectToAction(nameof(Index));
        }

        // ===== XEM ĐÁNH GIÁ CỦA 1 MÓN (public, cho khách) =====
        public async Task<IActionResult> XemDanhGia(int maMon)
        {
            var mon = await _context.Mons.FindAsync(maMon);
            if (mon == null) return NotFound();

            var list = await _context.DanhGias
                .Where(d => d.MaMon == maMon)
                .OrderByDescending(d => d.NgayTao)
                .ToListAsync();

            ViewBag.Mon = mon;
            ViewBag.SoSaoTrungBinh = list.Any() ? list.Average(d => d.SoSao) : 0;
            ViewBag.TongDanhGia = list.Count;

            return View(list);
        }
    }
}