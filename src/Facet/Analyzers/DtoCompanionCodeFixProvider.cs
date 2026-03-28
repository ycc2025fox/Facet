using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Facet.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DtoCompanionCodeFixProvider)), Shared]
public sealed class DtoCompanionCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(
            DtoCompanionCodeFixHelper.GetListInputDiagnosticId,
            DtoCompanionCodeFixHelper.CreateUpdateDtoDiagnosticId);

    public sealed override FixAllProvider GetFixAllProvider() =>
        WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null)
        {
            return;
        }

        foreach (var diagnostic in context.Diagnostics)
        {
            var node = root.FindNode(diagnostic.Location.SourceSpan);
            var classDeclaration = node.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().FirstOrDefault();
            if (classDeclaration == null)
            {
                continue;
            }

            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
            if (semanticModel?.GetDeclaredSymbol(classDeclaration, context.CancellationToken) is not INamedTypeSymbol dtoType ||
                !DtoCompanionCodeFixHelper.TryGetDtoBaseName(dtoType, out var dtoBaseName))
            {
                continue;
            }

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: DtoCompanionCodeFixHelper.GetCodeActionTitle(diagnostic.Id, dtoBaseName),
                    createChangedSolution: cancellationToken => CreateCompanionDocumentAsync(context.Document, dtoType, dtoBaseName, diagnostic.Id, cancellationToken),
                    equivalenceKey: $"{diagnostic.Id}:{dtoType.ToDisplayString()}"),
                diagnostic);
        }
    }

    internal static Task<Solution> CreateCompanionDocumentAsync(
        Document document,
        INamedTypeSymbol dtoType,
        string dtoBaseName,
        string diagnosticId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var solution = document.Project.Solution;
        var folders = DtoCompanionCodeFixHelper.GetTargetFolders(document, diagnosticId);
        var documentName = DtoCompanionCodeFixHelper.GetTargetFileName(diagnosticId, dtoBaseName);
        var text = SourceText.From(DtoCompanionCodeFixHelper.GenerateDocumentText(diagnosticId, dtoType, dtoBaseName));
        var filePath = DtoCompanionCodeFixHelper.GetTargetFilePath(document, diagnosticId, dtoBaseName);

        var updatedSolution = solution.AddDocument(
            DocumentId.CreateNewId(document.Project.Id),
            documentName,
            text,
            folders,
            filePath);

        return Task.FromResult(updatedSolution);
    }
}
