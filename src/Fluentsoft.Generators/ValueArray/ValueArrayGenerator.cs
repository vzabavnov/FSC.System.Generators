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

namespace Fluentsoft.Generators;

[Generator(LanguageNames.CSharp)]
public class ValueArrayGenerator : IIncrementalGenerator
{
    public const string VALUE_ARRAY_ATTRIBUTE_NAME = "Fluentsoft.System.ValueArrayAttribute`1";

    private static readonly DiagnosticDescriptor _invalidArraySize = new("FSC0001",
        "Cannot create an array of negative size",
        "Cannot create an array with a negative size or empty",
        "FluentsoftGenerator",
        DiagnosticSeverity.Error,
        true);

    private static readonly DiagnosticDescriptor _emptyArray = new("FSC0002",
        "Will create an empty array",
        "Will create an empty array",
        "FluentsoftGenerator",
        DiagnosticSeverity.Warning,
        true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider.ForAttributeWithMetadataName(VALUE_ARRAY_ATTRIBUTE_NAME,
            (node, _) => node is StructDeclarationSyntax,
            (syntaxContext, _) => syntaxContext);

        var combined = context.CompilationProvider.Combine(provider.Collect());

        context.RegisterSourceOutput(combined, (ctx, source) => Execute(ctx, source.Right));
    }

    private static void Execute(SourceProductionContext context, ImmutableArray<GeneratorAttributeSyntaxContext> arr)
    {
        var handledSymbols = new HashSet<string>();

        foreach (var item in arr)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var symbolDisplayString = item.TargetSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            if (handledSymbols.Contains(symbolDisplayString))
            {
                continue;
            }

            handledSymbols.Add(symbolDisplayString);

            var source = Generate(context, item);

            var fileName = $"{item.TargetSymbol.ToDisplayString().EscapeFileName()}.StructFixedArray.g.cs";
            context.AddSource(fileName, source);
        }
    }

    private static string Generate(SourceProductionContext context, GeneratorAttributeSyntaxContext info)
    {
        var attr = info.Attributes[0];

        var size = (int)attr.ConstructorArguments[0].Value!;

        if (size <= 0)
        {
            var location = attr.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation();

            if (size < 0)
            {
                context.ReportDiagnostic(Diagnostic.Create(_invalidArraySize, location));

                return string.Empty;
            }

            context.ReportDiagnostic(Diagnostic.Create(_emptyArray, location));
        }

        var elementTypeSymbol = (INamedTypeSymbol)attr.AttributeClass!.TypeArguments[0];

        using var buffer = new StringWriter(new StringBuilder(4096));
        using var writer = new IndentedTextWriter(buffer);

        writer.WriteSymbolBlock(new[]
            {
                GeneratorHelper.SYSTEM, 
                GeneratorHelper.SYSTEM_RUNTIME_INTEROP_SERVICES, 
                GeneratorHelper.SYSTEM_DIAGNOSTICS_CODE_ANALYSIS, 
                GeneratorHelper.SYSTEM_DIAGNOSTICS
            },
            info.TargetSymbol,
            w => GenerateFixedStruct(w, (INamedTypeSymbol)info.TargetSymbol, elementTypeSymbol, size, context.CancellationToken));

        return buffer.ToString();
    }

    private static void GenerateFixedStruct(IndentedTextWriter writer,
        INamedTypeSymbol structSymbol,
        INamedTypeSymbol elementTypeSymbol,
        int size,
        CancellationToken token)
    {
        var typeName = structSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

        var storageTypeName = typeName + "Storage";

        var itemTypeName = elementTypeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

        writer.WriteLine(GeneratorHelper.STRUCT_LAYOUT_PACK1_ATTRIBUTE);
        writer.WriteLine("[DebuggerDisplay(\"Type of element = {ElementType}; Length = {Length}\")]");
        writer.WriteLine($"partial struct {typeName}");
        writer.AppendOpenBracket();

        writer.WriteLine($"public const int Length = {size};");
        writer.WriteLine();

        writer.WriteLine($"public readonly static Type ElementType = typeof({itemTypeName});");
        writer.WriteLine();

        writer.WriteLine(GeneratorHelper.DEBUGGER_BROWSABLE_NEVER);
        writer.WriteLine($"private {storageTypeName} _storage;");
        writer.WriteLine();

        writer.WriteLine($"public {typeName}(Span<{itemTypeName}> source)");
        writer.AppendOpenBracket();
        writer.WriteLine("source.CopyTo(Span);");
        writer.AppendCloseBracket();
        writer.WriteLine();

        writer.WriteLine($"public {typeName}(Memory<{itemTypeName}> source)");
        writer.AppendOpenBracket();
        writer.WriteLine("source.Span.CopyTo(Span);");
        writer.AppendCloseBracket();
        writer.WriteLine();

        writer.WriteLine($"public {typeName}({itemTypeName}[] source)");
        writer.AppendOpenBracket();
        writer.WriteLine("source.CopyTo(Span);");
        writer.AppendCloseBracket();
        writer.WriteLine();

        writer.WriteLine($"public int Count => {size};");
        writer.WriteLine();

        writer.WriteLine(GeneratorHelper.UNSCOPEDREF_ATTRIBUTE);
        writer.WriteLine(
            $"public Span<{itemTypeName}> Span => MemoryMarshal.Cast<{storageTypeName}, {itemTypeName}>(new Span<{storageTypeName}>(ref _storage));");
        writer.WriteLine();
        writer.WriteLine(GeneratorHelper.UNSCOPEDREF_ATTRIBUTE);
        writer.WriteLine($"public ref {itemTypeName} this[Index idx] => ref Span[idx];");
        writer.WriteLine();

        writer.WriteLine(GeneratorHelper.UNSCOPEDREF_ATTRIBUTE);
        writer.WriteLine($"public Span<{itemTypeName}>.Enumerator GetEnumerator() => Span.GetEnumerator();");
        writer.WriteLine();

        writer.WriteLine($"public {itemTypeName}[] ToArray() => Span.ToArray();");
        writer.WriteLine();

        writer.WriteLine($"public void CopyTo({itemTypeName}[] target, int index)");
        writer.AppendOpenBracket();
        writer.WriteLine("Span.CopyTo(target.AsSpan(index));");
        writer.AppendCloseBracket();
        writer.WriteLine();

        writer.WriteLine($"public void CopyTo(Span<{itemTypeName}> target)");
        writer.AppendOpenBracket();
        writer.WriteLine("Span.CopyTo(target);");
        writer.AppendCloseBracket();
        writer.WriteLine();

        writer.WriteLine($"public void CopyTo(Memory<{itemTypeName}> target)");
        writer.AppendOpenBracket();
        writer.WriteLine("Span.CopyTo(target.Span);");
        writer.AppendCloseBracket();
        writer.WriteLine();

        GenerateStorageStruct(writer, storageTypeName, "private", elementTypeSymbol, "__t", size, token);

        writer.AppendCloseBracket();
    }

    private static void GenerateStorageStruct(IndentedTextWriter writer,
        string storageName,
        string accessibility,
        ISymbol elementSymbol,
        string elementName,
        int arraySize,
        CancellationToken cancellationToken)
    {
        writer.WriteLine(GeneratorHelper.STRUCT_LAYOUT_PACK1_ATTRIBUTE);
        writer.WriteLine($"{accessibility} struct {storageName}");
        writer.AppendOpenBracket();

        writer.WriteLine($"public {storageName}(){{ }}");

        GenerateArray(writer, elementSymbol, elementName, arraySize, cancellationToken);

        writer.AppendCloseBracket();
    }

    private static void GenerateArray(IndentedTextWriter writer,
        ISymbol elementSymbol,
        string elementName,
        int arraySize,
        CancellationToken cancellationToken)
    {
        if (arraySize > 0)
        {
            writer.Write($"public {elementSymbol.ToDisplayString()}");
            writer.Indent++;

            for (var i = 0; i < arraySize; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (i != 0)
                {
                    writer.Write(',');

                    if (i % 12 == 0)
                    {
                        writer.WriteLine();
                    }
                }

                writer.Write($" {elementName}{i:d04}");
            }

            writer.WriteLine(";");
            writer.Indent--;
        }
    }
}