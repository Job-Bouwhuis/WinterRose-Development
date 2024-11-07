using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Reflection;
using WinterRose.Serialization.Version2;
using gui = ImGuiNET.ImGui;

namespace WinterRose.Monogame.Tests
{
    internal class SerializerTestsLayout : ImGuiLayout
    {
        Dictionary<int, ProgressLog> _progressLogs;
        List<Vectors.Vector3> vectors;

        int totalHandled = 0;
        bool serializing = false;
        bool deserializing = false;
        Serialization.SerializationResult serializeResult;
        TimeSpan operationTime;

        int numItems = 1000000;
        int threads = 4;
        int logInterval = 1000;

        private void Awake()
        {
            vectors = new();
            numItems.Repeat(i => vectors.Add(new Vectors.Vector3(i, i, i)));

            SnowSerializer.Default.SimpleLogger += Default_SimpleLogger;
            SnowSerializer.Default.WithOptions(options => options.WithSimpleLoggingInterval(logInterval).WithMaxThreads(threads));
        }

        private void Default_SimpleLogger(ProgressLog log)
        {
            if (log.Thread != -1)
            {
                _progressLogs[log.Thread] = log;
            }
        }

        public override void RenderLayout()
        {
            gui.Begin("Serialize/DeserializeProgress", ImGuiNET.ImGuiWindowFlags.AlwaysVerticalScrollbar);
            gui.Text($"Total items: {vectors.Count}");
            gui.Text($"Operation Time: {operationTime}");
            if(gui.Button("Restart App"))
            {
                MonoUtils.RestartApp();
            }
            if (!serializing && !deserializing)
            {
                if(gui.SliderInt("Number of Items", ref numItems, 1, 50000000))
                {
                    vectors = new();
                    numItems.Repeat(i => vectors.Add(new Vectors.Vector3(i, i, i)));
                    GC.Collect();
                }
                if (gui.SliderInt("Max Threads", ref threads, 1, 32))
                    SnowSerializer.Default.WithOptions(options => options.WithMaxThreads(threads));
                if(gui.SliderInt("Log Interval", ref logInterval, 1, 2000))
                    SnowSerializer.Default.WithOptions(options => options.WithSimpleLoggingInterval(logInterval));
                if (gui.Button("Serialize"))
                {
                    serializing = true;
                    _progressLogs = [];
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    Task.Run(() =>
                    {
                        serializeResult = SnowSerializer.Default.Serialize(vectors);
                        serializing = false;
                        operationTime = stopwatch.Elapsed;
                        stopwatch.Stop();
                    });
                }

                if (gui.Button("Deserialize"))
                {
                    deserializing = true;
                    _progressLogs = [];
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    Task.Run(() =>
                    {
                        _ = SnowSerializer.Default.Deserialize<List<Vectors.Vector3>>(serializeResult.Result);
                        deserializing = false;
                        operationTime = stopwatch.Elapsed;
                        stopwatch.Stop();
                    });
                }
            }
            else
            {
                totalHandled = 0;

                if(SnowSerializer.Default.Serializer.Paused)
                {
                    if(gui.Button("Resume"))
                    {
                        SnowSerializer.Default.Serializer.Resume();
                        SnowSerializer.Default.Deserializer.Resume();
                    }
                }
                else
                {
                    if(gui.Button("Pause"))
                    {
                        SnowSerializer.Default.Serializer.Pause();
                        SnowSerializer.Default.Deserializer.Pause();
                    }
                }

                if (gui.Button("Abort"))
                {
                    SnowSerializer.Default.Serializer.Abort();
                    SnowSerializer.Default.Deserializer.Abort();
                }

                ProgressLog[] logs = [.. _progressLogs.Values];
                logs = [.. logs.OrderBy(x => x.Completed).ThenBy(x => x.Thread)];

                if( logs.Length is 0)
                {
                    gui.ProgressBar(0, new(gui.GetContentRegionAvail().X, 25), [.. $"No Logs..."]);
                }
                else
                {
                    float avarage = logs.Average(x => x.ProgressPercentage);
                    int text = (avarage * 100).FloorToInt();
                    gui.ProgressBar(avarage, new(gui.GetContentRegionAvail().X, 25), [.. $"Avarage: {text}%"]);
                }

                gui.Separator();
                gui.Separator();
                
                foreach (var log in logs)
                {
                    if(log.Total is 0)
                    {
                        gui.TextColored(gui.GetStyle().Colors[(int)ImGuiNET.ImGuiCol.TextDisabled], $"{log.Message}");
                        continue;
                    }

                    if (log.ProgressPercentage is 1)
                    {
                        gui.TextColored(new(0, 1, 0, 1), $"{log.Message}");
                        continue;
                    }
                    gui.Text($"Thread: {log.Thread} > {log.Progress}/{log.Total}");
                    gui.ProgressBar(log.ProgressPercentage, new(gui.GetContentRegionAvail().X, 25));
                    totalHandled += log.Progress;
                }
                gui.TextColored(new(1, 0, 0, 1), $"Finished a total of {totalHandled} items");
            }

            gui.End();
        }
    }
}
