using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MVC4.Sample.Startup))]
namespace MVC4.Sample
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);            
        }
    }
}
