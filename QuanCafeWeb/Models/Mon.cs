using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanCafeWeb.Models
{
    public class Mon
    {
        [Key]
        public int MaMon { get; set; }

        [Required(ErrorMessage = "Tên món không được để trống")]
        [StringLength(200)]
        [Display(Name = "Tên món")]
        public string TenMon { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Danh mục")]
        public int MaDM { get; set; }

        [ForeignKey("MaDM")]
        public DanhMucMon? DanhMucMon { get; set; }

        [Required(ErrorMessage = "Giá bán không được để trống")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá bán phải lớn hơn 0")]
        [Display(Name = "Giá bán")]
        public decimal GiaBan { get; set; }

        [Display(Name = "Ảnh món")]
        public string? AnhMoi { get; set; }

        [StringLength(500)]
        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }

        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true;

        public ICollection<ChiTietOrder>? ChiTietOrders { get; set; }
    }
}