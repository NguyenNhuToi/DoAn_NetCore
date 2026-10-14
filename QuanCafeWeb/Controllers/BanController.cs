using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanCafeWeb.Data;
using QuanCafeWeb.Filters;
using QuanCafeWeb.Models;
using X.PagedList;
using X.PagedList.Extensions;

namespace QuanCafeWeb.Controllers
{
    [AuthorizeRole("QuanTri", "NhanVien")] 
    public class BanController : Controller
    {
        private readonly QuanCafeContext _context;
        public BanController(QuanCafeContext context) => _context = context;

        // ===== DANH SÁCH + TÌM KIẾM + LỌC TRẠNG THÁI + SẮP XẾP =====
        public async Task<IActionResult> Index(
            string? searchString,
            string? khuVuc,
            string? trangThai,
            string? sortOrder,
            int? page)
        {
            var query = _context.Bans.AsQueryable();

            // Tìm theo tên
            if (!string.IsNullOrWhiteSpace(searchString))
                query = query.Where(b => b.TenBan.Contains(searchString));

            // ⭐ Lọc theo khu vực
            if (!string.IsNullOrWhiteSpace(khuVuc))
                query = query.Where(b => b.KhuVuc == khuVuc);

            // ⭐ Lọc theo trạng thái
            if (!string.IsNullOrWhiteSpace(trangThai))
                query = query.Where(b => b.TrangThai == trangThai);

            // ⭐ Sắp xếp
            ViewBag.CurrentSort = sortOrder;
            ViewBag.SortTenAsc = string.IsNullOrEmpty(sortOrder) ? "ten_desc" : "";
            ViewBag.SortSoGheAsc = sortOrder == "ghe_asc" ? "ghe_desc" : "ghe_asc";

            query = sortOrder switch
            {
                "ten_asc" => query.OrderBy(b => b.TenBan),
                "ten_desc" => query.OrderByDescending(b => b.TenBan),
                "ghe_asc" => query.OrderBy(b => b.SoGhe),
                "ghe_desc" => query.OrderByDescending(b => b.SoGhe),
                _ => query.OrderBy(b => b.MaBan)
            };

            // Lưu filter vào ViewBag
            ViewBag.SearchString = searchString;
            ViewBag.KhuVuc = khuVuc;
            ViewBag.TrangThai = trangThai;

            // Danh sách khu vực (dùng cho dropdown filter)
            ViewBag.DanhSachKhuVuc = await _context.Bans
                .Where(b => !string.IsNullOrEmpty(b.KhuVuc))
                .Select(b => b.KhuVuc)
                .Distinct()
                .ToListAsync();

            var list = await query.ToListAsync();
            var paged = list.ToPagedList(page ?? 1, 8);  // 8 bàn/trang
            return View(paged);
        }

        // ===== CREATE =====
        public IActionResult Create() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Ban ban)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(ban.TrangThai)) ban.TrangThai = "Trống";
                _context.Add(ban);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm bàn thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(ban);
        }

        // ===== EDIT =====
        public async Task<IActionResult> Edit(int id)
        {
            var ban = await _context.Bans.FindAsync(id);
            if (ban == null) return NotFound();
            return View(ban);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Ban ban)
        {
            if (id != ban.MaBan) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(ban);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(ban);
        }

        // ===== DELETE =====
        public async Task<IActionResult> Delete(int id)
        {
            var ban = await _context.Bans.FindAsync(id);
            if (ban == null) return NotFound();
            return View(ban);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ban = await _context.Bans.FindAsync(id);
            if (ban == null) return NotFound();

            // ⭐ KIỂM TRA: Không cho xóa bàn đang có Order
            bool coOrder = await _context.Orders.AnyAsync(o => o.MaBan == id);
            if (coOrder)
            {
                TempData["Error"] = "Không thể xóa! Bàn này đã có đơn hàng.";
                return RedirectToAction(nameof(Index));
            }

            // ⭐ Không cho xóa bàn đang phục vụ
            if (ban.TrangThai == "Đang phục vụ")
            {
                TempData["Error"] = "Không thể xóa! Bàn đang phục vụ.";
                return RedirectToAction(nameof(Index));
            }

            _context.Bans.Remove(ban);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Xóa thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}