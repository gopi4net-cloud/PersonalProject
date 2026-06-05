using Amazon.Runtime.Internal.Util;
using ANGDDEAPI.Common;
using ANGDDEAPIBO;
using ANGDDEAPIDO;
using ANGDDEAPIDO.Interface;
using ANGDDEAPIFoundation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ANGDDEAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValueController : ControllerBase
    {
        ExceptionTypes _resException;
        public static long LoggedInUserId = 0;
        public string _username = string.Empty;
        public string source = string.Empty;
        public string ModuleName = "ORSAPIController";

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DOConfiguration _objConfiguration;
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        private readonly BOCommon _objBOCommon;
        private readonly ICacheService _cache;

        public ValueController(IHttpContextAccessor httpContextAccessor, DOConfiguration objConfiguration, IMemoryCacheHelper memoryCacheHelper, BOCommon bOCommon, ICacheService cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _objConfiguration = objConfiguration;
            _memoryCacheHelper = memoryCacheHelper;
            _objBOCommon = bOCommon;
            _cache = cache;
            APIAuthorization.Instance.SetDOConfiguration(_objConfiguration);
        }

        // GET api/<ValueController>/5
        [HttpGet]
        public ActionResult Get()
        {
            var token = APIAuthorization.Instance.GetATSNonProdToken();
            var formattedToken = JsonConvert.SerializeObject(token, Formatting.Indented);
            var htmlContent = $"<pre>{formattedToken}</pre>";
            return Content(htmlContent, "text/html");
        }
    }
}