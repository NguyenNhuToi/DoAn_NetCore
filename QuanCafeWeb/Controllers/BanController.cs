using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanCafeWeb.Data;
using QuanCafeWeb.Models;
using X.PagedList;
using X.PagedList.Extensions;

namespace QuanCafeWeb.Controllers
{
    public class BanController : Controller
    {
        private readonly QuanCafeContext _context;
        public BanController(QuanCafeContext context) => _context = context;

        public async Task<IActionResult> Index(string? searchString, int? page)
        {
            var query = _context.Bans.AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchString))
                query = query.Where(b => b.TenBan.Contains(searchString));

            ViewBag.SearchString = searchString;
            var list = await query.OrderBy(b => b.MaBan).ToListAsync();
            var paged = list.ToPagedList(page ?? 1, 5);
            return View(paged);
        }

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
            if (ban != null) _context.Bans.Remove(ban);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Xóa thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}