using System.Collections.Immutable;
using System.Reflection;
using Facet;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Facet.Tests.DiagnosticTests;

public class DtoCompanionCodeFixTests
{
    [Fact]
    public async Task Analyzer_ShouldReportBothDiagnostics_WhenCompanionTypesAreMissing()
    {
        const string source = """
            namespace Demo.App;

            public class DepartmentDto
            {
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);

        diagnostics.Select(d => d.Id).Should().BeEquivalentTo(["FAC028", "FAC029"]);
    }

    [Fact]
    public async Task Analyzer_ShouldSkipExistingGetListInput()
    {
        const string source = """
            namespace Demo.App;

            public class DepartmentDto
            {
            }

            public class DepartmentGetListInput
            {
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);

        diagnostics.Select(d => d.Id).Should().BeEquivalentTo(["FAC029"]);
    }

    [Fact]
    public async Task Analyzer_ShouldSkipExistingCreateUpdateDto()
    {
        const string source = """
            namespace Demo.App;

            public class DepartmentDto
            {
            }

            public class CreateUpdateDepartmentDto
            {
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);

        diagnostics.Select(d => d.Id).Should().BeEquivalentTo(["FAC028"]);
    }

    [Fact]
    public async Task Analyzer_ShouldIgnoreNonDtoClasses_AndCompanionDtos()
    {
        const string source = """
            namespace Demo.App;

            public class Department
            {
            }

            public class CreateUpdateDepartmentDto
            {
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task CodeFix_ShouldCreateGetListInputDocumentInSiblingFolder()
    {
        const string source = """
            namespace Demo.App;

            public class DepartmentDto
            {
            }
            """;

        using var workspace = CreateWorkspace();
        var document = CreateDocument(workspace, source, @"E:\Workspace\Dtos\DepartmentDto.cs");
        var diagnostic = (await GetDiagnosticsAsync(document)).Single(d => d.Id == "FAC028");

        var fixedSolution = await ApplyFixAsync(document, diagnostic, "Create DepartmentGetListInput");

        var newDocument = fixedSolution.Projects.Single().Documents.Single(d => d.Name == "DepartmentGetListInput.cs");
        newDocument.Folders.Should().Equal("GetListInputs");
        newDocument.FilePath.Should().EndWith(@"Dtos\GetListInputs\DepartmentGetListInput.cs");

        var text = await newDocument.GetTextAsync();
        text.ToString().Should().Contain("[Facet(typeof(DepartmentDto))]");
        text.ToString().Should().Contain("public partial class DepartmentGetListInput : PagedAndSortedResultRequestDto");
        text.ToString().Should().Contain("namespace Demo.App;");
    }

    [Fact]
    public async Task CodeFix_ShouldCreateCreateUpdateDocumentInSiblingFolder()
    {
        const string source = """
            namespace Demo.App;

            public class DepartmentDto
            {
            }
            """;

        using var workspace = CreateWorkspace();
        var document = CreateDocument(workspace, source, @"E:\Workspace\Dtos\DepartmentDto.cs");
        var diagnostic = (await GetDiagnosticsAsync(document)).Single(d => d.Id == "FAC029");

        var fixedSolution = await ApplyFixAsync(document, diagnostic, "Create CreateUpdateDepartmentDto");

        var newDocument = fixedSolution.Projects.Single().Documents.Single(d => d.Name == "CreateUpdateDepartmentDto.cs");
        newDocument.Folders.Should().Equal("CreateUpdateDtos");
        newDocument.FilePath.Should().EndWith(@"Dtos\CreateUpdateDtos\CreateUpdateDepartmentDto.cs");

        var text = await newDocument.GetTextAsync();
        text.ToString().Should().Contain("[Facet(typeof(DepartmentDto))]");
        text.ToString().Should().Contain("public partial class CreateUpdateDepartmentDto");
        text.ToString().Should().Contain("namespace Demo.App;");
    }

    private static async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(string source)
    {
        using var workspace = CreateWorkspace();
        var document = CreateDocument(workspace, source, @"E:\Workspace\Dtos\DepartmentDto.cs");
        return await GetDiagnosticsAsync(document);
    }

    private static async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(Document document)
    {
        var analyzer = CreateAnalyzer();
        var compilation = await document.Project.GetCompilationAsync();
        compilation.Should().NotBeNull();

        var diagnostics = await compilation!
            .WithAnalyzers([analyzer])
            .GetAnalyzerDiagnosticsAsync();

        return diagnostics.OrderBy(d => d.Id).ToImmutableArray();
    }

    private static async Task<Solution> ApplyFixAsync(Document document, Diagnostic diagnostic, string actionTitle)
    {
        var provider = CreateCodeFixProvider();
        var actions = new List<CodeAction>();
        var context = new CodeFixContext(
            document,
            diagnostic,
            (action, _) => actions.Add(action),
            CancellationToken.None);

        await provider.RegisterCodeFixesAsync(context);

        var action = actions.Single(a => a.Title == actionTitle);
        var operations = await action.GetOperationsAsync(CancellationToken.None);
        var applyChanges = operations.OfType<ApplyChangesOperation>().Single();
        return applyChanges.ChangedSolution;
    }

    private static AdhocWorkspace CreateWorkspace()
    {
        return new AdhocWorkspace();
    }

    private static Document CreateDocument(AdhocWorkspace workspace, string source, string filePath)
    {
        var projectId = ProjectId.CreateNewId();
        var documentId = DocumentId.CreateNewId(projectId);

        var solution = workspace.CurrentSolution
            .AddProject(projectId, "Facet.Diagnostics.Tests", "Facet.Diagnostics.Tests", LanguageNames.CSharp)
            .WithProjectParseOptions(projectId, new CSharpParseOptions(LanguageVersion.Preview))
            .WithProjectCompilationOptions(projectId, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .AddMetadataReferences(projectId, GetMetadataReferences())
            .AddDocument(documentId, Path.GetFileName(filePath), SourceText.From(source), filePath: filePath);

        return solution.GetDocument(documentId)!;
    }

    private static ImmutableArray<MetadataReference> GetMetadataReferences()
    {
        var references = ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))!
            .Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path))
            .ToList();

        references.Add(MetadataReference.CreateFromFile(typeof(FacetAttribute).Assembly.Location));
        return references.Cast<MetadataReference>().ToImmutableArray();
    }

    private static DiagnosticAnalyzer CreateAnalyzer()
    {
        var type = LoadFacetAssembly().GetType("Facet.Analyzers.DtoCompanionAttributeAnalyzer", throwOnError: true)!;
        return (DiagnosticAnalyzer)Activator.CreateInstance(type)!;
    }

    private static CodeFixProvider CreateCodeFixProvider()
    {
        var type = LoadFacetAssembly().GetType("Facet.Analyzers.DtoCompanionCodeFixProvider", throwOnError: true)!;
        return (CodeFixProvider)Activator.CreateInstance(type)!;
    }

    private static Assembly LoadFacetAssembly()
    {
        return Assembly.Load("Facet");
    }
}
