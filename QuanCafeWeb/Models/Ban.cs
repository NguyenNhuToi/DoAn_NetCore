using System.ComponentModel.DataAnnotations;

namespace QuanCafeWeb.Models
{
    public class Ban
    {
        [Key]
        public int MaBan { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Tên bàn")]
        public string TenBan { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Khu vực")]
        public string? KhuVuc { get; set; }

        [Display(Name = "Số ghế")]
        public int? SoGhe { get; set; }

        [StringLength(50)]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = "Trống";

        public ICollection<Order>? Orders { get; set; }
    }
}