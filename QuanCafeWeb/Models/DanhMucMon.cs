using System.ComponentModel.DataAnnotations;

namespace QuanCafeWeb.Models
{
    public class DanhMucMon
    {
        [Key]
        public int MaDM { get; set; }

        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        [StringLength(100)]
        [Display(Name = "Tên danh mục")]
        public string TenDM { get; set; } = string.Empty;

        [Display(Name = "Thứ tự")]
        public int? ThuTu { get; set; }

        // Navigation
        public ICollection<Mon>? Mons { get; set; }
    }
}