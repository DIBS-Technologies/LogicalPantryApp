﻿using LogicalPantry.DTOs.TenantDtos;
using LogicalPantry.Services.InformationService;
using LogicalPantry.Services.TenantServices;
using Microsoft.AspNetCore.Mvc;

namespace LogicalPantry.Web.Controllers
{
    [Route("Tenant")]
    public class TenantController : BaseController
    {

        private readonly ITenantService _tenantService;
        private readonly IInformationService _infotenantService;


        private readonly ILogger _logger;
        public TenantController(ITenantService tenantService, ILogger<TenantController> logger, IInformationService infotenantService)
        {
            _tenantService = tenantService;
            _infotenantService = infotenantService;
            _logger = logger;
        }


        public IActionResult Index(string pageName)
        {
            _logger.LogInformation($"Index method call started.");
            _logger.LogInformation($"Index method call ended.");
            return View();
        }



        [HttpGet]
        [Route("AddTenant")]
        public IActionResult AddTenant()
        {
            _logger.LogInformation($"AddTenant method call started");
            _logger.LogInformation($"AddTenant method call ended");
            return View();
        }


        // Display the form for editing
        [HttpPost("AddTenat")]
        public async Task<IActionResult> EditTenant(int id)
        {
            _logger.LogInformation($"EditTenant get method call started");
            var tenantResponse = await _tenantService.GetTenantByIdAsync(id);
            if (tenantResponse.Success)
            {
                return View(tenantResponse.Data);
            }

            // Handle the case where the tenant is not found
            _logger.LogInformation($"EditTenant get  method call ended");
            return NotFound(tenantResponse.Message);

        }

        // Handle form submission
        [HttpPost("EditTenant")]
        public async Task<IActionResult> EditTenant(TenantDto tenantDto, IFormFile LogoFile)
        {
            _logger.LogInformation($"EditTenant post method call started");

            if (ModelState.IsValid)
            {
                if (LogoFile != null && LogoFile.Length > 0)
                {
                    // Generate a unique file name to avoid conflicts
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(LogoFile.FileName);
                    var filePath = Path.Combine("wwwroot/Pages", fileName);

                    //            // Save the file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await LogoFile.CopyToAsync(stream);
                    }

                    // Update the tenantDto with the new logo path
                    tenantDto.Logo = "/Pages/" + fileName;
                }

                var response = await _infotenantService.PostTenant(tenantDto);
                    if (response.Success)
                    {
                        // Redirect to the GET method to display the updated data
                        return RedirectToAction(nameof(EditTenant), new { id = tenantDto.Id });
                    }
                    else
                    {
                        ModelState.AddModelError("", response.Message);
                    }
                    _logger.LogInformation($"EditTenant post method call ended");

                }

                return View(tenantDto);
            }
        }
     }
    


