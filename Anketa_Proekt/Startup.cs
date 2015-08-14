using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Anketa_Proekt.Startup))]
namespace Anketa_Proekt
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
