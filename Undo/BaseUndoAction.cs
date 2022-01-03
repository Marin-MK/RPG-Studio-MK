using System.Collections.Generic;

namespace RPGStudioMK.Undo;

public class BaseUndoAction
{
    public BaseUndoAction()
    {
        Editor.UndoList.Add(this);
        Editor.RedoList.Clear();
        Editor.MainWindow.ToolBar.Undo.SetEnabled(true);
        Editor.MainWindow.ToolBar.Redo.SetEnabled(false);
    }

    public virtual bool Trigger(bool IsRedo)
    {
        return true;
    }

    public void RevertTo(bool IsRedo)
    {
        List<BaseUndoAction> ListA = IsRedo ? Editor.RedoList : Editor.UndoList;
        List<BaseUndoAction> ListB = IsRedo ? Editor.UndoList : Editor.RedoList;
        int Index = ListA.IndexOf(this);
        for (int i = ListA.Count - 1; i >= Index; i--)
        {
            BaseUndoAction action = ListA[i];
            bool success = action.Trigger(IsRedo);
            if (success)
            {
                ListB.Add(action);
                ListA.RemoveAt(i);
            }
        }
    }

    public bool InMode(EditorMode Mode)
    {
        return Editor.Mode == Mode;
    }

    public bool SetMode(EditorMode Mode)
    {
        Editor.SetMode(Mode);
        return false;
    }

    public bool SetDatabaseMode(Widgets.DatabaseMode Submode)
    {
        Editor.SetDatabaseMode(Submode);
        return false;
    }

    public bool InDatabaseSubmode(Widgets.DatabaseMode Submode)
    {
        if (Editor.Mode == EditorMode.Database)
        {
            return ((Widgets.DatabaseWidget) Editor.MainWindow.MainEditorWidget).Mode == Submode;
        }
        return false;
    }

    public bool SetDatabaseSubmode(Widgets.DatabaseMode Submode)
    {
        if (Editor.Mode == EditorMode.Database)
        {
            ((Widgets.DatabaseWidget) Editor.MainWindow.MainEditorWidget).SetMode(Submode);
        }
        return false;
    }
}
