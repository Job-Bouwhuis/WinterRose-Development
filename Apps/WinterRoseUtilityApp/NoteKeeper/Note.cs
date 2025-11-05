using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRoseUtilityApp.NoteKeeper;
public class Note
{
    public Guid NoteId { get; private set; } = Guid.NewGuid(); // unique ID for the note
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedDate { get; private set; } = DateTime.Now;
    public DateTime LastUpdatedDate { get; private set; } = DateTime.Now;
    public List<string> AttachedFiles { get; private set; } = new();
    public List<string> Tags { get; private set; } = new();
    public bool IsPinned { get; set; } = false;

    public Color Color { get; set; }

    public Note() { }

    public Note(string title, string message)
    {
        Title = title;
        Body = message;
    }

    public void Update(string? newTitle = null, string? newMessage = null)
    {
        if (newTitle != null)
            Title = newTitle;
        if (newMessage != null)
            Body = newMessage;

        LastUpdatedDate = DateTime.Now;
    }

    public void AttachFile(string fileName)
    {
        if (!AttachedFiles.Contains(fileName))
            AttachedFiles.Add(fileName);
        LastUpdatedDate = DateTime.Now;
    }

    public void RemoveFile(string fileName)
    {
        AttachedFiles.Remove(fileName);
        LastUpdatedDate = DateTime.Now;
    }

    public void AddTag(string tag)
    {
        if (!Tags.Contains(tag))
            Tags.Add(tag);
        LastUpdatedDate = DateTime.Now;
    }

    public void RemoveTag(string tag)
    {
        Tags.Remove(tag);
        LastUpdatedDate = DateTime.Now;
    }
}
