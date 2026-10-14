using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanCafeWeb.Models
{
    public class DatBan
    {
        [Key]
        public int MaDatBan { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn bàn")]
        [Display(Name = "Bàn")]
        public int MaBan { get; set; }

        [ForeignKey("MaBan")]
        public Ban? Ban { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên phải từ 2-100 ký tự")]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [StringLength(20)]
        [RegularExpression(@"^[0-9]{10,11}$", ErrorMessage = "SĐT phải là 10-11 chữ số")]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn ngày đặt")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày đặt")]
        public DateTime NgayDat { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Vui lòng chọn giờ đặt")]
        [DataType(DataType.Time)]
        [Display(Name = "Giờ đặt")]
        public TimeSpan GioDat { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số người")]
        [Range(1, 20, ErrorMessage = "Số người phải từ 1-20")]
        [Display(Name = "Số người")]
        public int SoNguoi { get; set; } = 2;

        [StringLength(300)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        [StringLength(50)]
        [Display(Name = "Trạng thái")]       
        public string TrangThai { get; set; } = "Chờ xác nhận";

        [Display(Name = "Ngày tạo")]           // ⭐ THÊM DÒNG NÀY
        public DateTime NgayTao { get; set; } = DateTime.Now;
    }
}