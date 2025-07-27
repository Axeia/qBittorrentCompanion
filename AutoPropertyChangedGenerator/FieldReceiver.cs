using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public class FieldReceiver : ISyntaxReceiver
{
    public List<FieldDeclarationSyntax> CandidateFields { get; } = [];

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is FieldDeclarationSyntax field &&
            field.AttributeLists.Any(a =>
                a.Attributes.Any(attr =>
                    attr.Name.ToString().Contains("AutoPropertyChanged"))))
        {
            CandidateFields.Add(field);
        }
    }
}
