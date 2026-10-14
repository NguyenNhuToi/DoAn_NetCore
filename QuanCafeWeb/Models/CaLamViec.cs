using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanCafeWeb.Models
{
    public class CaLamViec
    {
        [Key]
        public int MaCa { get; set; }

        [Required]
        [Display(Name = "Nhân viên")]
        public int MaTK { get; set; }

        [ForeignKey("MaTK")]
        public TaiKhoan? TaiKhoan { get; set; }

        [Display(Name = "Giờ vào")]
        public DateTime GioVao { get; set; } = DateTime.Now;

        [Display(Name = "Giờ ra")]
        public DateTime? GioRa { get; set; }

        [StringLength(300)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        // Tổng giờ làm
        public double? SoGioLam =>
            GioRa.HasValue ? (GioRa.Value - GioVao).TotalHours : null;
    }
}