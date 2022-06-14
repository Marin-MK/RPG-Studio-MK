using RPGStudioMK.Widgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK;

public partial class MainEditorWindow
{
    List<MenuItem> MenuBarItems => new List<MenuItem>()
    {
        new MenuItem("File")
        {
            Items = new List<IMenuItem>()
            {
                new MenuItem("New")
                {
                    HelpText = "Create a new project.",
                    OnClicked = _ => EnsureSaved(Editor.NewProject)
                },
                new MenuItem("Open")
                {
                    HelpText = "Open an existing project.",
                    Shortcut = "Ctrl+O",
                    OnClicked = _ => EnsureSaved(Editor.OpenProject)
                },
                new MenuItem("Save")
                {
                    HelpText = "Save all changes in the current project.",
                    Shortcut = "Ctrl+S",
                    IsClickable = e => e.Value = Editor.InProject,
                    OnClicked = _ => Editor.SaveProject()
                },
                new MenuSeparator(),
                new MenuItem("Close Project")
                {
                    HelpText = "Close this project and return to the welcome screen.",
                    IsClickable = e => e.Value = Editor.InProject,
                    OnClicked = _ => EnsureSaved(() => Editor.CloseProject(true))
                },
                new MenuItem("Reload Project")
                {
                    HelpText = "Closes and immediately reopens the project. Used for quickly determining if changes are saved properly, or to restore an old version.",
                    IsClickable = e => e.Value = Editor.InProject,
                    OnClicked = _ => EnsureSaved(Editor.ReloadProject)
                },
                new MenuItem("Exit Editor")
                {
                    HelpText = "Close this project and quit the program.",
                    OnClicked = _ => EnsureSaved(Editor.ExitEditor)
                }
            }
        },
        new MenuItem("View")
        {
            Items = new List<IMenuItem>()
            {
                new MenuItem("Show Animations")
                {
                    IsCheckable = true,
                    IsChecked = e => e.Value = Editor.GeneralSettings.ShowMapAnimations,
                    HelpText = "Toggles the animation of autotiles, fogs and panoramas.",
                    IsClickable = e => e.Value = Editor.InProject,
                    OnClicked = _ => Editor.ToggleMapAnimations()
                },
                new MenuItem("Show Grid")
                {
                    IsCheckable = true,
                    IsChecked = e => e.Value = Editor.GeneralSettings.ShowGrid,
                    HelpText = "Toggles the visibility of the grid overlay while mapping.",
                    IsClickable = e => e.Value = Editor.InProject,
                    OnClicked = _ => Editor.ToggleGrid()
                },
                new MenuItem("Expand Event Commands")
                {
                    IsCheckable = true,
                    IsChecked = e => e.Value = Editor.GeneralSettings.ExpandEventCommands,
                    HelpText = "Whether to expand event commands by default.",
                    IsClickable = e => e.Value = Editor.InProject,
                    OnClicked = _ => Editor.GeneralSettings.ExpandEventCommands = !Editor.GeneralSettings.ExpandEventCommands
                },
                new MenuItem("Event Graphics")
                {
                    IsClickable = e => e.Value = Editor.InProject,
                    Items = new List<IMenuItem>()
                    {
                        new MenuItem("Box only")
                        {
                            IsCheckable = true,
                            IsChecked = e => e.Value = Editor.ProjectSettings.EventGraphicViewMode == EventGraphicViewMode.BoxOnly,
                            HelpText = "Shows only the boxes of events, no graphics.",
                            OnClicked = _ => Editor.SetEventGraphicViewMode(EventGraphicViewMode.BoxOnly),
                            IsClickable = e => e.Value = Editor.ProjectSettings.EventGraphicViewMode != EventGraphicViewMode.BoxOnly
                        },
                        new MenuItem("Box and Graphic")
                        {
                            IsCheckable = true,
                            IsChecked = e => e.Value = Editor.ProjectSettings.EventGraphicViewMode == EventGraphicViewMode.BoxAndGraphic,
                            HelpText = "Shows boxes of events and the full graphic.",
                            OnClicked = _ => Editor.SetEventGraphicViewMode(EventGraphicViewMode.BoxAndGraphic),
                            IsClickable = e => e.Value = Editor.ProjectSettings.EventGraphicViewMode != EventGraphicViewMode.BoxAndGraphic
                        },
                        new MenuItem("Box and cropped Graphic")
                        {
                            IsCheckable = true,
                            IsChecked = e => e.Value = Editor.ProjectSettings.EventGraphicViewMode == EventGraphicViewMode.BoxAndCroppedGraphic,
                            HelpText = "Shows boxes of events and the graphic cropped to fit the box.",
                            OnClicked = _ => Editor.SetEventGraphicViewMode(EventGraphicViewMode.BoxAndCroppedGraphic),
                            IsClickable = e => e.Value = Editor.ProjectSettings.EventGraphicViewMode != EventGraphicViewMode.BoxAndCroppedGraphic
                        },
                        new MenuItem("Graphic only")
                        {
                            IsCheckable = true,
                            IsChecked = e => e.Value = Editor.ProjectSettings.EventGraphicViewMode == EventGraphicViewMode.GraphicOnly,
                            HelpText = "Shows no boxes of events, only the full graphics.",
                            OnClicked = _ => Editor.SetEventGraphicViewMode(EventGraphicViewMode.GraphicOnly),
                            IsClickable = e => e.Value = Editor.ProjectSettings.EventGraphicViewMode != EventGraphicViewMode.GraphicOnly
                        },
                        new MenuItem("Cropped Graphic only")
                        {
                            IsCheckable = true,
                            IsChecked = e => e.Value = Editor.ProjectSettings.EventGraphicViewMode == EventGraphicViewMode.CroppedGraphicOnly,
                            HelpText = "Shows no boxes of events, only the graphic cropped to fit where the box would be.",
                            OnClicked = _ => Editor.SetEventGraphicViewMode(EventGraphicViewMode.CroppedGraphicOnly),
                            IsClickable = e => e.Value = Editor.ProjectSettings.EventGraphicViewMode != EventGraphicViewMode.CroppedGraphicOnly
                        },
                        new MenuSeparator(),
                        new MenuItem("Show in Tiles submode")
                        {
                            IsCheckable = true,
                            IsChecked = e => e.Value = Editor.ProjectSettings.ShowEventBoxesInTilesSubmode,
                            HelpText = "When enabled, will also show event boxes and graphics in the Tiles submode.",
                            OnClicked = _ => Editor.SetEventBoxVisibilityInTiles(!Editor.ProjectSettings.ShowEventBoxesInTilesSubmode)
                        }
                    }
                },
                new MenuItem("History")
                {
                    IsClickable = e => e.Value = Editor.InProject,
                    Items = new List<IMenuItem>()
                    {
                        new MenuItem("Undo History")
                        {
                            HelpText = "Shows all your past undoable actions.",
                            IsClickable = e => e.Value = Editor.UndoList.Count > 0,
                            OnClicked = _ => Editor.ShowUndoHistory()
                        },
                        new MenuItem("Redo History")
                        {
                            HelpText = "Shows all your redoable actions.",
                            IsClickable = e => e.Value = Editor.RedoList.Count > 0,
                            OnClicked = _ => Editor.ShowRedoHistory()
                        }
                    }
                }
            }
        },
        new MenuItem("Game")
        {
            Items = new List<IMenuItem>()
            {
                new MenuItem("Play Game")
                {
                    Shortcut = "F12",
                    HelpText = "Play the game.",
                    IsClickable = e => e.Value = Editor.InProject,
                    OnClicked = _ => Editor.StartGame()
                },
                new MenuItem("Open Game Folder")
                {
                    HelpText = "Opens the file explorer and navigates to the project folder.",
                    IsClickable = e => e.Value = Editor.InProject,
                    OnClicked = _ => Editor.OpenGameFolder()
                },
                new MenuItem("Change Title")
                {
                    HelpText = "Change the title of your game.",
                    IsClickable = e => e.Value = Editor.InProject,
                    OnClicked = _ => Editor.RenameGame()
                }
            }
        },
        new MenuItem("Help")
        {
            Items = new List<IMenuItem>()
            {
                new MenuItem("Help")
                {
                    Shortcut = "F1",
                    HelpText = "Opens the help window.",
                    OnClicked = _ => Editor.OpenHelpWindow()
                },
                new MenuItem("About RPG Studio MK")
                {
                    HelpText = "Shows information about this program.",
                    OnClicked = _ => Editor.OpenAboutWindow()
                },
                new MenuItem("Legal")
                {
                    HelpText = "Shows legal information about this program.",
                    OnClicked = _ => Editor.OpenLegalWindow()
                }
            }
        }
    };
}
