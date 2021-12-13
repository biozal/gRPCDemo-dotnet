using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Grpc.Net.Client;
using gRPCDemoClient;
using Terminal.Gui;

//setup terminal.guid
Console.OutputEncoding = Encoding.Default;
if (Debugger.IsAttached)
    CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("en-US");

Application.Init();
var mainWindow = new MainWindow();
Application.Run();