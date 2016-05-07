using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using HubAnalytics.Core;
using HubAnalytics.Core.Helpers;

namespace HubAnalytics.WebAPI2
{
    /// <summary>
    /// This action filter attribute works with the HttpCorrelator (and HttpLogger) in AccidentalFish.ApplicationSupport.Owin
    /// to pull the correlation IDs out of the headers and set them in the call context.
    /// 
    /// The reason this is necessary is that when hosting Web API in IIS a new call context scope is created between OWIN and
    /// the controller being invoked and therefore any call context values set in the OWIN middleware are not visible within Web API
    /// controllers and beyond.
    /// 
    /// The easiest way to use this is to add it to the global filter set which in a typical Web API project is configured
    /// in App_Start\WebAPiConfig.cs:
    ///     config.Filters.Add(new HttpCorrelatorAttribute());
    /// </summary>
    public class HttpCorrelatorAttribute : ActionFilterAttribute
    {
        private readonly string _correlationIdName;
        private readonly string _userIdName;
        private readonly string _sessionIdName;
        private readonly IContextualIdProvider _contextualIdProvider;

        /// <summary>
        /// Constructor - defaults to a correlation header name of correlation-id
        /// </summary>
        public HttpCorrelatorAttribute(IHubAnalyticsClientFactory hubAnalyticsClientFactory = null)
        {
            if (hubAnalyticsClientFactory == null)
            {
                hubAnalyticsClientFactory = new HubAnalyticsClientFactory();
            }
            _correlationIdName = hubAnalyticsClientFactory.GetClientConfiguration().CorrelationIdKey;
            _userIdName = hubAnalyticsClientFactory.GetClientConfiguration().UserIdKey;
            _sessionIdName = hubAnalyticsClientFactory.GetClientConfiguration().SessionIdKey;
            _contextualIdProvider = hubAnalyticsClientFactory.GetCorrelationIdProvider();
        }

        /// <summary>
        /// Moves the correlation ID, if it exists, into the call context using a object key of the header name
        /// </summary>
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            IEnumerable<string> headerValues;
            if (actionContext.Request.Headers.TryGetValues(_correlationIdName, out headerValues))
            {
                _contextualIdProvider.CorrelationId = headerValues?.First();
            }
            if (actionContext.Request.Headers.TryGetValues(_userIdName, out headerValues))
            {
                _contextualIdProvider.UserId = headerValues?.First();
            }
            if (actionContext.Request.Headers.TryGetValues(_sessionIdName, out headerValues))
            {
                _contextualIdProvider.SessionId = headerValues?.First();
            }            
        }
    }
}
