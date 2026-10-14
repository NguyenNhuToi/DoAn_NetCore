using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace QuanCafeWeb.Filters
{
    public class AuthorizeRoleAttribute : ActionFilterAttribute
    {
        private readonly string[] _roles;

        public AuthorizeRoleAttribute(params string[] roles) => _roles = roles;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var vaiTro = context.HttpContext.Session.GetString("VaiTro");

            if (string.IsNullOrEmpty(vaiTro))
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            if (_roles.Length > 0 && !_roles.Contains(vaiTro))
            {
                context.Result = new ContentResult
                {
                    Content = @"
                        <html><head><meta charset='utf-8'>
                        <link rel='stylesheet' href='/lib/bootstrap/dist/css/bootstrap.min.css'>
                        </head><body class='p-5'>
                        <div class='alert alert-danger'>
                            <h3>🚫 Bạn không có quyền truy cập!</h3>
                            <p>Vai trò: <b>" + vaiTro + @"</b></p>
                            <a href='/' class='btn btn-primary'>Về trang chủ</a>
                        </div></body></html>",
                    ContentType = "text/html; charset=utf-8"
                };
            }
        }
    }
}