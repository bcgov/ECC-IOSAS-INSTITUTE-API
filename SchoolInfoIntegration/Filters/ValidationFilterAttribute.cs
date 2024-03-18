using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ECC.Institute.CRM.IntegrationAPI.Filters
{
    public class ValidationFilterAttribute : IActionFilter
    {
        public static string ConvertModelStateErrorsToString(ModelStateDictionary modelState)
        {
            return string.Join(" | ", modelState.Values
            .SelectMany(x => x.Errors)
            .Select(x => x.ErrorMessage));
        }
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                context.Result = new UnprocessableEntityObjectResult(context.ModelState);
                System.Console.WriteLine($"ValidationFilterAttribute: Validation error: {ConvertModelStateErrorsToString(context.ModelState)}");
            } else if (context.ActionArguments.ContainsKey("applicationName"))
            {
                var applicationName = context.ActionArguments["applicationName"];
                string[] validValues = { "iosas", "isfs" };
                if (!validValues.Contains(applicationName))
                {
                    context.Result = new UnprocessableEntityObjectResult("The application name should be iosas or isf");
                }
            }
        }
        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}

