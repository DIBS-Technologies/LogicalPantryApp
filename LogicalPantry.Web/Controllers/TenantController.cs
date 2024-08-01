using LogicalPantry.DTOs.TenantDtos;
using LogicalPantry.Services.TenantServices;
using Microsoft.AspNetCore.Mvc;

namespace LogicalPantry.Web.Controllers
{
    [Route("Tenant")]
    public class TenantController : Controller
    {

        private readonly ITenantService _tenantService;

        public TenantController(ITenantService tenantService)
        {
            _tenantService = tenantService;
        }


        [HttpGet]
        public IActionResult AddTenant()
        {
            return View();
        }


        // Display the form for editing
        [HttpGet("EditTenant/{id}")]
        public async Task<IActionResult> EditTenant(int id)
        {
            var tenantResponse = await _tenantService.GetTenantByIdAsync(id);
            if (tenantResponse.Success)
            {
                return View(tenantResponse.Data);
            }

            // Handle the case where the tenant is not found
            return NotFound(tenantResponse.Message);
        }

        // Handle form submission
        [HttpPost("EditTenant/{id}")]
        public async Task<IActionResult> EditTenant(TenantDto tenantDto, IFormFile LogoFile)
        {
            if (ModelState.IsValid)
            {
                if (LogoFile != null && LogoFile.Length > 0)
                {
                    // Generate a unique file name to avoid conflicts
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(LogoFile.FileName);
                    var filePath = Path.Combine("wwwroot/Pages", fileName);

                    // Save the file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await LogoFile.CopyToAsync(stream);
                    }

                    // Update the tenantDto with the new logo path
                    tenantDto.Logo = "/Pages/" + fileName;
                }

                var response = await _tenantService.UpdateTenantAsync(tenantDto);
                if (response.Success)
                {
                    // Redirect to the GET method to display the updated data
                    return RedirectToAction(nameof(EditTenant), new { id = tenantDto.Id });
                }
                else
                {
                    ModelState.AddModelError("", response.Message);
                }
            }

            return View(tenantDto);
        }
    }
}

