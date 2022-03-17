using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Test_Login.Startup))]
namespace Test_Login
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
