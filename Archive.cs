using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace RPGStudioMK;

public class Archive : IDisposable
{
    Stream ZipStream;
    ZipArchive ZipArchive;
    string ZipArchiveFilename;
    
    public bool Closed { get; protected set; } = false;
    public List<ArchiveEntry> Files = new List<ArchiveEntry>();

    /// <summary>
    /// Opens a Zip archive.
    /// </summary>
    /// <param name="ZipName">Filename to the archive to open.</param>
    public Archive(string ZipName)
    {
        ZipStream = File.OpenRead(ZipName);
        ZipArchive = new ZipArchive(ZipStream);
        ZipArchiveFilename = ZipName;
        foreach (ZipArchiveEntry entry in ZipArchive.Entries)
        {
            this.Files.Add(new ArchiveEntry(entry));
        }
    }

    /// <summary>
    /// Extracts all files and directories within this archive to the specified directory.
    /// </summary>
    /// <param name="Directory">The directory in which to extract this zip archive.</param>
    public void Extract(string Directory)
    {
        ZipArchive.ExtractToDirectory(Directory);
    }

    /// <summary>
    /// Extracts all files and directories that satisfy a condition to the specified directory.
    /// </summary>
    /// <param name="Directory">The directory in which to extract the files and directories.</param>
    /// <param name="Condition">The condition on which to select files and directories to extract.</param>
    public void ExtractWhere(string Directory, Predicate<ArchiveEntry> Condition)
    {
        Files.FindAll(f => Condition(f)).ForEach(f => f.Extract(Directory));
    }

    /// <summary>
    /// Finds a file in this archive.
    /// </summary>
    /// <param name="Condition">The condition on which to filter.</param>
    /// <returns>The corresponding ArchiveEntry, or null if no matches were found.</returns>
    public ArchiveEntry FindFile(Predicate<ArchiveEntry> Condition)
    {
        return Files.Find(f => Condition(f) && !f.IsDirectory);
    }

    /// <summary>
    /// Finds all files in this archive matching a condition.
    /// </summary>
    /// <param name="Condition">The condition on which to filter.</param>
    /// <returns>A list of matching ArchiveEntry obejcts</returns>
    public List<ArchiveEntry> FindAllFiles(Predicate<ArchiveEntry> Condition)
    {
        return Files.FindAll(f => Condition(f) && !f.IsDirectory);
    }

    /// <summary>
    /// Finds all files in this archive.
    /// </summary>
    /// <returns>A list of all files in this archive.</returns>
    public List<ArchiveEntry> FindAllFiles()
    {
        return Files.FindAll(f => !f.IsDirectory);
    }

    /// <summary>
    /// Finds a directory in this archive.
    /// </summary>
    /// <param name="Condition">The condition on which to filter.</param>
    /// <returns>The corresponding ArchiveEntry, or null if no matches were found.</returns>
    public ArchiveEntry FindDirectory(Predicate<ArchiveEntry> Condition)
    {
        return Files.Find(f => Condition(f) && f.IsDirectory);
    }

    /// <summary>
    /// Finds all directories in this archive matching a condition.
    /// </summary>
    /// <param name="Condition">The condition on which to filter.</param>
    /// <returns>A list of matching ArchiveEntry obejcts</returns>
    public List<ArchiveEntry> FindAllDirectories(Predicate<ArchiveEntry> Condition)
    {
        return Files.FindAll(f => Condition(f) && f.IsDirectory);
    }

    /// <summary>
    /// Finds all directories in this archive.
    /// </summary>
    /// <returns>A list of all directories in this archive.</returns>
    public List<ArchiveEntry> FindAllDirectories()
    {
        return Files.FindAll(f => f.IsDirectory);
    }

    /// <summary>
    /// Returns whether a file matching the specified filename is found in this archive.
    /// </summary>
    /// <param name="Filename">The filename to look for.</param>
    /// <returns>Whether a match was found.</returns>
    public bool ContainsFile(string Filename)
    {
        return Files.Exists(f => f.Filename == Filename && !f.IsDirectory);
    }

    /// <summary>
    /// Returns whether a directory matching the specified directory name is found in this archive.
    /// </summary>
    /// <param name="DirName">The directory name to look for.</param>
    /// <returns>Whether a match was found.</returns>
    public bool ContainsDirectory(string DirName)
    {
        return Files.Exists(f => f.Filename == DirName&& f.IsDirectory);
    }

    /// <summary>
    /// Deletes this archive file and closes the stream.
    /// </summary>
    public void Delete()
    {
        this.Dispose();
        File.Delete(this.ZipArchiveFilename);
    }

    /// <summary>
    /// Closes the stream.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown when the archive has already been disposed.</exception>
    public void Dispose()
    {
        if (Closed) throw new ObjectDisposedException("Archive");
        ZipStream.Close();
        ZipArchive.Dispose();
        Closed = true;
    }
}

public class ArchiveEntry
{
    ZipArchiveEntry ZipArchiveEntry;
    
    /// <summary>
    /// The filename of the file.
    /// </summary>
    public string Filename;
    /// <summary>
    /// The extension of the file
    /// </summary>
    public string Extension;
    /// <summary>
    /// Whether this entry represents a directory.
    /// </summary>
    public bool IsDirectory { get; }

    public ArchiveEntry(ZipArchiveEntry ZipArchiveEntry)
    {
        this.ZipArchiveEntry = ZipArchiveEntry;
        this.Filename = ZipArchiveEntry.FullName;
        this.IsDirectory = this.Filename.EndsWith('\\') || this.Filename.EndsWith('/');
        if (this.IsDirectory) this.Filename = this.Filename.Substring(0, this.Filename.Length - 1);
        int idx = this.Filename.LastIndexOf('.');
        this.Extension = this.Filename.Substring(idx + 1, this.Filename.Length - idx - 1);
    }

    /// <summary>
    /// Renames this file or directory.
    /// </summary>
    /// <param name="Filename">The new filename or directory name.</param>
    public void Rename(string Filename)
    {
        this.Filename = Filename;
    }

    /// <summary>
    /// Extracts this file or directory within the given directory.
    /// </summary>
    /// <param name="Directory">The directory in which to extract the file or directory.</param>
    public void Extract(string Directory, bool Overwrite = false)
    {
        if (!System.IO.Directory.Exists(Directory)) System.IO.Directory.CreateDirectory(Directory);
        if (this.IsDirectory) System.IO.Directory.CreateDirectory(Path.Combine(Directory, this.Filename));
        else
        {
            string path = Path.Combine(Directory, this.Filename);
            if (File.Exists(path) && Overwrite || !File.Exists(path)) ZipArchiveEntry.ExtractToFile(path, Overwrite);
        }
    }

    public override string ToString()
    {
        return this.Filename;
    }
}