using LogicalPantry.Services.InformationService;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using static System.Net.WebRequestMethods;

namespace LogicalPantry.Web.Controllers
{
    public class InformationController : Controller
    {
        IInformationService _informationService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public InformationController(IInformationService informationService, IWebHostEnvironment webHostEnvironment)
        {
            _informationService = informationService;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public object Get(int tenantid) 
        {
            if (tenantid == 0) { return null; }
            var response=_informationService.GetTenant(tenantid).Result;
            return response;
        }


        /// <summary>
        /// For anonymous page - 2-08-2024 kunal karne
        /// </summary>
        /// <param name="PageName"></param>
        /// <returns></returns>

        public async Task<IActionResult> Index1(string PageName)
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

                var tenantFolderPath = Path.Combine(_webHostEnvironment.WebRootPath, "TenantPages");
                var filepath = Path.Combine(tenantFolderPath, pageName);

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

                return View("Index1", model: htmlContent);
            }

            return NotFound(tenanatResponse.Message);
        }




    }
}
