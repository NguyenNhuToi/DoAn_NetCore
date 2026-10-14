using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using QuanCafeWeb.Data;
using QuanCafeWeb.Models;
using QuanCafeWeb.ViewModels;

namespace QuanCafeWeb.Controllers
{
    public class OrderController : Controller
    {
        private readonly QuanCafeContext _context;
        private const string CART_KEY = "GIOHANG";

        public OrderController(QuanCafeContext context) => _context = context;

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

        // GET: /Order - Danh sách order đã tạo
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.Ban)
                .OrderByDescending(o => o.GioVao)
                .ToListAsync();
            return View(orders);
        }

        // GET: /Order/TaoOrder - Giao diện chọn món
        public async Task<IActionResult> TaoOrder()
        {
            ViewBag.Mons = await _context.Mons.Include(m => m.DanhMucMon)
                                              .Where(m => m.TrangThai).ToListAsync();
            ViewBag.Bans = await _context.Bans.Where(b => b.TrangThai == "Trống").ToListAsync();
            return View(GetCart());
        }

        // AJAX: Thêm vào giỏ
        [HttpPost]
        public async Task<IActionResult> AddToCart(int maMon, int soLuong = 1)
        {
            var mon = await _context.Mons.FindAsync(maMon);
            if (mon == null) return Json(new { success = false, message = "Không tìm thấy món" });

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
            else item.SoLuong += soLuong;

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

        // GET: /Order/Checkout
        public IActionResult Checkout()
        {
            var cart = GetCart();
            if (cart.Count == 0) return RedirectToAction(nameof(TaoOrder));
            ViewBag.Bans = _context.Bans.Where(b => b.TrangThai == "Trống").ToList();
            return View(cart);
        }

        // POST: /Order/Checkout
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

        // GET: /Order/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Ban)
                .Include(o => o.ChiTietOrders).ThenInclude(c => c.Mon)
                .FirstOrDefaultAsync(o => o.MaOrder == id);
            if (order == null) return NotFound();
            return View(order);
        }
    }
}