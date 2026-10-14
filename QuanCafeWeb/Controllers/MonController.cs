using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanCafeWeb.Data;
using QuanCafeWeb.Models;
using X.PagedList;
using X.PagedList.Extensions;

namespace QuanCafeWeb.Controllers
{
    public class MonController : Controller
    {
        private readonly QuanCafeContext _context;
        private readonly IWebHostEnvironment _env;

        public MonController(QuanCafeContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: /Mon?searchString=&maDM=&giaMin=&giaMax=&sortOrder=&page=
        public async Task<IActionResult> Index(
            string? searchString,
            int? maDM,
            decimal? giaMin,
            decimal? giaMax,
            string? sortOrder,
            int? page)
        {
            var query = _context.Mons.Include(m => m.DanhMucMon).AsQueryable();

            // ===== 1. TÌM KIẾM THEO TÊN =====
            if (!string.IsNullOrWhiteSpace(searchString))
                query = query.Where(m => m.TenMon.Contains(searchString));

            // ===== 2. LỌC THEO DANH MỤC =====
            if (maDM.HasValue && maDM.Value > 0)
                query = query.Where(m => m.MaDM == maDM.Value);

            // ===== 3. LỌC THEO KHOẢNG GIÁ =====
            if (giaMin.HasValue)
                query = query.Where(m => m.GiaBan >= giaMin.Value);
            if (giaMax.HasValue)
                query = query.Where(m => m.GiaBan <= giaMax.Value);

            // ===== 4. SẮP XẾP =====
            ViewBag.CurrentSort = sortOrder;
            ViewBag.SortTenAsc = string.IsNullOrEmpty(sortOrder) ? "ten_desc" : "";
            ViewBag.SortGiaAsc = sortOrder == "gia_asc" ? "gia_desc" : "gia_asc";
            ViewBag.SortNgayAsc = sortOrder == "ngay_asc" ? "ngay_desc" : "ngay_asc";

            query = sortOrder switch
            {
                "ten_asc" => query.OrderBy(m => m.TenMon),
                "ten_desc" => query.OrderByDescending(m => m.TenMon),
                "gia_asc" => query.OrderBy(m => m.GiaBan),
                "gia_desc" => query.OrderByDescending(m => m.GiaBan),
                "ngay_asc" => query.OrderBy(m => m.MaMon),
                "ngay_desc" => query.OrderByDescending(m => m.MaMon),
                _ => query.OrderBy(m => m.MaMon)
            };

            // ===== 5. LƯU TRẠNG THÁI FILTER VÀO VIEWBAG =====
            ViewBag.SearchString = searchString;
            ViewBag.MaDM = new SelectList(_context.DanhMucMons, "MaDM", "TenDM", maDM);
            ViewBag.CurrentMaDM = maDM;
            ViewBag.GiaMin = giaMin;
            ViewBag.GiaMax = giaMax;

            // ===== 6. PHÂN TRANG =====
            int pageSize = 5;
            int pageNumber = page ?? 1;

            var list = await query.ToListAsync();
            var paged = list.ToPagedList(pageNumber, pageSize);

            return View(paged);
        }

        // GET: /Mon/Create
        public IActionResult Create()
        {
            ViewBag.MaDM = new SelectList(_context.DanhMucMons, "MaDM", "TenDM");
            return View();
        }

        // POST: /Mon/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Mon mon, IFormFile? fileAnh)
        {
            if (ModelState.IsValid)
            {
                if (fileAnh != null && fileAnh.Length > 0)
                {
                    string fileName = Guid.NewGuid() + Path.GetExtension(fileAnh.FileName);
                    string folder = Path.Combine(_env.WebRootPath, "images");
                    Directory.CreateDirectory(folder);
                    string path = Path.Combine(folder, fileName);

                    using (var fs = new FileStream(path, FileMode.Create))
                        await fileAnh.CopyToAsync(fs);

                    mon.AnhMoi = fileName;
                }

                _context.Add(mon);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm món thành công!";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.MaDM = new SelectList(_context.DanhMucMons, "MaDM", "TenDM", mon.MaDM);
            return View(mon);
        }

        // GET: /Mon/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var mon = await _context.Mons.FindAsync(id);
            if (mon == null) return NotFound();
            ViewBag.MaDM = new SelectList(_context.DanhMucMons, "MaDM", "TenDM", mon.MaDM);
            return View(mon);
        }

        // POST: /Mon/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Mon mon, IFormFile? fileAnh)
        {
            if (id != mon.MaMon) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    if (fileAnh != null && fileAnh.Length > 0)
                    {
                        string fileName = Guid.NewGuid() + Path.GetExtension(fileAnh.FileName);
                        string path = Path.Combine(_env.WebRootPath, "images", fileName);
                        using (var fs = new FileStream(path, FileMode.Create))
                            await fileAnh.CopyToAsync(fs);
                        mon.AnhMoi = fileName;
                    }
                    _context.Update(mon);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Mons.Any(e => e.MaMon == id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.MaDM = new SelectList(_context.DanhMucMons, "MaDM", "TenDM", mon.MaDM);
            return View(mon);
        }

        // GET: /Mon/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var mon = await _context.Mons.Include(m => m.DanhMucMon).FirstOrDefaultAsync(m => m.MaMon == id);
            if (mon == null) return NotFound();
            return View(mon);
        }

        // POST: /Mon/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var mon = await _context.Mons.FindAsync(id);
            if (mon != null) _context.Mons.Remove(mon);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Xóa thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}