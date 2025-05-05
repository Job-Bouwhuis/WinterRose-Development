using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WinterRose.FileManagement;
using WinterRose.Legacy.Serialization.Things;

namespace WinterRose.Legacy.Serialization
{
    /// <summary>
    /// Use only if you know what you are doing, this class is not meant to be used by the end user.
    /// <br></br> it was made public so it can be used by the <see cref="SerializationGenerator"/> class.
    /// </summary>
    public static class SnowSerializerDistributors
    {
        public static StringBuilder DistributeSerializationData<T>(T item, SerializerSettings? settings, SerializeReferenceCache refCache)
        {
            settings ??= new SerializerSettings();
            SerializeAsAttributeINTERNAL? serializeAs = item.GetType().GetCustomAttribute<SerializeAsAttributeINTERNAL>();

            if (item is IEnumerable && serializeAs == null || serializeAs?.Type is IEnumerable)
            {
                IEnumerable enumerable = (IEnumerable)item;

                if (enumerable is IDictionary)
                    throw new Exception("Dictionary serialization is not supported yet.");

                //TODO: create serializing and deserializing list methods
                Type itemListType = item.GetType();
                Type itemType = itemListType.IsArray ? itemListType.GetElementType() : itemListType.GetGenericArguments().First();

                var list = WinterUtils.CreateList(itemType);
                foreach (object i in enumerable)
                    list.Add(i);

                return (StringBuilder)typeof(SnowSerializerDistributors).GetMethod(nameof(DistributeListSerialization),
                    1,
                    [typeof(IList), typeof(SerializerSettings), typeof(SerializeReferenceCache)])!
                    .MakeGenericMethod(itemType)
                    .Invoke(null, [list, settings, refCache])!;
            }

            return SnowSerializerWorkers.SerializeObject(item, 0, settings, refCache);
        }
        public static StringBuilder DistributeListSerialization<T>(IList list, SerializerSettings? settings, SerializeReferenceCache refCache)
        {
            Type typeofT = typeof(T);
            List<T> items = WinterUtils.CreateList<T>(list);
            settings ??= new SerializerSettings();
            StringBuilder result = new();
            if (settings.TheadsToUse == 1)
            {
                int handled = 0;
                foreach (T item in items)
                {
                    if (handled % settings.ReportEvery == 0)
                        settings.ProgressReporter?.Invoke(new ProgressReporter((float)MathS.GetPercentage(handled, items.Count, 2), $"Handling item {handled} of {items.Count}."));
                    result.Append(SnowSerializer.Serialize(item, settings));
                    handled++;
                }
                return result;
            }
            else
            {
                List<T>[] partitions = items.Partition(settings?.TheadsToUse ?? SnowSerializer.DEFAULT_THREADS_TO_USE);
                List<Task<string>> tasks = new List<Task<string>>();
                partitions.Where(x => x.Count > 0)
                    .ToArray()
                    .Foreach((x, i) =>
                    tasks.Add(Task.Run(() => DistributeSerializationAsync(x, settings ?? new SerializerSettings(), refCache))));

                settings?.ProgressReporter?.Invoke(new ProgressReporter(0,
                    $"Running operation on {partitions.Length} {(settings.TheadsToUse == 1 ? "thread" : "threads")} for {items.Count} items." +
                $"handing {items.Count / partitions.Length} items per thread."));

                Task.WhenAll(tasks).Wait();

                settings?.ProgressReporter?.Invoke(new ProgressReporter(100, $"Finnishing up..."));

                tasks.Foreach(x => result.Append(x.Result));
                return result;
            }
        }
        public async static Task<string> DistributeSerializationAsync<T>(List<T> items, SerializerSettings? settings, SerializeReferenceCache refCache)
        {
            settings ??= new SerializerSettings();
            return await Task.Run(() =>
            {
                StringBuilder result = new();
                for (int i = 0; i < items.Count; i++)
                {
                    result.Append(SnowSerializerWorkers.SerializeObject(items[i], 0, settings, refCache));
                    if (settings?.ProgressReporter != null && i % settings.ReportEvery == 0 && i != 0)
                        settings.ProgressReporter.Invoke(
                            new ProgressReporter((float)MathS.GetPercentage(i * settings.TheadsToUse, items.Count * settings.TheadsToUse, 2), $"aproximately {i * settings.TheadsToUse} entries completed from the {items.Count * settings.TheadsToUse}"));

                }
                return result.ToString();
            });
        }

        public async static Task<dynamic> DistributeDeserializationAsync<T>(List<string> items, SerializerSettings? settings, SerializeReferenceCache refCache)
        {
            Type typeofT = typeof(T);

            Type objType = typeofT;
            if (objType.IsAssignableTo(typeof(IEnumerable)))
            {
                if (objType.IsArray)
                    objType = objType.GetElementType();
                else
                    objType = objType.GetGenericArguments().First();
            }

            settings ??= new SerializerSettings();
            return await Task.Run(() =>
            {
                List<dynamic> result = new List<dynamic>();
                for (int i = 0; i < items.Count; i++)
                {
                    result.Add(SnowSerializerWorkers.DeserializeObject<T>(items[i], 0, objType, settings, refCache));
                    if (settings?.ProgressReporter != null && i % settings.ReportEvery == 0 && i != 0)
                        settings.ProgressReporter.Invoke(
                            new ProgressReporter((float)MathS.GetPercentage(i * settings.TheadsToUse, items.Count * settings.TheadsToUse, 2), $"aproximately {i * settings.TheadsToUse} entries completed from the {items.Count * settings.TheadsToUse}"));
                }
                return result;
            });
        }
        public static dynamic DistributeDeserializationData<T>(string data, SerializerSettings? settings, SerializeReferenceCache refCache)
        {
            settings ??= new SerializerSettings();
            Type type = typeof(T);
            SerializeAsAttributeINTERNAL? serializeAs = type.GetCustomAttribute<SerializeAsAttributeINTERNAL>();
            if (serializeAs is not null)
                type = serializeAs.Type;
            Type itemType = typeof(object);

            if (typeof(T).IsArray)
                itemType = type.GetElementType();
            else
            {
                if (serializeAs is not null && (serializeAs.Type.IsAssignableTo(typeof(IEnumerable)) || serializeAs.Type.IsAssignableTo(typeof(IList))))
                    itemType = type;
                else if (typeof(T).IsAssignableTo(typeof(IEnumerable)) || typeof(T).IsAssignableTo(typeof(IList)))
                    itemType = type.GetGenericArguments().First();
                else
                    itemType = type;
            }


            if (type == WinterUtils.CreateList(itemType).GetType())
            {
                return typeof(SnowSerializerDistributors).GetMethod(nameof(DistributeDeserializationListData), 1, [typeof(string), typeof(SerializerSettings), typeof(SerializeReferenceCache)])!
                    .MakeGenericMethod(itemType)
                    .Invoke(null, [data, settings, refCache])!;
                //return DistributeDeserializationListData<T>(data, settings);
            }

            return SnowSerializerWorkers.DeserializeObject<T>(data, 0, itemType, settings, refCache);
        }
        public static List<T> DistributeDeserializationListData<T>(string data, SerializerSettings? settings, SerializeReferenceCache refCache)
        {
            Type typeofT = typeof(T);
            settings ??= new SerializerSettings();
            string[] items = data.Split("@0", StringSplitOptions.RemoveEmptyEntries);
            items = items.Foreach(x => "@0" + x);
            List<string>[] partitions = items.Partition(settings?.TheadsToUse ?? SnowSerializer.DEFAULT_THREADS_TO_USE);
            List<Task<dynamic>> tasks = new List<Task<dynamic>>();
            partitions.Where(x => x.Count != 0).Foreach(x => tasks.Add(Task.Run(() => DistributeDeserializationAsync<T>(x, settings, refCache))));

            settings?.ProgressReporter?.Invoke(new ProgressReporter(0,
                $"Running operation on {partitions.Length} {(settings.TheadsToUse == 1 ? "thread" : "threads")} for {items.Length} items." +
            $"handing {items.Length / settings.TheadsToUse} items per thread."));

            Task.WhenAll(tasks).Wait();

            settings?.ProgressReporter?.Invoke(new ProgressReporter(100, $"Finnishing up..."));

            List<T> result = new List<T>();
            tasks.Foreach(x =>
            {
                foreach (T d in x.Result)
                {
                    result.Add(d);
                }
            });
            return result;
        }
    }
}