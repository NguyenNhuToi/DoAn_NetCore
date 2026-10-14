using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanCafeWeb.Models
{
    public class Order
    {
        [Key]
        public int MaOrder { get; set; }

        [Display(Name = "Bàn")]
        public int? MaBan { get; set; }

        [ForeignKey("MaBan")]
        public Ban? Ban { get; set; }

        public int? MaKH { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Loại order")]
        public string LoaiOrder { get; set; } = "Tại chỗ";

        [Display(Name = "Giờ vào")]
        public DateTime GioVao { get; set; } = DateTime.Now;

        [Display(Name = "Giờ ra")]
        public DateTime? GioRa { get; set; }

        [Display(Name = "Tổng tiền")]
        public decimal TongTien { get; set; } = 0;

        public ICollection<ChiTietOrder>? ChiTietOrders { get; set; }
    }
}