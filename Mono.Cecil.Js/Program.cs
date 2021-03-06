﻿using System.IO;
using System.Linq;
using Jint;
using Jint.CommonJS;

namespace Mono.Cecil.Js
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var typeGen = new TypingsGenerator.TypingsGenerator())
            {
                typeGen.AddAssembly(typeof(Mono.Cecil.AssemblyDefinition).Assembly);
                typeGen.AddAssembly(typeof(MonoMod.MonoModder).Assembly);
                typeGen.AddType<System.Net.Http.HttpClient>();
                typeGen.AddType(typeof(System.IO.File));
                typeGen.AddType(typeof(System.IO.Directory));
                typeGen.AddType(typeof(System.Console));
                typeGen.AddType(typeof(System.IO.Path));
                typeGen.AddType(typeof(System.IO.Compression.ZipFile));
                typeGen.AddType(typeof(System.IO.DirectoryInfo));
                typeGen.AddType<Cecil.ResourceExtractor>();
                typeGen.AddType<Program>();
                typeGen.AddType<string>();
                typeGen.AddType<System.IDisposable>();

                typeGen.Write("../../../modules/common/global.autogenerated.d.ts");

                var engine = new Engine(cfg => cfg.AllowClr(
                    typeGen.Types.Select(x => x.Assembly).Distinct().ToArray()
                ));
                {
                    engine.ClrTypeConverter = new ClrTypeConverter(engine);
                    engine.SetValue("console", new Javascript.JsConsole());
                    engine.SetValue("MonoMod", new Jint.Runtime.Interop.NamespaceReference(engine, "MonoMod"));
                    engine.SetValue("Mono", new Jint.Runtime.Interop.NamespaceReference(engine, "Mono"));

                    foreach (var directory in Directory.EnumerateDirectories("modules"))
                    {
                        var moduleName = directory.Split(Path.DirectorySeparatorChar).Last();
                        var moduleFolder = Path.Combine(Directory.GetCurrentDirectory(), directory, "dist");
                        var moduleFile = Path.Combine(moduleFolder, "index.js");

                        engine.CommonJS().RunMain(moduleFile);
                    }
                }
            }
        }
    }
}
