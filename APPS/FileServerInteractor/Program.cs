global using WinterRose.ImGuiApps;
global using gui = ImGuiNET.ImGui;
global using ImGuiNET;

using FileServerInteractor.ImGuiWindows;


namespace FileServerInteractor
{
    internal class Program : Application
    {
        static void Main(string[] args)
        {
            try
            {
                var p = new Program();
                p.AskBeforeExit = false;
                p.AddWindow(new ConnectionWindow());
                Task appTask = p.Run();

                appTask.Wait();
            }
            catch (Exception e) 
            {
                var type = e.GetType();
            }
        }
    }
}
