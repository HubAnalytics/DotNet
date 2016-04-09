using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using HubAnalytics.Core;

namespace HubAnalytics.WebAPI2
{
    public class ExceptionCaptureFilter : ExceptionFilterAttribute
    {
        private readonly IHubAnalyticsClient _recorder;

        public ExceptionCaptureFilter(IHubAnalyticsClientFactory hubAnalyticsClientFactory = null)
        {
            if (hubAnalyticsClientFactory == null)
            {
                hubAnalyticsClientFactory = new HubAnalyticsClientFactory();
            }

            _recorder = hubAnalyticsClientFactory.GetClient();
        }


        public override async Task OnExceptionAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            Dictionary<string, string> additionalData = new Dictionary<string, string>
            {
                { "action", actionExecutedContext.ActionContext.ActionDescriptor.ActionName },
                {"controller", actionExecutedContext.ActionContext.ControllerContext.ControllerDescriptor.ControllerName }
            };

            _recorder.Error(actionExecutedContext.Exception, additionalData);
            await Task.FromResult(0);
        }
    }
}
