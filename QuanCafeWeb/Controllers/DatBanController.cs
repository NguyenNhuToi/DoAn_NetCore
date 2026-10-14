using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanCafeWeb.Data;
using QuanCafeWeb.Filters;
using QuanCafeWeb.Models;

namespace QuanCafeWeb.Controllers
{
    public class DatBanController : Controller
    {
        private readonly QuanCafeContext _context;
        public DatBanController(QuanCafeContext context) => _context = context;

        // ===== DANH SÁCH ĐẶT BÀN (Quản trị + Nhân viên) =====
        [AuthorizeRole("QuanTri", "NhanVien")]
        public async Task<IActionResult> Index(string? searchString, string? trangThai, DateTime? tuNgay)
        {
            var query = _context.DatBans.Include(d => d.Ban).AsQueryable();

            // Tìm theo tên hoặc SĐT
            if (!string.IsNullOrWhiteSpace(searchString))
                query = query.Where(d => d.HoTen.Contains(searchString)
                                      || d.SoDienThoai.Contains(searchString));

            // Lọc theo trạng thái
            if (!string.IsNullOrWhiteSpace(trangThai))
                query = query.Where(d => d.TrangThai == trangThai);

            // Lọc theo ngày
            if (tuNgay.HasValue)
                query = query.Where(d => d.NgayDat.Date == tuNgay.Value.Date);

            ViewBag.SearchString = searchString;
            ViewBag.TrangThai = trangThai;
            ViewBag.TuNgay = tuNgay?.ToString("yyyy-MM-dd");

            var list = await query.OrderByDescending(d => d.NgayTao).ToListAsync();
            return View(list);
        }

        // ===== FORM ĐẶT BÀN (Khách hàng - Không cần login) =====
        [HttpGet]
        public async Task<IActionResult> Create(int? maBan)
        {
            ViewBag.Bans = new SelectList(
                await _context.Bans.Where(b => b.TrangThai == "Trống").ToListAsync(),
                "MaBan", "TenBan", maBan);

            var model = new DatBan
            {
                NgayDat = DateTime.Today,
                GioDat = new TimeSpan(19, 0, 0), // 7h tối
                SoNguoi = 2
            };
            return View(model);
        }

        // ===== XỬ LÝ ĐẶT BÀN + VALIDATION ⭐ ĐÃ SỬA =====
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DatBan datBan)
        {
            // ===== VALIDATE 1: Ngày đặt không được ở quá khứ =====
            if (datBan.NgayDat.Date < DateTime.Today)
            {
                ModelState.AddModelError("NgayDat", "Ngày đặt không được ở quá khứ!");
            }

            // ===== VALIDATE 2: Giờ đặt trong giờ mở cửa (7h - 22h) =====
            if (datBan.GioDat < new TimeSpan(7, 0, 0) || datBan.GioDat > new TimeSpan(22, 0, 0))
            {
                ModelState.AddModelError("GioDat",
                    "Quán mở cửa từ 7:00 đến 22:00. Vui lòng chọn giờ khác!");
            }

            // ===== VALIDATE 3: Số người ≤ số ghế ⭐ MỚI THÊM =====
            var ban = await _context.Bans.FindAsync(datBan.MaBan);
            if (ban != null && ban.SoGhe.HasValue && datBan.SoNguoi > ban.SoGhe.Value)
            {
                ModelState.AddModelError("SoNguoi",
                    $"Bàn {ban.TenBan} chỉ có {ban.SoGhe} ghế. Vui lòng chọn bàn lớn hơn!");
            }

            // ===== VALIDATE 4: Không chùng giờ đặt bàn (cách < 2 giờ) ⭐ ĐÃ SỬA LỖI EF CORE =====
            // Tính khoảng thời gian trước → EF Core dịch được sang SQL
            var gioMin = datBan.GioDat.Add(TimeSpan.FromHours(-2));
            var gioMax = datBan.GioDat.Add(TimeSpan.FromHours(2));

            bool trungGio = await _context.DatBans.AnyAsync(d =>
                d.MaBan == datBan.MaBan &&
                d.NgayDat.Date == datBan.NgayDat.Date &&
                d.TrangThai != "Đã hủy" &&
                d.GioDat > gioMin &&
                d.GioDat < gioMax);

            if (trungGio)
            {
                ModelState.AddModelError("GioDat",
                    "Bàn này đã được đặt trong khung giờ gần đó (cách < 2 giờ). Vui lòng chọn giờ khác!");
            }

            if (ModelState.IsValid)
            {
                datBan.TrangThai = "Chờ xác nhận";
                datBan.NgayTao = DateTime.Now;
                _context.DatBans.Add(datBan);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Đặt bàn thành công! Mã đặt bàn: #{datBan.MaDatBan}. Chúng tôi sẽ liên hệ xác nhận.";
                return RedirectToAction(nameof(DatBanThanhCong), new { id = datBan.MaDatBan });
            }

            // Nếu lỗi → load lại dropdown
            ViewBag.Bans = new SelectList(
                await _context.Bans.Where(b => b.TrangThai == "Trống").ToListAsync(),
                "MaBan", "TenBan", datBan.MaBan);
            return View(datBan);
        }

        // ===== TRANG THÔNG BÁO ĐẶT BÀN THÀNH CÔNG =====
        public async Task<IActionResult> DatBanThanhCong(int id)
        {
            var datBan = await _context.DatBans
                .Include(d => d.Ban)
                .FirstOrDefaultAsync(d => d.MaDatBan == id);
            if (datBan == null) return NotFound();
            return View(datBan);
        }

        // ===== XEM CHI TIẾT =====
        [AuthorizeRole("QuanTri", "NhanVien")]
        public async Task<IActionResult> Details(int id)
        {
            var datBan = await _context.DatBans
                .Include(d => d.Ban)
                .FirstOrDefaultAsync(d => d.MaDatBan == id);
            if (datBan == null) return NotFound();
            return View(datBan);
        }

        // ===== XÁC NHẬN ĐẶT BÀN (Nhân viên) =====
        [HttpPost, ValidateAntiForgeryToken]
        [AuthorizeRole("QuanTri", "NhanVien")]
        public async Task<IActionResult> XacNhan(int id)
        {
            var db = await _context.DatBans.FindAsync(id);
            if (db != null)
            {
                db.TrangThai = "Đã xác nhận";

                // Cập nhật trạng thái bàn
                var ban = await _context.Bans.FindAsync(db.MaBan);
                if (ban != null && ban.TrangThai == "Trống")
                    ban.TrangThai = "Đã đặt";

                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xác nhận đặt bàn!";
            }
            return RedirectToAction(nameof(Index));
        }

        // ===== HỦY ĐẶT BÀN (Nhân viên) =====
        [HttpPost, ValidateAntiForgeryToken]
        [AuthorizeRole("QuanTri", "NhanVien")]
        public async Task<IActionResult> Huy(int id)
        {
            var db = await _context.DatBans.FindAsync(id);
            if (db != null)
            {
                db.TrangThai = "Đã hủy";

                // Trả bàn về Trống nếu bàn đang "Đã đặt"
                var ban = await _context.Bans.FindAsync(db.MaBan);
                if (ban != null && ban.TrangThai == "Đã đặt")
                    ban.TrangThai = "Trống";

                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã hủy đặt bàn!";
            }
            return RedirectToAction(nameof(Index));
        }

        // ===== XÓA ĐẶT BÀN (Chỉ Quản trị) =====
        [AuthorizeRole("QuanTri")]
        public async Task<IActionResult> Delete(int id)
        {
            var db = await _context.DatBans
                .Include(d => d.Ban)
                .FirstOrDefaultAsync(d => d.MaDatBan == id);
            if (db == null) return NotFound();
            return View(db);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        [AuthorizeRole("QuanTri")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var db = await _context.DatBans.FindAsync(id);
            if (db != null)
            {
                _context.DatBans.Remove(db);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa đặt bàn!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}