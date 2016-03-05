using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using MicroserviceAnalytics.Core;

namespace MicroserviceAnalytics.MVC5
{
    /// <summary>
    /// This action filter attribute works with the HttpCorrelator (and HttpLogger) in AccidentalFish.ApplicationSupport.Owin
    /// to pull the correlation ID out of the header and set it in the call context.
    /// 
    /// The reason this is necessary is that when hosting MVC in IIS a new call context scope is created between OWIN and
    /// the controller being invoked and therefore any call context values set in the OWIN middleware are not visible within Web API
    /// controllers and beyond.
    /// 
    /// The easiest way to use this is to add it to the global filter set which in a typical MVC project is configured
    /// in App_Start\FilterConfig.cs:
    ///     filters.Add(new HttpCorrelatorAttribute());
    /// </summary>
    public class HttpCorrelatorAttribute : ActionFilterAttribute
    {
        private readonly IContextualIdProvider _contextualIdProvider;
        private readonly string _correlationIdName;

        /// <summary>
        /// Constructor - defaults to a correlation header name of correlation-id
        /// </summary>
        public HttpCorrelatorAttribute(MicroserviceAnalyticClientFactory microserviceAnalyticClientFactory = null)
        {
            if (microserviceAnalyticClientFactory == null)
            {
                microserviceAnalyticClientFactory = new MicroserviceAnalyticClientFactory();
            }
            _contextualIdProvider = microserviceAnalyticClientFactory.GetCorrelationIdProvider();
            _correlationIdName = microserviceAnalyticClientFactory.GetClientConfiguration().CorrelationIdKey;
        }

        /// <summary>
        /// Moves the correlation ID, if it exists, into the call context using a object key of the header name
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            IEnumerable<string> headerValues = filterContext.HttpContext.Request.Headers.GetValues(_correlationIdName);
            if (headerValues != null && headerValues.Any())
            {
                _contextualIdProvider.CorrelationId = headerValues.First();
            }
        }
    }
}
