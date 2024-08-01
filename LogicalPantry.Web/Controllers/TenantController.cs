using LogicalPantry.DTOs.TenantDtos;
using LogicalPantry.Services.TenantServices;
using Microsoft.AspNetCore.Mvc;

namespace LogicalPantry.Web.Controllers
{
    [Route("Tenant")]
    public class TenantController : Controller
    {


        public async Task<IActionResult> GetImage(string userEmail)
        {

            var imageUrl = "~\\Image\\Demo.jpeg";


            return Json(new { ImageUrl = imageUrl });
        }

    }
}

