using LogicalPantry.DTOs.TenantDtos;
using LogicalPantry.Models.Models;
using LogicalPantry.Services.InformationService;
using Microsoft.AspNetCore.Mvc;

namespace LogicalPantry.Web.Controllers
{
    [Route("Information")]
    public class InformationController : Controller
    {
        IInformationService _informationService;
        public InformationController(IInformationService informationService)
        {
            _informationService = informationService;     
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public object Get(string tenantid) 
        {
            if (int.Parse(tenantid) == 0) { return null; }
            var response=_informationService.GetTenant(int.Parse(tenantid)).Result;
            return response;
        }

        [HttpGet]
        [Route("AddTenant")]
        public IActionResult AddTenant()
        {
            return View();
        }


        public async Task<IActionResult> AddTenant2(int id)
        {
            var response = _informationService.GetTenant(id);
            if (response.Result.Success)
            {
                return View(response.Result.Data);
            }

            //Handle the case where the tenant is not found
            return NotFound(/*tenantResponse.Message*/);
        }

        // Handle form submission
        [HttpPost("AddTenant")]
        public async Task<IActionResult> AddTenant(TenantDto tenantDto, IFormFile LogoFile)
        {
            
                if (LogoFile != null && LogoFile.Length > 0)
                {
                    // Generate a unique file name to avoid conflicts
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(LogoFile.FileName);
                    var filePath = Path.Combine("wwwroot\\Image", fileName);

                    // Save the file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await LogoFile.CopyToAsync(stream);
                    }

                    // Update the tenantDto with the new logo path
                    tenantDto.Logo = "/Image/" + fileName;
                }

                var response = await _informationService.PostTenant(tenantDto);
                if (response.Success)
                {
                    // Redirect to the GET method to display the updated data
                    return RedirectToAction(nameof(AddTenant2), new { id = tenantDto.Id });
                }
                else
                {
                    ModelState.AddModelError("", response.Message);
                }
            

            return View(tenantDto);
        }
    }


}
