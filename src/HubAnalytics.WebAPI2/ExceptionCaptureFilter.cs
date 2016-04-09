using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using HubAnalytics.Core;

namespace HubAnalytics.WebAPI2
{
    public class ExceptionCaptureFilter : ExceptionFilterAttribute
    {
        private readonly IMicroserviceAnalyticClient _recorder;

        public ExceptionCaptureFilter(MicroserviceAnalyticClientFactory microserviceAnalyticClientFactory = null)
        {
            if (microserviceAnalyticClientFactory == null)
            {
                microserviceAnalyticClientFactory = new MicroserviceAnalyticClientFactory();
            }

            _recorder = microserviceAnalyticClientFactory.GetClient();
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
