namespace QuanCafeWeb.ViewModels
{
    public class CartItem
    {
        public int MaMon { get; set; }
        public string TenMon { get; set; } = string.Empty;
        public decimal DonGia { get; set; }
        public int SoLuong { get; set; }
        public decimal ThanhTien => DonGia * SoLuong;
    }
}