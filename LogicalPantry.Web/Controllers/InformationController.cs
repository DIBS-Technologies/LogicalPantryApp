using Azure;
using LogicalPantry.DTOs.TenantDtos;
using LogicalPantry.Models.Models;
using LogicalPantry.Services.InformationService;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
        public async Task<IActionResult> Get(string tenantid)
        {
            _logger.LogInformation("Get Object call started");
            try
            {
                if (!int.TryParse(tenantid, out int tenantId) || tenantId == 0)
                {
                    return BadRequest("Invalid tenant ID");
                }

                var response = await _informationService.GetTenant(tenantId);
                if (response?.Data == null)
                {
                    return NotFound("Tenant data not found");
                }

                // Assuming response.Data is of type TenantDto
                var tenantDto = response.Data;
                if (string.IsNullOrEmpty(tenantDto.Logo))
                {
                    return NotFound("ImageUrl not found");
                }

                _logger.LogInformation("Get Object call ended");
                return Ok(tenantDto.Logo); 
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
            // Log the starting of the Index method execution.
            _logger.LogInformation("AddTenant Get method call started");
            var tenantName = TenantName;
            //var tenantIdString = HttpContext.Session.GetString("TenantId");

            //if (!int.TryParse(tenantIdString, out int tenantId) || tenantId == 0)
            //{
            //    return BadRequest("Invalid tenant ID");
            //}
            var PageName = HttpContext.Session.GetString("PageName");

            //ViewBag.TenantId = tenantId;
            ViewBag.PageName = PageName;
            var response = await _informationService.GetTenantByNameAsync(tenantName);
            // Log the ending of the Index method execution.
            _logger.LogInformation("AddTenant Get method call ended");
            return View(response.Data);
        }

        // Handle form submission
        [HttpPost("AddTenant")]
        public async Task<IActionResult> AddTenant(TenantDto tenantDto, IFormFile LogoFile)
        {
            // Log the starting of the Index method execution.
            _logger.LogInformation("AddTenant post method call started");
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
                var PageName = HttpContext.Session.GetString("PageName");

                //ViewBag.TenantId = tenantId;
                ViewBag.PageName = PageName;
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

            // Log the ending of the Index method execution.
            _logger.LogInformation("AddTenant post method call ended");
            return View(tenantDto);
        }

        public async Task<IActionResult> RedirectTenant(int id)
        {
            // Log the starting of the Index method execution.
            _logger.LogInformation("RedirectTenant  method call started");
            var response = _informationService.GetTenant(id);
            if (response.Result.Success)
            {

                return View(response.Result.Data);
            }
            // Log the ending of the Index method execution.
            _logger.LogInformation("RedirectTenant  method call ended");
            return NotFound(response.Result.Message);
        }




        [HttpGet("Home")]
        public async Task<IActionResult> Home(string PageName)
        {
            // Log the starting of the Index method execution.
            _logger.LogInformation("Home method call started");
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
            // Log the ending of the Index method execution.
            _logger.LogInformation("Home method call ended");
            return NotFound(tenanatResponse.Message);
        }

        [HttpGet("/{action?}")]
        public async Task<IActionResult> Home()
        {
            // Log the started of the Index method execution.
            _logger.LogInformation("Home method call started");
            var tenantName = TenantName;
            if (tenantName == null)
            {
                return NotFound("Tenant name is missing");
            }
            var tenanatResponse = await _informationService.GetTenantPageNameForUserAsync(tenantName);
            if (tenanatResponse.Success)
            {
                var pageName = tenanatResponse.Data.PageName;
                var tenantId = tenanatResponse.Data.Id;
                if (pageName == null)
                {
                    //return NotFound("The requested page name was not found.");
                    return View();
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
                    return View();
                }

                string htmlContent;
                try
                {
                    htmlContent = await System.IO.File.ReadAllTextAsync(filepath);
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"IOException: {ex.Message}");
                    return View();
                }

            
                ViewBag.PageName = fileNameWithExtension;
                ViewBag.TenantId = tenantId;
                TempData["TenantId"] = tenantId;
                TempData["PageName"] = fileNameWithExtension;

                HttpContext.Session.SetString("TenantId", tenantId.ToString());
                HttpContext.Session.SetString("PageName", fileNameWithExtension);
                HttpContext.Session.SetString("TenantImage", tenanatResponse.Data?.Logo);
                return View();
            }           
            else
            {
                // Log the ended of the Index method execution.
                _logger.LogInformation("Home method call ended");
                ViewBag.ErrorMessage = "Tenant Not Found.";
                return View("Error");
            }

            
        }


        [HttpGet("GetTenant")]
        public async Task<IActionResult> GetTenantIdByName(string tenantName)
        {
            // Log the starting of the Index method execution.
            _logger.LogInformation("GetTenantIdByName method call started");
            var response = await _informationService.GetTenantByNameAsync(tenantName);
            if (response.Success)
            {
                return Ok(response.Data);
            }
            // Log the ending of the Index method execution.
            _logger.LogInformation("GetTenantIdByName method call ended");
            return NotFound(response.Message);
        }


        [HttpGet("GetTenantByUserEmail")]
        public async Task<IActionResult> GetTenantIdByEmail(string userEmail, string tenantname)
        {
            var response = await _informationService.GetTenantIdByEmail(userEmail, tenantname);
            if (response.Success)
            {
                return Ok(response.Data);
            }
            // Log the ending of the Index method execution.
            _logger.LogInformation("GetTenantIdByEmail method call ended");
            return NotFound(response.Message);
        }


    }
}