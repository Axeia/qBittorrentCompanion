using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AutoPropertyChangedSuppressor : DiagnosticSuppressor
{
    private static readonly SuppressionDescriptor SuppressReadonlyRule = new(
        id: "SP0001",
        suppressedDiagnosticId: "IDE0044",
        justification: "Field is modified by generated property setter via ref parameter");

    public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions =>
        [SuppressReadonlyRule];

    public override void ReportSuppressions(SuppressionAnalysisContext context)
    {
        foreach (var diagnostic in context.ReportedDiagnostics)
        {
            if (diagnostic.Id != "IDE0044") continue;

            var syntaxTree = diagnostic.Location.SourceTree;
            if (syntaxTree == null) continue;

            var semanticModel = context.GetSemanticModel(syntaxTree);
            var root = syntaxTree.GetRoot(context.CancellationToken);
            var node = root.FindNode(diagnostic.Location.SourceSpan);

            // Find the field declaration that contains this diagnostic
            var fieldDeclaration = node.Ancestors()
                .OfType<FieldDeclarationSyntax>()
                .FirstOrDefault();

            if (fieldDeclaration == null) continue;

            // Check if any variable in this field declaration has the AutoPropertyChanged attribute
            foreach (var variable in fieldDeclaration.Declaration.Variables)
            {
                var fieldSymbol = semanticModel.GetDeclaredSymbol(variable) as IFieldSymbol;
                if (fieldSymbol == null) continue;

                var hasAutoPropertyChangedAttribute = fieldSymbol.GetAttributes()
                    .Any(attr => attr.AttributeClass?.Name == "AutoPropertyChangedAttribute");

                if (hasAutoPropertyChangedAttribute)
                {
                    context.ReportSuppression(Suppression.Create(SuppressReadonlyRule, diagnostic));
                    break;
                }
            }
        }
    }
}