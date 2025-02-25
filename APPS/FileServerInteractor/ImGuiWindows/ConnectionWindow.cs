using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WinterRose.FileServer;
using WinterRose.Networking;

namespace FileServerInteractor.ImGuiWindows
{
    internal class ConnectionWindow : ImGuiWindow
    {
        string ipInput = "";
        string portInput = "";

        IPAddress ip;
        int port;

        bool once = true;

        public override void Render()
        {
            if(once)
            {
                Style.ApplyDefault();
                once = false;
            }

            gui.InputText("IP", ref ipInput, 100);
            if(gui.Button("This machine"))
            {
                ipInput = NetworkUtils.GetLocalIPAddress().ToString();
            }

            bool canConnect;

            if (!(canConnect = IPAddress.TryParse(ipInput, out ip)))
                gui.TextColored(new(1, 0, 0, 1), "Invalid IP");


            gui.InputText("Port", ref portInput, 100);
            if (!(canConnect && int.TryParse(portInput, out port)))
                gui.TextColored(new(1, 0, 0, 1), "Invalid port");

            gui.Text("");
            gui.Separator();
            gui.Text("");

            if(!canConnect)
            {
                gui.Text("Cannot connect. Some values are invalid");
                return;
            }
            if(gui.Button("Connect"))
            {
                FileServerClient client = new FileServerClient(IPAddress.Parse(ipInput), int.Parse(portInput));
                Globals.client = client;
                Application.AddWindow(new ServerFileExplorer());
                Close();
            }
        }
    }
}
