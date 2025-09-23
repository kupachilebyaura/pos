using Microsoft.AspNetCore.Authorization;

namespace POS.API.Authorization
{
    public class PermissionAttribute : AuthorizeAttribute
    {
        public PermissionAttribute(string permission)
        {
            Policy = $"Permission:{permission}";
        }
    }
}