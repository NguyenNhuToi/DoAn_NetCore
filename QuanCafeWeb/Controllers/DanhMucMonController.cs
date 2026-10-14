using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanCafeWeb.Data;
using QuanCafeWeb.Filters;
using QuanCafeWeb.Models;
using X.PagedList;
using X.PagedList.Extensions;

namespace QuanCafeWeb.Controllers
{
    [AuthorizeRole("QuanTri")]  // ⭐ CHỈ QUẢN TRỊ MỚI VÀO ĐƯỢC
    public class DanhMucMonController : Controller
    {
        private readonly QuanCafeContext _context;
        public DanhMucMonController(QuanCafeContext context) => _context = context;

        // ===== DANH SÁCH + TÌM KIẾM + SẮP XẾP =====
        public async Task<IActionResult> Index(string? searchString, string? sortOrder, int? page)
        {
            var query = _context.DanhMucMons.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
                query = query.Where(d => d.TenDM.Contains(searchString));

            // ⭐ Sắp xếp
            ViewBag.CurrentSort = sortOrder;
            ViewBag.SortTenAsc = string.IsNullOrEmpty(sortOrder) ? "ten_desc" : "";
            ViewBag.SortThuTuAsc = sortOrder == "thutu_asc" ? "thutu_desc" : "thutu_asc";

            query = sortOrder switch
            {
                "ten_asc" => query.OrderBy(d => d.TenDM),
                "ten_desc" => query.OrderByDescending(d => d.TenDM),
                "thutu_asc" => query.OrderBy(d => d.ThuTu),
                "thutu_desc" => query.OrderByDescending(d => d.ThuTu),
                _ => query.OrderBy(d => d.MaDM)
            };

            ViewBag.SearchString = searchString;

            int pageSize = 5;
            int pageNumber = page ?? 1;

            var list = await query.ToListAsync();
            var paged = list.ToPagedList(pageNumber, pageSize);
            return View(paged);
        }

        // ===== CREATE =====
        public IActionResult Create() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DanhMucMon dm)
        {
            if (ModelState.IsValid)
            {
                _context.Add(dm);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm danh mục thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(dm);
        }

        // ===== EDIT =====
        public async Task<IActionResult> Edit(int id)
        {
            var dm = await _context.DanhMucMons.FindAsync(id);
            if (dm == null) return NotFound();
            return View(dm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DanhMucMon dm)
        {
            if (id != dm.MaDM) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(dm);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(dm);
        }

        // ===== DELETE =====
        public async Task<IActionResult> Delete(int id)
        {
            var dm = await _context.DanhMucMons.FindAsync(id);
            if (dm == null) return NotFound();
            return View(dm);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dm = await _context.DanhMucMons.FindAsync(id);
            if (dm == null) return NotFound();

            // ⭐ KIỂM TRA: Còn món nào thuộc danh mục này không?
            bool conMon = await _context.Mons.AnyAsync(m => m.MaDM == id);
            if (conMon)
            {
                TempData["Error"] = "❌ Không thể xóa! Còn món thuộc danh mục này.";
                return RedirectToAction(nameof(Index));
            }

            _context.DanhMucMons.Remove(dm);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Xóa thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}