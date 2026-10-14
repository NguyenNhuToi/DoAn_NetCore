using System.ComponentModel.DataAnnotations;

namespace QuanCafeWeb.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Nhập tên đăng nhập")]
        [Display(Name = "Tên đăng nhập")]
        public string TenDangNhap { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nhập mật khẩu")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string MatKhau { get; set; } = string.Empty;
    }
}