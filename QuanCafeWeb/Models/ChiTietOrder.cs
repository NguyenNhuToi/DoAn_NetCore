using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanCafeWeb.Models
{
    public class ChiTietOrder
    {
        public int MaOrder { get; set; }
        [ForeignKey("MaOrder")]
        public Order? Order { get; set; }

        public int MaMon { get; set; }
        [ForeignKey("MaMon")]
        public Mon? Mon { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải >= 1")]
        [Display(Name = "Số lượng")]
        public int SoLuong { get; set; }

        [Required]
        [Display(Name = "Đơn giá")]
        public decimal DonGia { get; set; }

        [StringLength(200)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }
    }
}