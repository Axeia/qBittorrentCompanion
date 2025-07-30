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

            // Build the full class name including nested class hierarchy
            var classNames = new List<string>();
            var currentType = classSymbol;
            while (currentType != null && !currentType.ContainingNamespace.Equals(currentType, SymbolEqualityComparer.Default))
            {
                classNames.Insert(0, currentType.Name);
                currentType = currentType.ContainingType;
            }

            var fullClassName = string.Join(".", classNames);
            var ns = classSymbol.ContainingNamespace.IsGlobalNamespace
                ? ""
                : classSymbol.ContainingNamespace.ToDisplayString();

            return new FieldInfo(
                FieldName: fieldSymbol.Name,
                PropertyName: ToPascal(fieldSymbol.Name),
                Type: fieldSymbol.Type.ToDisplayString(),
                ContainingClass: fullClassName,
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

        // Handle nested classes by generating the proper class hierarchy
        var classDeclarations = new List<string>();
        var classClosings = new List<string>();

        var classParts = field.ContainingClass.Split('.');
        for (int i = 0; i < classParts.Length; i++)
        {
            var isLastClass = i == classParts.Length - 1;
            var interfaceDeclaration = (isLastClass && !hasPropertyChanged) ? " : System.ComponentModel.INotifyPropertyChanged" : "";

            classDeclarations.Add($"    public partial class {classParts[i]}{interfaceDeclaration}");
            classDeclarations.Add("    {");
            classClosings.Insert(0, "    }");
        }

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

        var classHierarchy = string.Join("\n", classDeclarations);
        var closingBraces = string.Join("\n", classClosings);

        return $@"{usingDirectives}{namespaceDeclaration}{classHierarchy}{eventAndMethodDeclaration}

{propertyImplementation}
{closingBraces}{namespaceClosing}";
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