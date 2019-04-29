using Microsoft.Owin;
using Owin;
using Teema;

namespace Teema {
    public class Startup {
        public void Configuration(IAppBuilder app) {
            app.MapSignalR();
        }
    }
}