using MKUtils;
using RPGStudioMK.Game;
using RPGStudioMK.Widgets;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RPGStudioMK;

public static class KitManager
{
	public static string KitFolder => Path.Combine(Editor.AppDataFolder, "Kits").Replace('\\', '/');

	public static List<Kit> GetAvailableKits()
	{
		List<Kit> kits = VersionMetadata.Kits.Select(l => new Kit(l[0], l[1], l[2])).ToList();
		List<string> kitFiles = new List<string>();
		if (Directory.Exists(KitFolder))
		{
			foreach (string kitFile in Directory.GetFiles(KitFolder))
			{
				string relativeKitFilename = Path.GetFileName(kitFile);
				if (!kits.Any(kit => kit.Filename == relativeKitFilename))
				{
					// Kit was added manually
					string kitName = Path.GetFileNameWithoutExtension(relativeKitFilename);
					Kit kit = new Kit(kitName, relativeKitFilename);
					kits.Insert(0, kit);
				}
			}
		}
		kits.Reverse();
		return kits;
    }

	public static void Download(Kit kit, Action continueCallback)
	{
		if (!Directory.Exists(KitFolder)) Directory.CreateDirectory(KitFolder);
		string kitFilename = Path.Combine(KitFolder, kit.Filename).Replace('\\', '/');
		FileDownloaderWindow window = new FileDownloaderWindow(kit.URL, kitFilename, "Downloading kit...");
		window.Download();
		FileInfo fi = new FileInfo(kitFilename);
		if (fi.Length <= 0)
		{
			new MessageBox("Error", "Something went wrong while downloading the kit. The downloaded file is empty.", ButtonType.OK, IconType.Error);
			Logger.Error("The downloaded kit has a file size of 0. This is likely an indication that something went wrong during the downloading phase.");
			return;
		}
		if (!kit.IsInstalled())
		{
			new MessageBox("Error", "The kit failed to install properly.", ButtonType.OK, IconType.Error);
			Logger.Error("The kit was not installed properly. This might indicate a mismatch in filenames, or that accented characters messed up checking whether the file exists.");
			return;
		}
		if (!kit.IsValid())
		{
			MessageBox win = new MessageBox("Error", "The hash value of the downloaded kit did not equal the expected hash value. This might mean the file has been tampered with, or that it did not download properly. Please redownload the kit, or continue anyway.",
				new List<string>() { "Continue", "Redownload", "Cancel" }, IconType.Error);
			win.OnClosed += _ =>
			{
				if (win.Result == 0) continueCallback();
				if (win.Result == 1)
				{
					File.Delete(kitFilename);
					Download(kit, continueCallback);
				}
				if (win.Result == 2) return; // Cancel
			};
			return;
		}
		continueCallback();
	}

	public static void Copy(Kit kit, string destinationFolder, CancellationTokenSource source, DynamicCallbackManager<SimpleProgress>? dcm = null)
	{
		string kitFilename = Path.Combine(KitFolder, kit.Filename).Replace('\\', '/');
		MKUtils.Archive archive = new MKUtils.Archive(kitFilename);
		string MainFolder = null;
		if (!Directory.Exists(destinationFolder)) Directory.CreateDirectory(destinationFolder);
		foreach (MKUtils.ArchiveEntry entry in archive.Files)
		{
			if (entry.Filename.Contains('\\') || entry.Filename.Contains('/')) continue;
			if (MainFolder != null)
			{
				MainFolder = null;
				break;
			}
			MainFolder = entry.Filename;
		}
		// if MainFolder is null, then there either are no entries,
		// or there is more than one file/folder in the main archive,
		// meaning we extract it as-is.
		SimpleProgress progress = new SimpleProgress();
		if (MainFolder == null)
		{
			float total = archive.Files.Count;
			float count = 0;
			foreach (MKUtils.ArchiveEntry entry in archive.Files)
			{
				entry.Extract(destinationFolder);
				source.Token.ThrowIfCancellationRequested();
				count++;
				progress.SetFactor(count / total);
				dcm?.Update(progress);
			}
			if (count != total)
			{
				progress.SetFactor(1);
				dcm?.Update(progress);
			}
		}
		else
		{
			// If MainFolder is not null, that means the archive has one single folder at its root.
			// We want to ignore this folder as we've already created our own folder in which we want
			// all the files to reside, thus we purge this part of the path for all the other entries.
			float total = archive.Files.Count - 1;
			float count = 0;
			foreach (MKUtils.ArchiveEntry entry in archive.Files)
			{
				if (entry.Filename == MainFolder) continue;
				if (entry.Filename.Contains(MainFolder)) entry.Rename(entry.Filename.Substring(MainFolder.Length + 1));
				entry.Extract(destinationFolder);
				source.Token.ThrowIfCancellationRequested();
				count++;
				progress.SetFactor(count / total);
				dcm.Update(progress);
			}
			if (count != total)
			{
				progress.SetFactor(1);
				dcm.Update(progress);
			}
		}
		archive.Dispose();
	}
}

public class Kit
{
	public string DisplayName;
	public string Filename;
	public string? URL;
	public string? ExpectedSHA;

    public Kit(string displayName, string url, string expectedSHA)
    {
		this.DisplayName = displayName;
		this.Filename = this.DisplayName + ".zip";
		this.URL = url;
		this.ExpectedSHA = expectedSHA;
    }

    public Kit(string displayName, string filename)
    {
		this.DisplayName = displayName;
		this.Filename = filename;
		this.URL = null;
		this.ExpectedSHA = null;
    }

    public bool IsInstalled()
	{
		string kitPath = Path.Combine(KitManager.KitFolder, this.Filename).Replace('\\', '/');
		return File.Exists(kitPath);
	}

	public bool IsValid()
	{
		if (this.ExpectedSHA is null)
		{
			Logger.WriteLine("Kit '{0}' is counted as valid as it is an offline kit without SHA value.", this.DisplayName);
			return true;
		}
		string? calculatedSHA = this.CalculateSHA();
		if (calculatedSHA is null) return false;
		return calculatedSHA.Equals(this.ExpectedSHA);
	}

    public string? CalculateSHA()
	{
		if (!IsInstalled()) return null;
		string kitPath = Path.Combine(KitManager.KitFolder, this.Filename).Replace('\\', '/');
		using (FileStream fs = File.OpenRead(kitPath))
		{
			byte[] sha = SHA256.HashData(fs);
			return Convert.ToBase64String(sha);
		}
	}
}