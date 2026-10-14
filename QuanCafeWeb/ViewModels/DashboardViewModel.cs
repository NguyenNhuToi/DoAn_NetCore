namespace QuanCafeWeb.ViewModels
{
    public class DashboardViewModel
    {
        // Thống kê số lượng
        public int SoMon { get; set; }
        public int SoBan { get; set; }
        public int SoOrder { get; set; }
        public int SoNhanVien { get; set; }

        // Doanh thu
        public decimal DoanhThuHomNay { get; set; }
        public decimal DoanhThuTuan { get; set; }
        public decimal DoanhThuThang { get; set; }
        public decimal DoanhThuNam { get; set; }

        // Biểu đồ 7 ngày
        public List<string> Labels7Ngay { get; set; } = new();
        public List<decimal> Data7Ngay { get; set; } = new();

        // Top 5 món bán chạy
        public List<string> TopMonTen { get; set; } = new();
        public List<int> TopMonSoLuong { get; set; } = new();

        // Đơn hàng gần đây
        public List<QuanCafeWeb.Models.Order> DonHangGanDay { get; set; } = new();

        // Bàn đang phục vụ
        public List<QuanCafeWeb.Models.Ban> BanDangPhucVu { get; set; } = new();
    }
}