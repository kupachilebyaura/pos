using Microsoft.AspNetCore.Mvc;

namespace POS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        [HttpGet("product-image/{fileName}")]
        public IActionResult GetProductImage(string fileName)
        {
            var cloudinaryBase = "https://res.cloudinary.com/<tu_cloud_name>/image/upload/";
            var url = $"{cloudinaryBase}{fileName}";
            return Redirect(url);
        }
    }
}