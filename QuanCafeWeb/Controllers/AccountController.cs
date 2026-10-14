using Microsoft.AspNetCore.Mvc;
using QuanCafeWeb.Data;
using QuanCafeWeb.ViewModels;

namespace QuanCafeWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly QuanCafeContext _context;
        public AccountController(QuanCafeContext context) => _context = context;

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = _context.TaiKhoans
                .FirstOrDefault(u => u.TenDangNhap == model.TenDangNhap && u.TrangThai);

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.MatKhau, user.MatKhau))
            {
                ViewBag.Error = "Sai tên đăng nhập hoặc mật khẩu!";
                return View(model);
            }

            HttpContext.Session.SetInt32("MaTK", user.MaTK);
            HttpContext.Session.SetString("HoTen", user.HoTen ?? "");
            HttpContext.Session.SetString("VaiTro", user.VaiTro);

            return user.VaiTro switch
            {
                "QuanTri" => RedirectToAction("Index", "Home"),
                "NhanVien" => RedirectToAction("Index", "Order"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}