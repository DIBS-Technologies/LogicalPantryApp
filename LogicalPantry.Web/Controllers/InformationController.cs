using LogicalPantry.Services.InformationService;
using Microsoft.AspNetCore.Mvc;

namespace LogicalPantry.Web.Controllers
{
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
        public object Get(int tenantid) 
        {
            if (tenantid == 0) { return null; }
            var response=_informationService.GetTenant(tenantid).Result;
            return response;
        }

    }
}
