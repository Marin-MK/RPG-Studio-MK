using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace MKEditor
{
    public static class CommandPlugins
    {
        public static List<DynamicCommandType> CommandTypes = new List<DynamicCommandType>();

        public static void Initialize()
        {
            AssemblyLoadContext context = new AssemblyLoadContext("Command API");
            Assembly asm = context.LoadFromAssemblyPath(Path.GetFullPath("ext/netcoreapp3.1/MKAPI"));
            foreach (Type type in asm.GetTypes())
            {
                List<Type> types = Utilities.GetParentTypes(type);
                if (types.Find(t => t.FullName == "MKAPI.Command") != null)
                {
                    dynamic cmd = Activator.CreateInstance(type);
                    CommandTypes.Add(new DynamicCommandType(cmd));
                }
            }
        }
    }

    public class DynamicCommandType
    {
        protected dynamic DynamicType;
        public string Name { get => DynamicType.Name; }
        public string Identifier { get => DynamicType.Identifier; }
        public bool ShowHeader { get => DynamicType.ShowHeader; }
        public ODL.Color HeaderColor { get => DynamicToColor(DynamicType.HeaderColor); }
        public List<ODL.Color> TextColors
        {
            get
            {
                List<ODL.Color> Colors = new List<ODL.Color>();
                for (int i = 0; i < DynamicType.TextColors.Count; i++) Colors.Add(DynamicToColor(DynamicType.TextColors[i]));
                return Colors;
            }
        }
        public int WindowWidth { get => DynamicType.WindowWidth; }
        public int WindowHeight { get => DynamicType.WindowHeight; }

        public DynamicCommandType(dynamic Type)
        {
            this.DynamicType = Type;
        }

        protected ODL.Color DynamicToColor(dynamic Color)
        {
            return new ODL.Color(Color.Red, Color.Green, Color.Blue, Color.Alpha);
        }

        public DynamicCommandType EmptyClone()
        {
            return new DynamicCommandType(DynamicType.CreateEmptyClone());
        }

        public dynamic CallCreateReadOnly()
        {
            return DynamicType.CallCreateReadOnly();
        }

        public dynamic CallLoadReadOnly(CommandUtility Utility)
        {
            return DynamicType.CallLoadReadOnly(Utility);
        }

        public dynamic CallCreateWindow(CommandUtility Utility)
        {
            return DynamicType.CallCreateWindow(Utility);
        }

        public void CallSaveWindow(CommandUtility Utility)
        {
            DynamicType.CallSaveWindow(Utility);
        }
    }
}
