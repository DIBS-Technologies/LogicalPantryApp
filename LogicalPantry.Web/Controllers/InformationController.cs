﻿using Azure;
using LogicalPantry.DTOs.TenantDtos;
using LogicalPantry.Models.Models;
using LogicalPantry.Services.InformationService;
using LogicalPantry.Web.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

       //No need to authorization public method 
        [HttpGet("Get")]
        public async Task<IActionResult> Get(int tenantId)
        {
            _logger.LogInformation("Get Object call started");
            try
            {
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
                // return Ok(tenantDto.Logo); 
                return Ok(tenantDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting tenant.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }





        [Authorize(Roles = $"{UserRoleEnum.Admin}")]
        [HttpGet]
        [Route("AddTenant")]
        public async Task<IActionResult> AddTenant()
        {
            // Log the starting of the Index method execution.
            _logger.LogInformation("AddTenant Get method call started");
            var tenantName = TenantName;
            
            var TenantDisplayName = HttpContext.Session.GetString("TenantDisplayName");
            ViewBag.PageName = TenantDisplayName;
            var response = await _informationService.GetTenantByNameAsync(tenantName);
            // Log the ending of the Index method execution.
            _logger.LogInformation("AddTenant Get method call ended");
            return View(response.Data);
        }

        // Handle form submission
        [Authorize(Roles = $"{UserRoleEnum.Admin}")]
        [HttpPost("AddTenant")]
        public async Task<IActionResult> AddTenant(TenantDto tenantDto, IFormFile LogoFile)
        {
            // Log the starting of the Index method execution.
            _logger.LogInformation("AddTenant post method call started");
            tenantDto.TenantName = TenantName;
            var TenantDisplayName = HttpContext.Session.GetString("TenantDisplayName");
            ViewBag.PageName = TenantDisplayName;
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
                TenantDisplayName = HttpContext.Session.GetString("TenantDisplayName");
                ViewBag.PageName = TenantDisplayName;
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


        [HttpGet("RedirectTenant")]
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

                //var fileExtension = ".html";
                //if (!pageName.EndsWith(fileExtension, StringComparison.OrdinalIgnoreCase))
                //{
                //    pageName += fileExtension;
                //}

                var tenantFolderPath = Path.Combine(_webHostEnvironment.WebRootPath, "TenantHomePage");
                var filepath = Path.Combine(tenantFolderPath, pageName);
                var fileNameWithExtension = Path.GetFileName(filepath);

                Console.WriteLine($"Page Name: {pageName}");
                Console.WriteLine($"Tenant Folder Path: {tenantFolderPath}");
                Console.WriteLine($"File Path: {filepath}");

              
                var homepageName = HttpContext.Session.GetString("HomePageName");
                var TenantDisplayName = HttpContext.Session.GetString("TenantDisplayName");
                TenantDisplayName = TenantDisplayName ?? string.Empty;

                if (TenantDisplayName != null) { 
                    HttpContext.Session.SetString("TenantId", tenantId.ToString());
                    HttpContext.Session.SetString("PageName", tenanatResponse.Data.TenantDisplayName??string.Empty);
                    HttpContext.Session.SetString("HomePageName", tenanatResponse.Data.PageName);

                    ViewBag.PageName = TenantDisplayName;               
                    ViewBag.TenantId = tenantId;
                    TempData["TenantId"] = tenantId;
                    TempData["PageName"] = tenanatResponse.Data.TenantDisplayName;
                }
                if(tenanatResponse.Data?.PageName != null ||homepageName != null  )
                {
                    HttpContext.Session.SetString("HomePageName", tenanatResponse.Data?.PageName);
                    ViewBag.homepageName = tenanatResponse.Data?.PageName;
                }
                HttpContext.Session.SetString("TenantId", tenantId.ToString());
                ViewBag.TenantId = tenantId;

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


                //HttpContext.Session.SetString("TenantImage", tenanatResponse.Data?.Logo);
                return View();
            }         
            else
            {
                ViewBag.ErrorMessage = "   Page Not Found.";
                // Log the ended of the Index method execution.
                _logger.LogInformation("Home method call ended");
                return View("Error");
               // return View();
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
            // Log the starting of the Index method execution.
            _logger.LogInformation("GetTenantIdByEmail method call started");
            var response = await _informationService.GetTenantIdByEmail(userEmail,tenantname);
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