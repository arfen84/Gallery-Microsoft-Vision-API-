using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Vision_API_App.Startup))]
namespace Vision_API_App
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
