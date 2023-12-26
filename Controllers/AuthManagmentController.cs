using Microsoft.AspNetCore.Mvc;

namespace DriversApi.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AuthManagmentController : ControllerBase
    {
        private readonly ILogger<AuthManagmentController> _logger;
        public AuthManagmentController(ILogger<AuthManagmentController> logger)
        {
            _logger = logger;
        }
    }
}
