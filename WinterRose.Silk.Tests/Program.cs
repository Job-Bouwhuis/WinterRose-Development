using WinterRose;
using WinterRose.Recordium;
using WinterRose.SilkEngine;
using WinterRose.SilkEngine.Windowing;

LogDestinations.AddDestination(new FileLogDestination("Logs"));

var engine = new ForgeWarden();

var gameLogic1 = new Game();
var window1 = engine.AddWindow(new EngineWindow("Game 1", 800, 600, gameLogic1));

var gameLogic2 = new Game();
var window2 = engine.AddWindow(new EngineWindow("Game 2", 1024, 768, gameLogic2));

engine.Run();
