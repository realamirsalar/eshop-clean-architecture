using E_Shop.Application.Common.Errors;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace E_Shop.Api.Controllers
{

    [ApiController]
    public class ApiControllerBase : ControllerBase
    {
        protected IActionResult HandleResult<T>(Result<T> result)
        {
            if (result.IsSuccess)
            {
                if (result.Value is null) return NoContent();
                return Ok(result.Value);
            }

            return HandleFailure(result);
        }

        protected IActionResult HandleResult(Result result)
        {
            if (result.IsSuccess)
            {
                return NoContent();
            }

            return HandleFailure(result);
        }

        protected IActionResult HandleFailure(IResultBase result)
        {
            if (result.IsSuccess)
                throw new InvalidOperationException("متد هندل خطا نباید برای نتیجه موفق فراخوانی شود.");

            var firstError = result.Errors.FirstOrDefault();

            if (firstError == null)
                return BadRequest();

            // ۱. بررسی اینکه آیا خطا از نوع ولیدیشن است
            // (بررسی از طریق کلاس کاستوم شما یا وجود متادیتای PropertyName که در پایپ‌لاین ساختیم)
            bool isValidationError = result.Errors.Any(e => e.Metadata.ContainsKey("PropertyName"));

            if (isValidationError)
            {
                var modelState = new ModelStateDictionary();

                // در خطاهای ولیدیشن، برخلاف ارورهای بیزینسی، ما به "تمام" خطاها نیاز داریم نه فقط اولی
                foreach (var error in result.Errors)
                {
                    var propertyName = error.Metadata.TryGetValue("PropertyName", out var propName)
                        ? propName?.ToString() ?? string.Empty
                        : string.Empty;

                    modelState.AddModelError(propertyName, error.Message);
                }

                // ساخت آبجکت مخصوص ValidationProblemDetails
                var validationProblem = new ValidationProblemDetails(modelState)
                {
                    Status = 400,
                    Title = "Validation Error",
                    Instance = HttpContext.Request.Path
                };
                validationProblem.Extensions["traceId"] = HttpContext.TraceIdentifier;

                return BadRequest(validationProblem);
            }

            // ۲. هندل کردن سایر خطاهای بیزینسی (دقیقاً مثل کد خودت)
            return firstError switch
            {
                NotFoundError => NotFound(CreateProblemDetails(404, "Not Found", firstError.Message)),
                ConflictError => Conflict(CreateProblemDetails(409, "Conflict", firstError.Message)),
                _ => BadRequest(CreateProblemDetails(400, "Bad Request", firstError.Message))
            };
        }
        private ProblemDetails CreateProblemDetails(int statusCode, string title, string detail)
        {
            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = HttpContext.Request.Path
            };

            problem.Extensions["traceId"] = HttpContext.TraceIdentifier;

            return problem;
        }
    }
}
