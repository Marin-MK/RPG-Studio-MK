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
                    Console.WriteLine(CommandTypes.Last().Name);
                }
            }
        }
    }

    public class DynamicCommandType
    {
        protected dynamic DynamicType;
        public string Name { get; }
        public string Identifier { get; }

        public DynamicCommandType(dynamic Type)
        {
            this.DynamicType = Type;
            this.Name = DynamicType.Name;
            this.Identifier = DynamicType.Identifier;
        }

        public void CreateReadOnly(Dictionary<string, object> Params)
        {
            DynamicType.CreateReadOnly(Params);
        }
    }
}
