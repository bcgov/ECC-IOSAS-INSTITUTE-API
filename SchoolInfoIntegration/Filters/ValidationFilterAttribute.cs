using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ECC.Institute.CRM.IntegrationAPI.Filters
{
    public class ValidationFilterAttribute : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                System.Console.WriteLine("ValidationFilterAttribute: Validation error");
                context.Result = new UnprocessableEntityObjectResult(context.ModelState);
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

