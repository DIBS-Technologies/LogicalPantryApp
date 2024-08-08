using LogicalPantry.DTOs.TenantDtos;
using LogicalPantry.Models.Models;
using LogicalPantry.Services.InformationService;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace LogicalPantry.Web.Controllers
{
    [Route("Information")]
    public class InformationController : HomeController
    {
        IInformationService _informationService;
        IWebHostEnvironment _webHostEnvironment;

        //protected TenantDto Tenant => HttpContext.Items["Tenant"] as TenantDto;
        public InformationController(IInformationService informationService,IWebHostEnvironment web)
        {
            _informationService = informationService;
            _webHostEnvironment = web;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public object Get(string tenantid)
        {
            
            if (int.Parse(tenantid) == 0) { return null; }
            var response = _informationService.GetTenant(int.Parse(tenantid)).Result;
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
                
                    @TempData["MessageClass"] = "alert-success";
                    @TempData["SuccessMessageInfo"] = "Infromation Saved Successfully";

                // Redirect to the GET method to display the updated data
                return RedirectToAction(nameof(AddTenant2), new { id = tenantDto.Id });
            }
            else
            {
                @TempData["MessageClass"] = "alert-danger";
                @TempData["ErrorMessageInfo"] = "Internal server error.";
                ModelState.AddModelError("", response.Message);
            }


            return View(tenantDto);
        }


        [HttpGet("Home")]
        public async Task<IActionResult> Home(string PageName)
        {
           // HttpContext.Items["SkipTenantMiddleware"] = true;


            var tenanatResponse = await _informationService.GetTenantPageNameForUserAsync(PageName);
            if (tenanatResponse.Success)
            {
                var pageName = tenanatResponse.Data.PageName;

                var fileExtension = ".html";
                if (!pageName.EndsWith(fileExtension, StringComparison.OrdinalIgnoreCase))
                {
                    pageName += fileExtension;
                }

                var tenantFolderPath = Path.Combine(_webHostEnvironment.WebRootPath, "TenantHomePage");
                var filepath = Path.Combine(tenantFolderPath, pageName);
                var fileNameWithExtension = Path.GetFileName(filepath);

                Console.WriteLine($"Page Name: {pageName}");
                Console.WriteLine($"Tenant Folder Path: {tenantFolderPath}");
                Console.WriteLine($"File Path: {filepath}");

                if (!System.IO.File.Exists(filepath))
                {
                    Console.WriteLine("File not found.");
                    return NotFound("The requested page was not found.");
                }

                string htmlContent;
                try
                {
                    htmlContent = await System.IO.File.ReadAllTextAsync(filepath);
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"IOException: {ex.Message}");
                    return StatusCode(500, "An error occurred while reading the file.");
                }

                TempData["PageName"] = fileNameWithExtension;

                return View();
            }

            return NotFound(tenanatResponse.Message);
        }

        [HttpGet("/{action?}")]
        public async Task<IActionResult> Home()
        {
            // HttpContext.Items["SkipTenantMiddleware"] = true;

             var tenantName = TenantName; ;
            var tenanatResponse = await _informationService.GetTenantPageNameForUserAsync(tenantName);
            if (tenanatResponse.Success)
            {
                var pageName = tenanatResponse.Data.PageName;

                if (pageName == null)
                {
                    return NotFound("The requested page name was not found.");
                }

                var fileExtension = ".html";
                if (!pageName.EndsWith(fileExtension, StringComparison.OrdinalIgnoreCase))
                {
                    pageName += fileExtension;
                }

                var tenantFolderPath = Path.Combine(_webHostEnvironment.WebRootPath, "TenantHomePage");
                var filepath = Path.Combine(tenantFolderPath, pageName);
                var fileNameWithExtension = Path.GetFileName(filepath);

                Console.WriteLine($"Page Name: {pageName}");
                Console.WriteLine($"Tenant Folder Path: {tenantFolderPath}");
                Console.WriteLine($"File Path: {filepath}");

                if (!System.IO.File.Exists(filepath))
                {
                    Console.WriteLine("File not found.");
                    return NotFound("The requested page was not found.");
                }

                string htmlContent;
                try
                {
                    htmlContent = await System.IO.File.ReadAllTextAsync(filepath);
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"IOException: {ex.Message}");
                    return StatusCode(500, "An error occurred while reading the file.");
                }

                TempData["PageName"] = fileNameWithExtension;

                return View();
            }

            return NotFound(tenanatResponse.Message);
        }


        [HttpGet("GetTenant")]
        public async Task<IActionResult> GetTenantIdByName(string tenantName)
        {
            var response = await _informationService.GetTenantByNameAsync(tenantName);
            if (response.Success)
            {
                return Ok(response.Data);
            }

            return NotFound(response.Message);
        }


        [HttpGet("GetTenantByUserEmail")]
        public async Task<IActionResult> GetTenantIdByEmail(string userEmail)
        {
            var response = await _informationService.GetTenantIdByEmail(userEmail);
            if (response.Success)
            {
                return Ok(response.Data);
            }

            return NotFound(response.Message);
        }


    }

}
