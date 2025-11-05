using Microsoft.Internal.VisualStudio.Shell.Interop;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WinterRose;
using WinterRose.ForgeGuardChecks;
using WinterRose.ForgeSignal;
using WinterRose.ForgeWarden;
using WinterRose.ForgeWarden.AssetPipeline;
using WinterRose.Recordium;
using WinterRose.WinterForgeSerializing;

namespace WinterRoseUtilityApp.NoteKeeper;
internal struct NoteManager : IAssetHandler<List<Note>>
{
    private const string ASSET_NAME = "Notes";
    #region IAssetHandler
    private static readonly Log log = new Log("NoteKeeper");

    public static string[] InterestedInExtensions => [];

    public static bool InitializeNewAsset(AssetHeader header)
    {
        throw new NotImplementedException("Asset File conversion not yet implemented");
    }

    public static List<Note> LoadAsset(AssetHeader header)
    {
        lock (SYNC_OBJECT)
        {
            var result = WinterForge.DeserializeFromFile(header.Path);
            if (result is not List<Note>)
            {
                log.Info("No prior notes found!");
                notes = [];
                return notes;
            }

            notes = Unsafe.As<List<Note>>(result)!;
            return notes;
        }
    }

    public static bool SaveAsset(AssetHeader header, List<Note> asset)
    {
        lock (SYNC_OBJECT)
        {
            try
            {
                WinterForge.SerializeToFile(asset, header.Path);
                return true;
            }
            catch (Exception ex)
            {
                log.Error(ex, "Failed to save notes");
                return false;
            }
        }
    }
    public static bool SaveAsset(string assetName, List<Note> asset)
    {
        if (!Assets.Exists(assetName))
            Assets.CreateAsset<List<Note>>([], assetName);
        return SaveAsset(Assets.GetHeader(assetName), asset);
    }
    #endregion

    private static readonly object SYNC_OBJECT = new();
    private static List<Note> notes = [];

    //private static readonly AssetHeader header;

    static NoteManager()
    {
        Application.Current.GameClosing.Subscribe(
            Invocation.Create(() =>
            { 
                SaveAsset(ASSET_NAME, notes);
                log.Warning("Last ditch save attempt!");
            }));
    }

    private AssetHeader header;

    public NoteManager()
    {
        if (notes.Count == 0)
        {
            if (Assets.Exists(ASSET_NAME))
            {
                notes = Assets.Load<List<Note>>(ASSET_NAME);
            }
            else
            {
                notes = new List<Note>();
                Assets.CreateAsset<List<Note>>([], ASSET_NAME);
            }
            header = Assets.GetHeader(ASSET_NAME);
        }    
    }

    public IReadOnlyList<Note> Notes
    {
        get
        {
            lock (SYNC_OBJECT)
                return notes.ToList().AsReadOnly();
        }
    }

    public void Add(Note note)
    {
        lock (SYNC_OBJECT)
            notes.Add(note);
    }

    public bool Remove(Note note)
    {
        lock (SYNC_OBJECT)
            return notes.Remove(note);
    }

    public void Save()
    {
        lock (SYNC_OBJECT)
            SaveAsset(header, notes);
    }

    public void Reload()
    {
        lock (SYNC_OBJECT)
            LoadAsset(header);
    }

    public List<(Note item, float score)> Search(
        string query,
        Fuzzy.ComparisonType comparisonType = Fuzzy.ComparisonType.IgnoreCase,
        float titleWeight = 2.0f,
        float bodyWeight = 1.0f)
    {
        lock (SYNC_OBJECT)
        {
            var titleResults = notes.SearchMany(query, n => n.Title, comparisonType);
            var bodyResults = notes.SearchMany(query, n => n.Body, comparisonType);

            var combined = new Dictionary<Note, float>();

            // Apply weights and combine
            foreach (var (item, score) in titleResults)
            {
                if (combined.TryGetValue(item, out float existing))
                    combined[item] = existing + (score * titleWeight);
                else
                    combined[item] = score * titleWeight;
            }

            foreach (var (item, score) in bodyResults)
            {
                if (combined.TryGetValue(item, out float existing))
                    combined[item] = existing + (score * bodyWeight);
                else
                    combined[item] = score * bodyWeight;
            }

            // Order descending by weighted score
            return combined
                .OrderByDescending(kv => kv.Value)
                .Select(kv => (item: kv.Key, score: kv.Value))
                .ToList();
        }
    }
}

