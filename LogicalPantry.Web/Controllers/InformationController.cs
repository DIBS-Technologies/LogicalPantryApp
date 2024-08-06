using LogicalPantry.Services.InformationService;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Reflection;
using static System.Net.WebRequestMethods;

namespace LogicalPantry.Web.Controllers
{
    public class InformationController : Controller
    {
        IInformationService _informationService;
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
        [HttpGet]
        public object Get(int tenantid) 
        {
            _logger.LogInformation($"Get Object call started");

            if (tenantid == 0) { return null; }
            var response=_informationService.GetTenant(tenantid).Result;

            _logger.LogInformation($"Get Object call ended");

            return response;

        }


        /// <summary>
        /// For anonymous page - 2-08-2024 kunal karne
        /// </summary>
        /// <param name="PageName"></param>
        /// <returns></returns>

        [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any, NoStore = false)]
        public async Task<IActionResult> PageName(string PageName)
        {
            

            _logger.LogInformation($"Index1 method call started");

            // Call the service to get tenant-specific page name information
            var tenanatResponse = await _informationService.GetTenantPageNameForUserAsync(PageName);
            //check if the response indicated success
            if (tenanatResponse.Success)
            {

                //Extract the pageName from response
                var pageName = tenanatResponse.Data.PageName;

                var fileExtension = ".html";
                if (!pageName.EndsWith(fileExtension, StringComparison.OrdinalIgnoreCase))
                {
                    pageName += fileExtension;
                }

                // Build the path to the tenant-specific pages directory
                var tenantFolderPath = Path.Combine(_webHostEnvironment.WebRootPath, "TenantPages");
                var tenantFolderName = Path.GetFileName(tenantFolderPath);   //Get the folder name
                var filepath = Path.Combine(tenantFolderPath, pageName);    //Get full path of pageName
                string filenameWithExtension = Path.GetFileName(filepath);  //Get pageName with extension


                //check if file is exist
                if (!System.IO.File.Exists(filepath))
                {
                    Console.WriteLine("File not found.");
                    return NotFound("The requested page was not found.");
                }


                 string CacheKey = "PageName";

                // Try to get the page name from memory cache
                if (!_memoryCache.TryGetValue(CacheKey, out string cachedPageName))
                {
                    // If not found in cache, fetch it from the service
                    cachedPageName = tenanatResponse.Data.PageName;

                    // Set the fetched page name in the memory cache with an expiration time
                    var cacheEntryOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                        // Optionally, set other cache options like priority or sliding expiration
                        Priority = CacheItemPriority.Normal,
                        SlidingExpiration = TimeSpan.FromMinutes(2)
                    };

                    _memoryCache.Set(CacheKey, cachedPageName, cacheEntryOptions);

                    Console.WriteLine( $" store memory cashe {_memoryCache.Get("PageName")}");
                    _logger.LogInformation("PageName fetched from service and stored in cache.");
                }
                else
                {
                    _logger.LogInformation("PageName is already stored in cache.");
                }


                _logger.LogInformation($"Index1 method call Ended");

                //return View("Index1", model: htmlContent);

                return Redirect($"/{tenantFolderName}/{filenameWithExtension}");

            }
            return NotFound(tenanatResponse.Message);
        }



    }
}
