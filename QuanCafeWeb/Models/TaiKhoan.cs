using System.ComponentModel.DataAnnotations;

namespace QuanCafeWeb.Models
{
    public class TaiKhoan
    {
        [Key]
        public int MaTK { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Tên đăng nhập")]
        public string TenDangNhap { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string MatKhau { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Họ tên")]
        public string? HoTen { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Vai trò")]
        public string VaiTro { get; set; } = "KhachHang";

        public bool TrangThai { get; set; } = true;
    }
}