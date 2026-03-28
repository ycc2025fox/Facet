using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Facet.Analyzers;

internal static class DtoCompanionCodeFixHelper
{
    internal const string GetListInputDiagnosticId = "FAC028";
    internal const string CreateUpdateDtoDiagnosticId = "FAC029";

    internal const string GetListInputsFolderName = "GetListInputs";
    internal const string CreateUpdateDtosFolderName = "CreateUpdateDtos";

    internal static bool TryGetDtoBaseName(INamedTypeSymbol namedType, out string dtoBaseName)
    {
        dtoBaseName = string.Empty;

        if (namedType.TypeKind != TypeKind.Class ||
            !namedType.Name.EndsWith("Dto", StringComparison.Ordinal) ||
            namedType.Name.Length <= 3 ||
            namedType.Name.StartsWith("CreateUpdate", StringComparison.Ordinal))
        {
            return false;
        }

        dtoBaseName = namedType.Name.Substring(0, namedType.Name.Length - 3);
        return !string.IsNullOrWhiteSpace(dtoBaseName);
    }

    internal static bool HasCompanionType(INamedTypeSymbol dtoType, string companionTypeName)
    {
        if (dtoType.ContainingNamespace is not INamespaceSymbol namespaceSymbol)
        {
            return false;
        }

        foreach (var member in namespaceSymbol.GetTypeMembers(companionTypeName))
        {
            if (SymbolEqualityComparer.Default.Equals(member.ContainingNamespace, dtoType.ContainingNamespace))
            {
                return true;
            }
        }

        return false;
    }

    internal static string GetTargetFileName(string diagnosticId, string dtoBaseName)
    {
        return diagnosticId switch
        {
            GetListInputDiagnosticId => $"{dtoBaseName}GetListInput.cs",
            CreateUpdateDtoDiagnosticId => $"CreateUpdate{dtoBaseName}Dto.cs",
            _ => throw new ArgumentOutOfRangeException(nameof(diagnosticId), diagnosticId, "Unsupported companion DTO diagnostic id.")
        };
    }

    internal static string GetTargetTypeName(string diagnosticId, string dtoBaseName)
    {
        return diagnosticId switch
        {
            GetListInputDiagnosticId => $"{dtoBaseName}GetListInput",
            CreateUpdateDtoDiagnosticId => $"CreateUpdate{dtoBaseName}Dto",
            _ => throw new ArgumentOutOfRangeException(nameof(diagnosticId), diagnosticId, "Unsupported companion DTO diagnostic id.")
        };
    }

    internal static string GetTargetFolderName(string diagnosticId)
    {
        return diagnosticId switch
        {
            GetListInputDiagnosticId => GetListInputsFolderName,
            CreateUpdateDtoDiagnosticId => CreateUpdateDtosFolderName,
            _ => throw new ArgumentOutOfRangeException(nameof(diagnosticId), diagnosticId, "Unsupported companion DTO diagnostic id.")
        };
    }

    internal static IReadOnlyList<string> GetTargetFolders(Document document, string diagnosticId)
    {
        return document.Folders.Concat(new[] { GetTargetFolderName(diagnosticId) }).ToArray();
    }

    internal static string? GetTargetFilePath(Document document, string diagnosticId, string dtoBaseName)
    {
        if (string.IsNullOrWhiteSpace(document.FilePath))
        {
            return null;
        }

        var directory = Path.GetDirectoryName(document.FilePath);
        if (string.IsNullOrWhiteSpace(directory))
        {
            return null;
        }

        return Path.Combine(directory, GetTargetFolderName(diagnosticId), GetTargetFileName(diagnosticId, dtoBaseName));
    }

    internal static string GenerateDocumentText(string diagnosticId, INamedTypeSymbol dtoType, string dtoBaseName)
    {
        var namespaceName = dtoType.ContainingNamespace?.IsGlobalNamespace == false
            ? dtoType.ContainingNamespace.ToDisplayString()
            : null;

        var typeName = GetTargetTypeName(diagnosticId, dtoBaseName);
        var baseType = diagnosticId == GetListInputDiagnosticId
            ? " : PagedAndSortedResultRequestDto"
            : string.Empty;

        var lines = new List<string>
        {
            "using Facet;",
            string.Empty
        };

        if (!string.IsNullOrWhiteSpace(namespaceName))
        {
            lines.Add($"namespace {namespaceName};");
            lines.Add(string.Empty);
        }

        lines.Add($"[Facet(typeof({dtoType.Name}))]");
        lines.Add($"public partial class {typeName}{baseType}");
        lines.Add("{");
        lines.Add("}");

        return string.Join("\r\n", lines);
    }

    internal static string GetCodeActionTitle(string diagnosticId, string dtoBaseName)
    {
        return diagnosticId switch
        {
            GetListInputDiagnosticId => $"Create {dtoBaseName}GetListInput",
            CreateUpdateDtoDiagnosticId => $"Create CreateUpdate{dtoBaseName}Dto",
            _ => throw new ArgumentOutOfRangeException(nameof(diagnosticId), diagnosticId, "Unsupported companion DTO diagnostic id.")
        };
    }
}
