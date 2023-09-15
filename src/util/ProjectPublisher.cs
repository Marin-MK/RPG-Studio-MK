using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using RPGStudioMK.Game;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MKUtils;
using System.Security.Cryptography;

namespace RPGStudioMK;

// TODO: Compile all scripts into a new temporary Scripts.rxdata, zip it, and delete the temp file.

public class ProjectPublisher : IDisposable
{
	public static List<(string ID, string Text, bool Checked)> PublishOptions = new List<(string ID, string Text, bool Checked)>()
	{
		("species-assets", "Exclude unused species assets", true),
		("item-assets", "Exclude unused item assets", true),
		("trainer-assets", "Exclude unused trainer assets", true),
		("autotile-assets", "Exclude unused autotile assets", true),
		("tileset-assets", "Exclude unused tileset assets", true),

		//("encrypt", "Encrypt the project")
	};

	public bool Cancelled { get; protected set; }

	string projectName;
	string projectVersion;
	FileStream archiveStream;
	ZipArchive archive;
	string zipFilename;
	List<string> selectedOptions;
	string tempScriptsFile;

    public ProjectPublisher(string projectName, string projectVersion, string zipFilename, List<string> selectedOptions)
    {
		this.projectName = projectName;
		this.projectVersion = projectVersion;
		this.zipFilename = zipFilename;
		this.selectedOptions = selectedOptions;
		Logger.WriteLine("Publishing the project to '{0}'", zipFilename);
		archiveStream = new FileStream(zipFilename, FileMode.Create);
		archive = new ZipArchive(archiveStream, ZipArchiveMode.Create, true);
	}

	private List<string> GetRootFiles()
	{
		return new List<string>()
		{
			Data.ProjectPath + "/Game.exe",
			Data.ProjectPath + "/Game.ini",
			Data.ProjectPath + "/mkxp.json",
			Data.ProjectPath + "/soundfont.sf2",
			Data.ProjectPath + "/x64-msvcrt-ruby310.dll",
			Data.ProjectPath + "/zlib1.dll"
		};
	}

	private List<string> GetFolderFiles(string folder, bool includeSubdirectories)
	{
		List<string> files = Directory.GetFiles(folder).Select(x => x.Replace('\\', '/')).ToList();
		if (!includeSubdirectories) return files;
		foreach (string subdirectory in Directory.EnumerateDirectories(folder))
		{
			files.AddRange(GetFolderFiles(subdirectory, includeSubdirectories));
		}
		return files;
	}

	private void RemoveUnusedSpeciesFiles(List<string> masterFileList, string folder)
	{
		List<string> files = GetFolderFiles(folder, false);
		foreach (string filename in files)
		{
			if (this.Cancelled) return;
			// Do not delete missingno graphics
			if (filename.Contains("000")) continue;
			if (!Data.Species.Any(s => filename.Contains(s.Value.ID)))
			{
				masterFileList.Remove(filename);
			}
		}
	}

	private void RemoveUnusedItemFiles(List<string> masterFileList, string folder)
	{
		List<string> files = GetFolderFiles(folder, false);
		foreach (string filename in files)
		{
			if (this.Cancelled) return;
			// Do not delete missingno graphics
			if (filename.Contains("000")) continue;
			if (!Data.Items.Any(s => filename.Contains(s.Value.ID)))
			{
				masterFileList.Remove(filename);
			}
		}
	}

	private void RemoveUnusedTrainerFiles(List<string> masterFileList, string folder)
	{
		List<string> files = GetFolderFiles(folder, false);
		foreach (string filename in files)
		{
			if (this.Cancelled) return;
			// Do not delete missingno graphics
			if (filename.Contains("000")) continue;
			if (!Data.TrainerTypes.Any(s => filename.Contains(s.Value.ID)))
			{
				masterFileList.Remove(filename.Replace('\\', '/'));
			}
		}
	}

	private void RemoveUnusedTilesets(List<string> masterFileList)
	{
		// Start with all tilesets
		List<int> unusedTilesets = Enumerable.Range(1, Data.Tilesets.Count - 1).ToList();
		foreach (Map map in Data.Maps.Values)
		{
			if (unusedTilesets.Contains(map.TilesetIDs[0]))
			{
				// Remove tilesets in use by a map
				unusedTilesets.Remove(map.TilesetIDs[0]);
			}
		}

		// Filter out unused tilesets that share the same graphic as in-use tilesets.
		unusedTilesets = unusedTilesets.Where(t =>
		{
			// Only tilesets that do not share the same graphic as tilesets that are in use, are truly unused and can be have their associated graphics removed.
			List<Tileset> usedTilesetsWithSameGraphic = Data.Tilesets.Where(d => d is not null && d.GraphicName == Data.Tilesets[t].GraphicName && Data.Tilesets.IndexOf(d) != t && !unusedTilesets.Contains(Data.Tilesets.IndexOf(d))).ToList();
			return usedTilesetsWithSameGraphic.Count == 0;
		}).ToList();

		// Map tileset IDs to names
		List<string> unusedTilesetNames = unusedTilesets.Select(t => Data.Tilesets[t].GraphicName).Where(t => !string.IsNullOrEmpty(t)).ToList();
		
		// Remove unused files.
		foreach (string filename in GetFolderFiles(Data.ProjectPath + "/Graphics/Tilesets", false))
		{
			if (this.Cancelled) return;
			bool tilesetGraphicIsUnused = unusedTilesetNames.Any(t => Path.GetFileNameWithoutExtension(filename) == t);
			bool filenameIsUnused = !Data.Tilesets.Any(t => t is not null && filename.Contains(t.GraphicName));
			if (tilesetGraphicIsUnused || filenameIsUnused)
			{
				masterFileList.Remove(filename.Replace('\\', '/'));
			}
		}
	}

	private void RemoveUnusedAutotiles(List<string> masterFileList)
	{
		// Start with all tilesets
		List<int> unusedTilesets = Enumerable.Range(1, Data.Tilesets.Count - 1).ToList();
		foreach (Map map in Data.Maps.Values)
		{
			if (unusedTilesets.Contains(map.TilesetIDs[0]))
			{
				// Remove tilesets in use by a map
				unusedTilesets.Remove(map.TilesetIDs[0]);
			}
		}

		// Remove unused files.
		foreach (string filename in GetFolderFiles(Data.ProjectPath + "/Graphics/Autotiles", false))
		{
			if (this.Cancelled) return;
			// Checks whether there are any in-use tilesets that use this autotile
			bool filenameIsUnused = !Data.Tilesets.Any(t => t is not null && !unusedTilesets.Contains(Data.Tilesets.IndexOf(t)) && t.Autotiles.Any(a => a is not null && filename.Contains(a.GraphicName)));
			if (filenameIsUnused)
			{
				masterFileList.Remove(filename.Replace('\\', '/'));
			}
		}
	}

	public void Run(DynamicCallbackManager<SimpleProgress>? dcm = null)
	{
		Logger.WriteLine("Calculating files to include...");
		List<string> files = GetRootFiles();
		List<string> graphicsFiles;
		List<string> audioFiles;

		files.AddRange(GetFolderFiles(Data.ProjectPath + "/Fonts", true));
		files.AddRange(GetFolderFiles(Data.ProjectPath + "/Data", false));
		files.AddRange(graphicsFiles = GetFolderFiles(Data.ProjectPath + "/Graphics", true));
		files.AddRange(audioFiles = GetFolderFiles(Data.ProjectPath + "/Audio", true));

		// Remove scripts from the archive.
		dcm.SetStatus("Recompiling scripts...");
		files.RemoveAll(x => x == Data.ProjectPath + "/Data/Scripts.rxdata");
		tempScriptsFile = Path.GetTempFileName();
		Logger.WriteLine("Dumping scripts to '{0}' for inclusion...", tempScriptsFile);
		Data.SaveScriptsRXDATA(tempScriptsFile);
		if (Data.StopLoading)
		{
			// Something went wrong whilst saving the scripts
			Logger.WriteLine("Something went wrong whilst saving the scripts; the publisher cannot continue.");
			return;
		}
		if (this.Cancelled) return;

		if (selectedOptions.Contains("species-assets"))
		{
			dcm.SetStatus("Trimming species assets...");
			Logger.WriteLine("Removing species assets for non-existing species.");
			RemoveUnusedSpeciesFiles(files, Data.ProjectPath + "/Graphics/Pokemon/Back");
			RemoveUnusedSpeciesFiles(files, Data.ProjectPath + "/Graphics/Pokemon/Back shiny");
			RemoveUnusedSpeciesFiles(files, Data.ProjectPath + "/Graphics/Pokemon/Footprints");
			RemoveUnusedSpeciesFiles(files, Data.ProjectPath + "/Graphics/Pokemon/Front");
			RemoveUnusedSpeciesFiles(files, Data.ProjectPath + "/Graphics/Pokemon/Front shiny");
			RemoveUnusedSpeciesFiles(files, Data.ProjectPath + "/Graphics/Pokemon/Icons");
			RemoveUnusedSpeciesFiles(files, Data.ProjectPath + "/Graphics/Pokemon/Icons shiny");
			RemoveUnusedSpeciesFiles(files, Data.ProjectPath + "/Audio/SE/Cries");
			if (this.Cancelled) return;
		}

		if (selectedOptions.Contains("item-assets"))
		{
			dcm.SetStatus("Trimming item assets...");
			Logger.WriteLine("Removing item assets for non-existing items.");
			RemoveUnusedItemFiles(files, Data.ProjectPath + "/Graphics/Items");
			if (this.Cancelled) return;
		}

		if (selectedOptions.Contains("trainer-assets"))
		{
			dcm.SetStatus("Trimming trainer assets...");
			Logger.WriteLine("Removing trainer assets for non-existing trainer types.");
			RemoveUnusedTrainerFiles(files, Data.ProjectPath + "/Graphics/Trainers");
			if (this.Cancelled) return;
		}

		if (selectedOptions.Contains("autotile-assets"))
		{
			dcm.SetStatus("Trimming autotile assets...");
			Logger.WriteLine("Smart-trimming autotile assets.");
			RemoveUnusedAutotiles(files);
			if (this.Cancelled) return;
		}

		if (selectedOptions.Contains("tileset-assets"))
		{
			dcm.SetStatus("Trimming tileset assets...");
			Logger.WriteLine("Smart-trimming tileset assets.");
			RemoveUnusedTilesets(files);
			if (this.Cancelled) return;
		}

		// Remove unused battle backs
		files.RemoveAll(x => x.StartsWith(Data.ProjectPath + "/Graphics/Battlebacks/unused"));

		// Remove all thumbnail/cache files
		files.RemoveAll(x => x.EndsWith("Thumbs.db") || x.EndsWith("desktop.ini"));

		// Remove all files and folders that start with . in the root folder (e.g. .gitignore, .git, .vs)
		files.RemoveAll(x => x.StartsWith(Data.ProjectPath + "/."));

		// Compress files
		dcm.SetStatus($"Compressing {Utilities.PrettyInt(files.Count)} files...");
		SimpleProgress progress = new SimpleProgress();
		for (int i = 0; i < files.Count; i++)
		{
			if (this.Cancelled) return;
			string filename = files[i];
			string relativeFilename = filename.Substring(Data.ProjectPath.Length + 1);
			archive.CreateEntryFromFile(filename, relativeFilename);
			dcm?.Update(progress.SetFactor(i / (float) files.Count));
		}

		// Add new custom tempScriptsFile to the archive.
		archive.CreateEntryFromFile(tempScriptsFile, "Data/Scripts.rxdata");
		dcm?.Update(progress.SetFactor(1d));
	}

	public string CalculateSHA()
	{
		archiveStream.Position = 0;
		return Convert.ToBase64String(SHA256.HashData(archiveStream));
	}

	public void Cancel()
	{
		this.Cancelled = true;
	}

	public void Dispose()
	{
		Logger.WriteLine("Releasing archive access...");
		archive.Dispose();
		archiveStream.Dispose();
		if (File.Exists(tempScriptsFile)) File.Delete(tempScriptsFile);
		if (this.Cancelled)
		{
			if (File.Exists(zipFilename)) File.Delete(zipFilename);
			Logger.WriteLine("Project publisher was cancelled.");
		}
		else Logger.WriteLine("Project published to '{0}' successfully!", zipFilename);
	}
}
