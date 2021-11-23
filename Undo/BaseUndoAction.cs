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

        public virtual bool Trigger(bool IsRedo)
        {
            return true;
        }

        public void RevertTo(bool IsRedo)
        {
            List<BaseUndoAction> ListA = IsRedo ? Editor.MapRedoList : Editor.MapUndoList;
            List<BaseUndoAction> ListB = IsRedo ? Editor.MapUndoList : Editor.MapRedoList;
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
    }
}
