using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using QuanCafeWeb.Data;
using QuanCafeWeb.Filters;
using QuanCafeWeb.Models;
using QuanCafeWeb.ViewModels;
using X.PagedList;
using X.PagedList.Extensions;

namespace QuanCafeWeb.Controllers
{
    [AuthorizeRole("QuanTri", "NhanVien")]
    public class OrderController : Controller
    {
        private readonly QuanCafeContext _context;
        private const string CART_KEY = "GIOHANG";

        public OrderController(QuanCafeContext context) => _context = context;

        // ============ HELPER: SESSION GIỎ HÀNG ============
        private List<CartItem> GetCart()
        {
            var json = HttpContext.Session.GetString(CART_KEY);
            return string.IsNullOrEmpty(json)
                ? new List<CartItem>()
                : JsonConvert.DeserializeObject<List<CartItem>>(json)!;
        }

        private void SaveCart(List<CartItem> cart)
        {
            HttpContext.Session.SetString(CART_KEY, JsonConvert.SerializeObject(cart));
        }

        // ============ DANH SÁCH ORDER (LỌC + SẮP XẾP + PHÂN TRANG) ============
        public async Task<IActionResult> Index(
            int? maBan,
            string? loaiOrder,
            DateTime? tuNgay,
            DateTime? denNgay,
            string? sortOrder,
            int? page)
        {
            var query = _context.Orders.Include(o => o.Ban).AsQueryable();

            if (maBan.HasValue && maBan.Value > 0)
                query = query.Where(o => o.MaBan == maBan.Value);

            if (!string.IsNullOrWhiteSpace(loaiOrder))
                query = query.Where(o => o.LoaiOrder == loaiOrder);

            if (tuNgay.HasValue)
                query = query.Where(o => o.GioVao.Date >= tuNgay.Value.Date);
            if (denNgay.HasValue)
                query = query.Where(o => o.GioVao.Date <= denNgay.Value.Date);

            ViewBag.CurrentSort = sortOrder;
            ViewBag.SortNgayAsc = string.IsNullOrEmpty(sortOrder) ? "ngay_desc" : "";
            ViewBag.SortTienAsc = sortOrder == "tien_asc" ? "tien_desc" : "tien_asc";

            query = sortOrder switch
            {
                "ngay_asc" => query.OrderBy(o => o.GioVao),
                "ngay_desc" => query.OrderByDescending(o => o.GioVao),
                "tien_asc" => query.OrderBy(o => o.TongTien),
                "tien_desc" => query.OrderByDescending(o => o.TongTien),
                _ => query.OrderByDescending(o => o.GioVao)
            };

            ViewBag.MaBan = new SelectList(_context.Bans, "MaBan", "TenBan", maBan);
            ViewBag.CurrentMaBan = maBan;
            ViewBag.LoaiOrder = loaiOrder;
            ViewBag.TuNgay = tuNgay?.ToString("yyyy-MM-dd");
            ViewBag.DenNgay = denNgay?.ToString("yyyy-MM-dd");

            var list = await query.ToListAsync();
            var paged = list.ToPagedList(page ?? 1, 10);
            return View(paged);
        }

        // ============ TẠO ORDER ============
        public async Task<IActionResult> TaoOrder()
        {
            ViewBag.Mons = await _context.Mons
                .Include(m => m.DanhMucMon)
                .Where(m => m.TrangThai)
                .ToListAsync();

            ViewBag.Bans = await _context.Bans
                .Where(b => b.TrangThai == "Trống")
                .ToListAsync();

            return View(GetCart());
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int maMon, int soLuong = 1)
        {
            var mon = await _context.Mons.FindAsync(maMon);
            if (mon == null)
                return Json(new { success = false, message = "Không tìm thấy món" });

            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.MaMon == maMon);

            if (item == null)
            {
                cart.Add(new CartItem
                {
                    MaMon = mon.MaMon,
                    TenMon = mon.TenMon,
                    DonGia = mon.GiaBan,
                    SoLuong = soLuong
                });
            }
            else
            {
                item.SoLuong += soLuong;
            }

            SaveCart(cart);
            return Json(new { success = true, soMon = cart.Sum(c => c.SoLuong) });
        }

        public IActionResult RemoveFromCart(int maMon)
        {
            var cart = GetCart();
            cart.RemoveAll(c => c.MaMon == maMon);
            SaveCart(cart);
            return RedirectToAction(nameof(TaoOrder));
        }

        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove(CART_KEY);
            return RedirectToAction(nameof(TaoOrder));
        }

        // ============ THANH TOÁN ============
        public IActionResult Checkout()
        {
            var cart = GetCart();
            if (cart.Count == 0) return RedirectToAction(nameof(TaoOrder));
            ViewBag.Bans = _context.Bans.Where(b => b.TrangThai == "Trống").ToList();
            return View(cart);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(int? maBan, string loaiOrder)
        {
            var cart = GetCart();
            if (cart.Count == 0) return RedirectToAction(nameof(TaoOrder));

            var order = new Order
            {
                MaBan = maBan,
                LoaiOrder = loaiOrder,
                GioVao = DateTime.Now,
                TongTien = cart.Sum(c => c.ThanhTien)
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            foreach (var item in cart)
            {
                _context.ChiTietOrders.Add(new ChiTietOrder
                {
                    MaOrder = order.MaOrder,
                    MaMon = item.MaMon,
                    SoLuong = item.SoLuong,
                    DonGia = item.DonGia
                });
            }
            await _context.SaveChangesAsync();

            if (maBan.HasValue)
            {
                var ban = await _context.Bans.FindAsync(maBan);
                if (ban != null) ban.TrangThai = "Đang phục vụ";
                await _context.SaveChangesAsync();
            }

            HttpContext.Session.Remove(CART_KEY);
            TempData["Success"] = $"Đã tạo đơn hàng #{order.MaOrder} thành công!";
            return RedirectToAction(nameof(Details), new { id = order.MaOrder });
        }

        // ============ XEM CHI TIẾT ============
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Ban)
                .Include(o => o.ChiTietOrders).ThenInclude(c => c.Mon)
                .FirstOrDefaultAsync(o => o.MaOrder == id);

            if (order == null) return NotFound();
            return View(order);
        }

        // ============ SỬA ORDER (CÓ CHẶN HĐ ĐÃ THANH TOÁN) ============
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            // ⭐ Chặn sửa HĐ đã thanh toán (trừ Quản trị)
            if (order.GioRa != null && HttpContext.Session.GetString("VaiTro") != "QuanTri")
            {
                TempData["Error"] = "🚫 Hóa đơn đã thanh toán, không thể sửa!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Bans = new SelectList(_context.Bans, "MaBan", "TenBan", order.MaBan);
            return View(order);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Order order)
        {
            if (id != order.MaOrder) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.Orders.FindAsync(id);
                    if (existing == null) return NotFound();

                    // ⭐ Chặn sửa HĐ đã thanh toán (trừ Quản trị)
                    if (existing.GioRa != null && HttpContext.Session.GetString("VaiTro") != "QuanTri")
                    {
                        TempData["Error"] = "🚫 Hóa đơn đã thanh toán, không thể sửa!";
                        return RedirectToAction(nameof(Index));
                    }

                    existing.MaBan = order.MaBan;
                    existing.LoaiOrder = order.LoaiOrder;
                    existing.GioRa = order.GioRa;

                    _context.Update(existing);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật đơn hàng thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Orders.Any(e => e.MaOrder == id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Bans = new SelectList(_context.Bans, "MaBan", "TenBan", order.MaBan);
            return View(order);
        }

        // ============ XÓA ORDER - BẢO MẬT 3 TẦNG + CHẶN HĐ ĐÃ TT ============
        // 🛡️ TẦNG 2: Chỉ Quản trị mới vào được
        [HttpGet]
        [AuthorizeRole("QuanTri")]
        public async Task<IActionResult> Delete(int id)
        {
            // 🛡️ TẦNG 3: Kiểm tra lại vai trò trong code
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            if (vaiTro != "QuanTri")
            {
                TempData["Error"] = "🚫 Chỉ Quản trị mới được xóa hóa đơn!";
                return RedirectToAction(nameof(Index));
            }

            var order = await _context.Orders
                .Include(o => o.Ban)
                .Include(o => o.ChiTietOrders).ThenInclude(c => c.Mon)
                .FirstOrDefaultAsync(o => o.MaOrder == id);

            if (order == null) return NotFound();

            // 🛡️ Chặn xóa hóa đơn đã thanh toán
            if (order.GioRa != null)
            {
                TempData["Error"] = $"🚫 Không thể xóa! Hóa đơn #{id} đã thanh toán.";
                return RedirectToAction(nameof(Index));
            }

            return View(order);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        [AuthorizeRole("QuanTri")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // 🛡️ TẦNG 3: Kiểm tra lại vai trò
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            if (vaiTro != "QuanTri")
            {
                TempData["Error"] = "🚫 Chỉ Quản trị mới được xóa hóa đơn!";
                return RedirectToAction(nameof(Index));
            }

            var order = await _context.Orders
                .Include(o => o.ChiTietOrders)
                .FirstOrDefaultAsync(o => o.MaOrder == id);

            if (order == null) return NotFound();

            // 🛡️ Chặn xóa hóa đơn đã thanh toán
            if (order.GioRa != null)
            {
                TempData["Error"] = "🚫 Không thể xóa hóa đơn đã thanh toán!";
                return RedirectToAction(nameof(Index));
            }

            _context.ChiTietOrders.RemoveRange(order.ChiTietOrders!);
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"✅ Đã xóa hóa đơn #{id}!";
            return RedirectToAction(nameof(Index));
        }

        // ============ ĐÁNH DẤU HOÀN THÀNH (NV + QT đều dùng được) ============
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> HoanThanh(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null && order.GioRa == null)
            {
                order.GioRa = DateTime.Now;

                if (order.MaBan.HasValue)
                {
                    var ban = await _context.Bans.FindAsync(order.MaBan.Value);
                    if (ban != null) ban.TrangThai = "Trống";
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = $"Đã hoàn thành đơn #{id}!";
            }
            return RedirectToAction(nameof(Index));
        }

        // ============ 🆕 KHÓA HÓA ĐƠN (CHỈ QUẢN TRỊ) ============
        [HttpPost, ValidateAntiForgeryToken]
        [AuthorizeRole("QuanTri")]
        public async Task<IActionResult> KhoaHoaDon(int id)
        {
            // 🛡️ Kiểm tra lại vai trò
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            if (vaiTro != "QuanTri")
            {
                TempData["Error"] = "🚫 Chỉ Quản trị mới được khóa hóa đơn!";
                return RedirectToAction(nameof(Index));
            }

            var order = await _context.Orders.FindAsync(id);
            if (order != null && order.GioRa == null)
            {
                order.GioRa = DateTime.Now;
                await _context.SaveChangesAsync();
                TempData["Success"] = $"🔒 Đã khóa hóa đơn #{id}!";
            }
            else if (order != null)
            {
                TempData["Error"] = $"⚠️ Hóa đơn #{id} đã được khóa trước đó.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}