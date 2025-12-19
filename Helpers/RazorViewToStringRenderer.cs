using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

public static class RazorViewToStringRenderer
{
    public static string RenderViewToString(HttpContext httpContext, string viewName, object model)
    {
        var serviceProvider = httpContext.RequestServices;
        var actionContext = new ActionContext(httpContext, httpContext.GetRouteData(), new ActionDescriptor());

        var razorViewEngine = serviceProvider.GetService(typeof(IRazorViewEngine)) as IRazorViewEngine;
        var tempDataProvider = serviceProvider.GetService(typeof(ITempDataProvider)) as ITempDataProvider;

        using var sw = new StringWriter();

        var viewResult = razorViewEngine.FindView(actionContext, viewName, false);

        if (viewResult.View == null)
            throw new ArgumentNullException($"{viewName} not found.");

        var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
        {
            Model = model
        };

        var viewContext = new ViewContext(
            actionContext,
            viewResult.View,
            viewDictionary,
            new TempDataDictionary(httpContext, tempDataProvider),
            sw,
            new HtmlHelperOptions()
        );

        viewResult.View.RenderAsync(viewContext).Wait();
        return sw.ToString();
    }
}
