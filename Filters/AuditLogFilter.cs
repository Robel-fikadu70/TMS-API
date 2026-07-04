using Microsoft.AspNetCore.Mvc.Filters;

namespace TmsApi.Filters;

public class AuditLogFilter(ILogger<AuditLogFilter> logger) : IActionFilter
{
    // Runs BEFORE the action (controller method) starts
    public void OnActionExecuting(ActionExecutingContext context)
    {
        var route = context.HttpContext.Request.Path;
        var method = context.HttpContext.Request.Method;
        logger.LogInformation("TMS API call: {Method} {Route}", method, route);
    }

    // Runs AFTER the action finishes
    public void OnActionExecuted(ActionExecutedContext context)
    {
        var status = context.HttpContext.Response.StatusCode;
        logger.LogInformation("TMS API response: {StatusCode}", status);
    }
}
