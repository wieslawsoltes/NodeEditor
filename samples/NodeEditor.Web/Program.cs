using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;

namespace NodeEditorDemo.Web;

internal static class Program
{
    private static Task Main(string[] args) => AppBuilder.Configure<BrowserApp>()
        .WithInterFont()
        .StartBrowserAppAsync("out");
}
