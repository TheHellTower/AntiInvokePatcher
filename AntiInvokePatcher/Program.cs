using dnlib.DotNet;
using dnlib.DotNet.Writer;
using System.Reflection;

namespace AntiInvokePatcher
{
    internal class Program
    {
        private static ModuleDefMD Module = null;
        private static string NewLocation = string.Empty;
        private static MetadataFlags Flags = MetadataFlags.PreserveAll;
        private static DummyLogger Logger = DummyLogger.NoThrowInstance;
        private static ModuleWriterOptions ManagedWriterOptions = null;
        private static NativeModuleWriterOptions NativeWriterOptions = null;
        static void Main(string[] args)
        {
            Module = ModuleDefMD.Load(args[0]);

            NewLocation = Module.Location.Insert(Module.Location.Length - 4, "-AntiInvokes");

            ManagedWriterOptions = new ModuleWriterOptions(Module) { Logger = Logger };
            ManagedWriterOptions.MetadataOptions.Flags = Flags;

            NativeWriterOptions = new NativeModuleWriterOptions(Module, true) { Logger = Logger };
            NativeWriterOptions.MetadataOptions.Flags = Flags;

            AntiInvokes();

            //Console.ReadLine(); //Hesitate because of people that close instead of clicking enter lmfao

            if (Module.IsILOnly)
                Module.Write(NewLocation, ManagedWriterOptions);
            else
                Module.NativeWrite(NewLocation, NativeWriterOptions);

            Console.ReadLine();
        }

        private static void AntiInvokes()
        {
            int patchedCA = 0;
            
            foreach (TypeDef Type in Module.Types.Where(T => T.HasMethods))
                foreach (MethodDef Method in Type.Methods.Where(M => M.HasBody && M.Body.HasInstructions && M.Body.Instructions.Count() > 2))
                    for (var I = 0; I < Method.Body.Instructions.Count(); I++)
                    {
                        if(Method.Body.Instructions[I].ToString().Contains("::") && Method.Body.Instructions[I].ToString().Split(new string[] { "::" }, StringSplitOptions.None)[1] == "GetCallingAssembly()")
                        {
                            Method.Body.Instructions[I].Operand = Module.Import(typeof(Assembly).GetMethod("GetExecutingAssembly"));
                            patchedCA++;
                        }
                    }
            Console.WriteLine(patchedCA == 0 ? "No Anti-Invoke Detected." : $"{patchedCA} Anti-Invoke{(patchedCA == 1 ? "" : "s")} Patched !");
        }
    }
}