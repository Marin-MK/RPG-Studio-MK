using System;
using System.Collections.Generic;
using RPGStudioMK.Game;

namespace RPGStudioMK
{
    public class BaseUndoAction
    {
        public BaseUndoAction()
        {
            Editor.MapUndoList.Add(this);
            Editor.MapRedoList.Clear();
        }

        public virtual void Trigger(bool IsRedo) { }

        public void RevertTo(bool IsRedo)
        {
            if (!IsRedo)
            {
                int Index = Editor.MapUndoList.IndexOf(this);
                for (int i = Editor.MapUndoList.Count - 1; i >= Index; i--)
                {
                    BaseUndoAction action = Editor.MapUndoList[i];
                    action.Trigger(IsRedo);
                    Editor.MapRedoList.Add(action);
                    Editor.MapUndoList.RemoveAt(i);
                }
            }
            else
            {
                int Index = Editor.MapRedoList.IndexOf(this);
                for (int i = Editor.MapRedoList.Count - 1; i >= Index; i--)
                {
                    BaseUndoAction action = Editor.MapRedoList[i];
                    action.Trigger(IsRedo);
                    Editor.MapUndoList.Add(action);
                    Editor.MapRedoList.RemoveAt(i);
                }
            }
        }
    }
}
