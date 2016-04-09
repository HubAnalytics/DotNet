﻿using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using HubAnalytics.Core;

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
        public HttpCorrelatorAttribute(MicroserviceAnalyticClientFactory microserviceAnalyticClientFactory = null)
        {
            if (microserviceAnalyticClientFactory == null)
            {
                microserviceAnalyticClientFactory = new MicroserviceAnalyticClientFactory();
            }
            _correlationIdName = microserviceAnalyticClientFactory.GetClientConfiguration().CorrelationIdKey;
            _userIdName = microserviceAnalyticClientFactory.GetClientConfiguration().UserIdKey;
            _sessionIdName = microserviceAnalyticClientFactory.GetClientConfiguration().SessionIdKey;
            _contextualIdProvider = microserviceAnalyticClientFactory.GetCorrelationIdProvider();
        }

        /// <summary>
        /// Moves the correlation ID, if it exists, into the call context using a object key of the header name
        /// </summary>
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            IEnumerable<string> headerValues = actionContext.Request.Headers.GetValues(_correlationIdName);
            if (headerValues != null && headerValues.Any())
            {
                _contextualIdProvider.CorrelationId = headerValues.First();
            }
            headerValues = actionContext.Request.Headers.GetValues(_userIdName);
            if (headerValues != null && headerValues.Any())
            {
                _contextualIdProvider.UserId = headerValues.First();
            }
            headerValues = actionContext.Request.Headers.GetValues(_sessionIdName);
            if (headerValues != null && headerValues.Any())
            {
                _contextualIdProvider.SessionId = headerValues.First();
            }
        }
    }
}