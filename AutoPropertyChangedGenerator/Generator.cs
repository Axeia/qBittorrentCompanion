using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using System.Collections.Immutable;

/// <summary>
/// Generates reactive properties for annotated fields.
/// </summary>
[Generator(LanguageNames.CSharp)]
public class Generator : IIncrementalGenerator
{
    public record FieldInfo(
        string FieldName,
        string PropertyName,
        string Type,
        string ContainingClass,
        string Namespace
    );

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var reactiveFields = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsFieldWithAutoPropertyChanged(node),
                transform: static (ctx, _) =>
                {
                    var info = GetFieldInfo(ctx);
                    var fieldSyntax = (FieldDeclarationSyntax)ctx.Node;
                    var classDeclaration = fieldSyntax.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
                    var classSymbol = classDeclaration != null
                        ? ctx.SemanticModel.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol
                        : null;
                    return (Info: info, ClassSymbol: classSymbol);
                }
            )
            .Where(t => t.Info is FieldInfo && t.ClassSymbol is not null);

        context.RegisterSourceOutput(reactiveFields, (ctx, t) =>
        {
            var field = (FieldInfo)t.Info!;
            var classSymbol = t.ClassSymbol!;

            var hasPropertyChanged = ClassHasPropertyChanged(classSymbol);
            var source = GenerateReactiveProperty(field, hasPropertyChanged);
            ctx.AddSource($"{field.ContainingClass}_{field.PropertyName}_Reactive.g.cs", SourceText.From(source, Encoding.UTF8));
        });
    }

    static bool IsFieldWithAutoPropertyChanged(SyntaxNode node)
    {
        return node is FieldDeclarationSyntax field &&
               field.AttributeLists
                    .SelectMany(al => al.Attributes)
                    .Any(attr => attr.Name.ToString().Contains("AutoPropertyChanged"));
    }

    static FieldInfo? GetFieldInfo(GeneratorSyntaxContext context)
    {
        if (context.Node is not FieldDeclarationSyntax fieldSyntax) return null;

        var variable = fieldSyntax.Declaration.Variables.FirstOrDefault();
        if (variable is null) return null;

        var fieldSymbol = context.SemanticModel.GetDeclaredSymbol(variable) as IFieldSymbol;
        if (fieldSymbol is null) return null;

        foreach (var attr in fieldSymbol.GetAttributes())
        {
            if (attr.AttributeClass?.Name != "AutoPropertyChangedAttribute") continue;

            var classSymbol = fieldSymbol.ContainingType;
            var ns = classSymbol.ContainingNamespace.IsGlobalNamespace
                ? ""
                : classSymbol.ContainingNamespace.ToDisplayString();

            return new FieldInfo(
                FieldName: fieldSymbol.Name,
                PropertyName: ToPascal(fieldSymbol.Name),
                Type: fieldSymbol.Type.ToDisplayString(),
                ContainingClass: classSymbol.Name,
                Namespace: ns
            );
        }

        return null;
    }

    static string ToPascal(string name) =>
        name.TrimStart('_') is string trimmed && trimmed.Length > 0
            ? char.ToUpper(trimmed[0]) + trimmed.Substring(1)
            : name;

    static string GenerateReactiveProperty(FieldInfo field, bool hasPropertyChanged)
    {
        var usingDirectives = hasPropertyChanged ? "using ReactiveUI;\n" : "";

        var namespaceDeclaration = string.IsNullOrEmpty(field.Namespace)
            ? ""
            : $"namespace {field.Namespace}\n{{\n";
        var namespaceClosing = string.IsNullOrEmpty(field.Namespace)
            ? ""
            : "\n}";

        // If the class already implements INotifyPropertyChanged, don't add it again
        var interfaceDeclaration = hasPropertyChanged ? "" : " : System.ComponentModel.INotifyPropertyChanged";
        var eventAndMethodDeclaration = hasPropertyChanged ? "" : @"
        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }";

        // For ReactiveObject, use this.RaiseAndSetIfChanged pattern
        var propertyImplementation = hasPropertyChanged ?
            $@"        public {field.Type} {field.PropertyName}
        {{
            get => {field.FieldName};
            set => this.RaiseAndSetIfChanged(ref {field.FieldName}, value);
        }}" :
            $@"        public {field.Type} {field.PropertyName}
        {{
            get => {field.FieldName};
            set
            {{
                if (!System.Collections.Generic.EqualityComparer<{field.Type}>.Default.Equals({field.FieldName}, value))
                {{
                    {field.FieldName} = value;
                    OnPropertyChanged();
                }}
            }}
        }}";

        return $@"{usingDirectives}{namespaceDeclaration}    public partial class {field.ContainingClass}{interfaceDeclaration}
    {{{eventAndMethodDeclaration}

{propertyImplementation}
    }}{namespaceClosing}";
    }

    static bool ClassHasPropertyChanged(INamedTypeSymbol classSymbol)
    {
        // Check if the class or any of its base classes implements INotifyPropertyChanged
        var current = classSymbol;
        while (current != null)
        {
            if (current.AllInterfaces.Any(i => i.Name == "INotifyPropertyChanged" &&
                i.ContainingNamespace.ToDisplayString() == "System.ComponentModel"))
            {
                return true;
            }

            // Also check for ReactiveUI's IReactiveNotifyPropertyChanged
            if (current.AllInterfaces.Any(i => i.Name.Contains("IReactiveNotifyPropertyChanged")))
            {
                return true;
            }

            current = current.BaseType;
        }
        return false;
    }
}