using Microsoft.AspNetCore.Mvc;

namespace FactoryOps_AccessManagementService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestExceptionController : ControllerBase
    {
        [HttpGet("throw")]
        public IActionResult ThrowException()
        {
            throw new Exception("Test Manual Exception for Middleware");
        }

        [HttpGet("nullref")]
        public IActionResult ThrowNullReference()
        {
            string? name = null;
            var len = name.Length; // Null Reference Exception
            return Ok();
        }

        //[HttpGet("divide")]
        //public IActionResult DivideByZero()
        //{
        //    var x = 10 / 0; // Divide by Zero Exception
        //    return Ok();
        //}

        [HttpGet("db-error")]
        public IActionResult DatabaseError()
        {
            throw new InvalidOperationException("Database connection failed");
        }
    }
}
