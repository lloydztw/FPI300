#region AUTHOR
/*
 *
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 *
 * REVISION:
 *      2026-10-08 使用 ChatGPT Codex  重整 (by LeTian Chang)
 *
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 *
 */
#endregion

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

internal static class SyntaxChecks
{
    #region PRIVATE_METHODS

    static int Main(string[] args)
    {
        // Resolve compiler dependencies from the installed Visual Studio, without copying SDKs.
        AppDomain.CurrentDomain.AssemblyResolve += (sender, e) =>
        {
            var path = Path.Combine(args[0], new AssemblyName(e.Name).Name + ".dll");
            return File.Exists(path) ? Assembly.LoadFrom(path) : null;
        };
        return Check(args[1]);
    }

    static int Check(string projectPath)
    {
        var document = XDocument.Load(projectPath);
        var ns = document.Root.Name.Namespace;
        var sourceFiles = document.Descendants(ns + "Compile")
            .Select(item => Path.Combine(Path.GetDirectoryName(projectPath), item.Attribute("Include").Value))
            .ToArray();
        var configurations = new[]
        {
            new[] { "DEBUG", "TRACE", "OPT_CALIB_V5", "OPT_TRANSFORM_V38", "OPT_LETIAN_AUTO_LAYOUT", "FATEK" },
            new[] { "TRACE", "OPT_CALIB_V5", "OPT_TRANSFORM_V38", "OPT_LETIAN_AUTO_LAYOUT", "FATEK" },
            new[] { "DEBUG", "TRACE", "OPT_TRANSFORM_V38", "OPT_LETIAN_AUTO_LAYOUT", "FX3U", "XFATEK" }
        };
        int errors = 0;
        foreach (var symbols in configurations)
        {
            var options = new CSharpParseOptions(LanguageVersion.CSharp7_3, preprocessorSymbols: symbols);
            foreach (var file in sourceFiles)
            {
                var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(file), options, file);
                foreach (var diagnostic in tree.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error))
                {
                    Console.Error.WriteLine(diagnostic);
                    errors++;
                }
            }
        }
        Console.WriteLine("Syntax: {0} source files, {1} symbol sets, {2} errors.", sourceFiles.Length, configurations.Length, errors);
        return errors == 0 ? 0 : 1;
    }

    #endregion
}
