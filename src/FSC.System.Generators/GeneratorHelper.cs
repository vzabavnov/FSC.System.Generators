// Copyright 2023 Fluent Software Corporation
// Author: Vadim Zabavnov (mailto:vzabavnov@fluentsoft.net; mailto:zabavnov@gmail.com)
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace FSC.System.Generators;

internal static class GeneratorHelper
{
    public const string DEBUGGER_BROWSABLE_NEVER = "[DebuggerBrowsable(DebuggerBrowsableState.Never)]";
    public const string ENABLE_NULLABLE_CONTEXT = "#nullable enable";
    public const string SUPPRESS_OBSOLETE_WARNINGS_PRAGMA = "#pragma warning disable CS0612,CS0618";
    public const string SUPPRESS_TYPE_CONFLICTS_WARNINGS_PRAGMA = "#pragma warning disable CS0436";
    public const string INHERIT_DOC_COMMENT = "/// <inheritdoc/>";
    public const string GENERATOR_CODE_ATTRIBUTE = "[GeneratedCode(\"FSC.System.Generators.StructFixedArray\", \"1.0.0.0\")]";
    public const string STRUCT_LAYOUT_PACK1_ATTRIBUTE = "[StructLayout(LayoutKind.Sequential, Pack = 1)]";

    public const string SYSTEM = "System";
    public const string SYSTEM_RUNTIME_INTEROP_SERVICES = "System.Runtime.InteropServices";
    public const string SYSTEM_DIAGNOSTICS = "System.Diagnostics";
    public const string SYSTEM_CODEDOM_COMPILER = "System.CodeDom.Compiler";
    public const string SYSTEM_DIAGNOSTICS_CODE_ANALYSIS = "System.Diagnostics.CodeAnalysis";

    public const string UNSCOPEDREF_ATTRIBUTE = "[UnscopedRef]";

    public static void AppendOpenBracket(this IndentedTextWriter writer)
    {
        writer.WriteLine("{");
        writer.Indent++;
    }

    public static void AppendCloseBracket(this IndentedTextWriter writer)
    {
        writer.Indent--;
        writer.WriteLine("}");
    }

    public static void WriteLines(this IndentedTextWriter writer, IEnumerable<string> lines)
    {
        foreach (var line in lines)
        {
            writer.WriteLine(line);
        }
    }

    public static void WriteLine(this IndentedTextWriter writer, int offset, string value)
    {
        writer.Indent += offset;
        writer.WriteLine(value);
        writer.Indent -= offset;
    }

    public static IndentedTextWriter Using(this IndentedTextWriter writer, string @namespace)
    {
        writer.WriteLine($"using {@namespace};");

        return writer;
    }

    public static IndentedTextWriter Using(this IndentedTextWriter writer, string alias, string aliasFor)
    {
        writer.WriteLine($"using {alias}={aliasFor};");

        return writer;
    }

    public static IEnumerable<INamespaceOrTypeSymbol> AllContainingNamespacesAndTypes(this ISymbol symbol, bool includeSelf)
    {
        if (includeSelf && symbol is INamespaceOrTypeSymbol self)
        {
            yield return self;
        }

        while (true)
        {
            symbol = symbol.ContainingSymbol;

            if (symbol is INamespaceOrTypeSymbol namespaceOrTypeSymbol)
            {
                yield return namespaceOrTypeSymbol;
            }
            else
            {
                yield break;
            }
        }
    }

    public static IEnumerable<INamespaceOrTypeSymbol> ContainingNamespaceAndTypes(this ISymbol symbol, bool includeSelf)
    {
        foreach (var item in AllContainingNamespacesAndTypes(symbol, includeSelf))
        {
            yield return item;

            if (item.IsNamespace)
            {
                yield break;
            }
        }
    }

    public static IndentedTextWriter WriteSymbolBlock(this IndentedTextWriter writer,
        string[] usingStrings,
        ISymbol symbol,
        Action<IndentedTextWriter> writeContext)
    {
        foreach (var usingString in usingStrings)
        {
            writer.Using(usingString);
        }

        var names = ImmutableArray.CreateRange(ContainingNamespaceAndTypes(symbol, false));

        bool isGlobalNamespace = false;
        for (var idx = names.Length - 1; idx >= 0; idx--)
        {
            var name = names[idx];

            if (name.IsNamespace)
            {
                writer.WriteLine();
                writer.WriteLine(ENABLE_NULLABLE_CONTEXT);
                writer.WriteLine(SUPPRESS_OBSOLETE_WARNINGS_PRAGMA);
                writer.WriteLine(SUPPRESS_TYPE_CONFLICTS_WARNINGS_PRAGMA);
                writer.WriteLine();

                var namespaceName =
                    name.ToDisplayString(
                        SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));

                if (!string.IsNullOrEmpty(namespaceName))
                {
                    writer.WriteLine($"namespace {namespaceName}");
                    writer.AppendOpenBracket();
                }
                else
                {
                    //global namespace
                    isGlobalNamespace = true;
                }
            }
            else
            {
                var typeDeclarationSyntax = name.DeclaringSyntaxReferences
                    .Select(x => x.GetSyntax())
                    .OfType<TypeDeclarationSyntax>()
                    .First();

                var keyword = typeDeclarationSyntax.Kind() switch
                {
                    SyntaxKind.ClassDeclaration => "class",
                    SyntaxKind.RecordDeclaration => "record",
                    (SyntaxKind)9068 => "record struct", // RecordStructDeclaration
                    SyntaxKind.StructDeclaration => "struct",
                    var x => throw new ArgumentOutOfRangeException($"Syntax kind {x} not supported")
                };

                writer.WriteLine($"partial {keyword} {name.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)}");
                writer.AppendOpenBracket();
            }
        }

        writeContext(writer);

        for (var i = 0; i < (isGlobalNamespace ? names.Length - 1 : names.Length); i++)
        {
            writer.AppendCloseBracket();
        }

        return writer;
    }

    public static string EscapeFileName(this string fileName)
    {
        return new[] { '<', '>', ',' }.Aggregate(new StringBuilder(fileName), (s, c) => s.Replace(c, '_')).ToString();
    }
}