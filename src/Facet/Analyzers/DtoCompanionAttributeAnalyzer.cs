using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Facet.Analyzers;

/// <summary>
/// Analyzer that suggests creating standard companion DTO types for hand-authored xxxDto classes.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DtoCompanionAttributeAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor MissingGetListInputRule = new(
        DtoCompanionCodeFixHelper.GetListInputDiagnosticId,
        "Missing GetListInput companion DTO",
        "DTO '{0}' is missing companion type '{1}'",
        "CodeFix",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Suggests creating an xxxGetListInput companion DTO for a hand-authored xxxDto type.");

    public static readonly DiagnosticDescriptor MissingCreateUpdateDtoRule = new(
        DtoCompanionCodeFixHelper.CreateUpdateDtoDiagnosticId,
        "Missing CreateUpdate companion DTO",
        "DTO '{0}' is missing companion type '{1}'",
        "CodeFix",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Suggests creating a CreateUpdatexxxDto companion DTO for a hand-authored xxxDto type.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(MissingGetListInputRule, MissingCreateUpdateDtoRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        if (context.Symbol is not INamedTypeSymbol namedType ||
            !DtoCompanionCodeFixHelper.TryGetDtoBaseName(namedType, out var dtoBaseName))
        {
            return;
        }

        var location = namedType.Locations.Length > 0 ? namedType.Locations[0] : null;
        if (location == null || !location.IsInSource)
        {
            return;
        }

        ReportMissingCompanionType(context, namedType, dtoBaseName, MissingGetListInputRule);
        ReportMissingCompanionType(context, namedType, dtoBaseName, MissingCreateUpdateDtoRule);
    }

    private static void ReportMissingCompanionType(
        SymbolAnalysisContext context,
        INamedTypeSymbol namedType,
        string dtoBaseName,
        DiagnosticDescriptor rule)
    {
        var companionTypeName = DtoCompanionCodeFixHelper.GetTargetTypeName(rule.Id, dtoBaseName);
        if (DtoCompanionCodeFixHelper.HasCompanionType(namedType, companionTypeName))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(rule, namedType.Locations[0], namedType.Name, companionTypeName));
    }
}
