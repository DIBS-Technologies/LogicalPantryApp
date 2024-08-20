using Azure;
using LogicalPantry.DTOs.TenantDtos;
using LogicalPantry.Models.Models;
using LogicalPantry.Services.InformationService;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Reflection;
using static System.Net.WebRequestMethods;

namespace LogicalPantry.Web.Controllers
{
    [Route("Information")]
    public class InformationController : BaseController
    {
        private readonly IInformationService _informationService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<InformationController> _logger;
        private readonly IMemoryCache _memoryCache;

        public InformationController(IInformationService informationService, IWebHostEnvironment webHostEnvironment, ILogger<InformationController> logger, IMemoryCache memoryCache)
        {
            _informationService = informationService;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
            _memoryCache = memoryCache;
        }


        public IActionResult Index()
        {
            _logger.LogInformation($"Index method call started");
            _logger.LogInformation($"Index method call ended");
            return View();
        }

        [HttpGet("Get")]
        public async Task<IActionResult> Get(int tenantid)
        {
            _logger.LogInformation("Get Object call started");
            try
            {            
            if (tenantid == 0) { return null; }
            var response =await _informationService.GetTenant(tenantid);

                _logger.LogInformation("Get Object call ended");
                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting tenant.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
     
        }

           



        [HttpGet]
        [Route("AddTenant")]        
        public async Task<IActionResult> AddTenant()
        {
            var tenantName = TenantName;
            var response = await _informationService.GetTenantByNameAsync(tenantName);

            return View(response.Data);
        }

        // Handle form submission
        [HttpPost("AddTenant")]
        public async Task<IActionResult> AddTenant(TenantDto tenantDto, IFormFile LogoFile)
        {
            tenantDto.TenantName = TenantName;

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
                return View(tenantDto);
                //return RedirectToAction(nameof(AddTenant));
            }
            else
            {
                @TempData["MessageClass"] = "alert-danger";
                @TempData["ErrorMessageInfo"] = "Internal server error.";
                ModelState.AddModelError("", response.Message);
            }


            return View(tenantDto);
        }

        public async Task<IActionResult> RedirectTenant(int id)
        {
            var response = _informationService.GetTenant(id);
            if (response.Result.Success)
            {

                return View(response.Result.Data);
            }
            return NotFound(response.Result.Message);
        }




        [HttpGet("Home")]
        public async Task<IActionResult> Home(string PageName)
        {


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

            var tenantName = TenantName; 
            var tenanatResponse = await _informationService.GetTenantPageNameForUserAsync(tenantName);
            if (tenanatResponse.Success)
            {
                var pageName = tenanatResponse.Data.PageName;
                var tenantId = tenanatResponse.Data.Id;
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
                ViewBag.PageName = pageName;
                ViewBag.TenantId = tenantId;
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