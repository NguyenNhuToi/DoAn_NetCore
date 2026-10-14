using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanCafeWeb.Models
{
    public class DanhGia
    {
        [Key]
        public int MaDanhGia { get; set; }

        [Required]
        public int MaMon { get; set; }

        [ForeignKey("MaMon")]
        public Mon? Mon { get; set; }

        [StringLength(100)]
        [Display(Name = "Họ tên")]
        public string? HoTen { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Số sao 1-5")]
        [Display(Name = "Số sao")]
        public int SoSao { get; set; } = 5;

        [StringLength(500)]
        [Display(Name = "Bình luận")]
        public string? BinhLuan { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;
    }
}