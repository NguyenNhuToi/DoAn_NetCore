using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanCafeWeb.Data;
using QuanCafeWeb.Models;
using X.PagedList;
using X.PagedList.Extensions;

namespace QuanCafeWeb.Controllers
{
    public class DanhMucMonController : Controller
    {
        private readonly QuanCafeContext _context;
        public DanhMucMonController(QuanCafeContext context) => _context = context;

        // GET: /DanhMucMon
        public async Task<IActionResult> Index(string? searchString, int? page)
        {
            var query = _context.DanhMucMons.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
                query = query.Where(d => d.TenDM.Contains(searchString));

            int pageSize = 5;
            int pageNumber = page ?? 1;

            ViewBag.SearchString = searchString;
            var list = await query.OrderBy(d => d.MaDM).ToListAsync();
            var paged = list.ToPagedList(pageNumber, pageSize);
            return View(paged);
        }

        // GET: /DanhMucMon/Create
        public IActionResult Create() => View();

        // POST: /DanhMucMon/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
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

        // GET: /DanhMucMon/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var dm = await _context.DanhMucMons.FindAsync(id);
            if (dm == null) return NotFound();
            return View(dm);
        }

        // POST: /DanhMucMon/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
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

        // GET: /DanhMucMon/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var dm = await _context.DanhMucMons.FindAsync(id);
            if (dm == null) return NotFound();
            return View(dm);
        }

        // POST: /DanhMucMon/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dm = await _context.DanhMucMons.FindAsync(id);
            if (dm != null) _context.DanhMucMons.Remove(dm);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Xóa thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}