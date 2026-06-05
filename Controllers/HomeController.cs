using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO;
using ANGDDEAPIDO.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ANGDDEAPI.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly ICacheService _cache;
        public readonly BOCommon _objBOCommon;
        public readonly IMemoryCacheHelper _memory;
        public HomeController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, ICacheService cache, BOCommon bOCommon, IMemoryCacheHelper memory)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _cache = cache;
            _objBOCommon = bOCommon;
            _memory = memory;
        }
        public IActionResult Index()
        {
            return View();
        }
 
        [Route("api/[controller]/[action]")]
        public ActionResult healthcheck()
        {
            return Ok("Success. Service is Up and Running.");
        }

 
    }
    
}
