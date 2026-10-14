using QuanCafeWeb.Models;

namespace QuanCafeWeb.Data
{
    public static class DbSeeder
    {
        public static void Seed(QuanCafeContext context)
        {
            var accounts = context.TaiKhoans.ToList();
            foreach (var acc in accounts)
            {
                if (!acc.MatKhau.StartsWith("$2"))
                {
                    acc.MatKhau = BCrypt.Net.BCrypt.HashPassword(acc.MatKhau);
                }
            }
            context.SaveChanges();
        }
    }
}