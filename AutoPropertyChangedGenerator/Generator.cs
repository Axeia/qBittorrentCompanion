using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using System.Collections.Immutable;

/// <summary>
/// Generates reactive properties for annotated fields using ReactiveUI.
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

    public record ProxyPropertyInfo(
        string PropertyName,
        string ProxyProperty,
        string Type,
        string FieldName,
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

        var proxyFields = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsFieldWithAutoProxyPropertyChanged(node),
                transform: static (ctx, _) =>
                {
                    var infos = GetProxyPropertyInfos(ctx);
                    var fieldSyntax = (FieldDeclarationSyntax)ctx.Node;
                    var classDeclaration = fieldSyntax.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
                    var classSymbol = classDeclaration != null
                        ? ctx.SemanticModel.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol
                        : null;
                    return (Infos: infos, ClassSymbol: classSymbol);
                }
            )
            .Where(t => t.Infos.Any() && t.ClassSymbol is not null);

        context.RegisterSourceOutput(reactiveFields, (ctx, t) =>
        {
            var field = (FieldInfo)t.Info!;
            var source = GenerateReactiveProperty(field);
            ctx.AddSource($"{field.ContainingClass.Replace(".", "_")}_{field.PropertyName}_Reactive.g.cs", SourceText.From(source, Encoding.UTF8));
        });

        context.RegisterSourceOutput(proxyFields, (ctx, t) =>
        {
            var infos = t.Infos;
            var firstInfo = infos.First();
            var source = GenerateProxyProperties(infos);
            ctx.AddSource($"{firstInfo.ContainingClass.Replace(".", "_")}_{firstInfo.FieldName}_Proxy.g.cs", SourceText.From(source, Encoding.UTF8));
        });
    }

    static bool IsFieldWithAutoPropertyChanged(SyntaxNode node)
    {
        return node is FieldDeclarationSyntax field &&
               field.AttributeLists
                    .SelectMany(al => al.Attributes)
                    .Any(attr => attr.Name.ToString().Contains("AutoPropertyChanged") &&
                                !attr.Name.ToString().Contains("AutoProxyPropertyChanged"));
    }

    static bool IsFieldWithAutoProxyPropertyChanged(SyntaxNode node)
    {
        return node is FieldDeclarationSyntax field &&
               field.AttributeLists
                    .SelectMany(al => al.Attributes)
                    .Any(attr => attr.Name.ToString().Contains("AutoProxyPropertyChanged"));
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

    static List<ProxyPropertyInfo> GetProxyPropertyInfos(GeneratorSyntaxContext context)
    {
        var result = new List<ProxyPropertyInfo>();

        if (context.Node is not FieldDeclarationSyntax fieldSyntax) return result;

        var variable = fieldSyntax.Declaration.Variables.FirstOrDefault();
        if (variable is null) return result;

        var fieldSymbol = context.SemanticModel.GetDeclaredSymbol(variable) as IFieldSymbol;
        if (fieldSymbol is null) return result;

        var classSymbol = fieldSymbol.ContainingType;
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

        foreach (var attr in fieldSymbol.GetAttributes())
        {
            if (attr.AttributeClass?.Name != "AutoProxyPropertyChangedAttribute") continue;

            string? propertyPath = null;
            string? propertyName = null;

            if (attr.ConstructorArguments.Length >= 1)
            {
                propertyPath = attr.ConstructorArguments[0].Value?.ToString();
            }
            if (attr.ConstructorArguments.Length >= 2)
            {
                propertyName = attr.ConstructorArguments[1].Value?.ToString();
            }

            foreach (var namedArg in attr.NamedArguments)
            {
                switch (namedArg.Key)
                {
                    case "Property":
                        propertyPath = namedArg.Value.Value?.ToString();
                        break;
                    case "Name":
                        propertyName = namedArg.Value.Value?.ToString();
                        break;
                }
            }

            if (string.IsNullOrEmpty(propertyPath)) continue;

            var finalPropertyName = propertyName ?? propertyPath.Split('.').Last();
            var propertyType = DeterminePropertyType(context, fieldSymbol, propertyPath);

            result.Add(new ProxyPropertyInfo(
                PropertyName: finalPropertyName,
                ProxyProperty: propertyPath,
                Type: propertyType,
                FieldName: fieldSymbol.Name,
                ContainingClass: fullClassName,
                Namespace: ns
            ));
        }

        return result;
    }

    static string DeterminePropertyType(GeneratorSyntaxContext context, IFieldSymbol fieldSymbol, string propertyPath)
    {
        var fieldType = fieldSymbol.Type;
        var parts = propertyPath.Split('.');

        var currentType = fieldType;
        foreach (var part in parts)
        {
            var property = currentType.GetMembers(part).OfType<IPropertySymbol>().FirstOrDefault();
            if (property != null)
            {
                currentType = property.Type;
            }
            else
            {
                return "object";
            }
        }

        return currentType.ToDisplayString();
    }

    static string ToPascal(string name) =>
        name.TrimStart('_') is string trimmed && trimmed.Length > 0
            ? char.ToUpper(trimmed[0]) + trimmed.Substring(1)
            : name;

    static string GenerateReactiveProperty(FieldInfo field)
    {
        var namespaceDeclaration = string.IsNullOrEmpty(field.Namespace)
            ? ""
            : $"namespace {field.Namespace}\n{{\n";
        var namespaceClosing = string.IsNullOrEmpty(field.Namespace)
            ? ""
            : "\n}";

        var classDeclarations = new List<string>();
        var classClosings = new List<string>();

        var classParts = field.ContainingClass.Split('.');
        for (int i = 0; i < classParts.Length; i++)
        {
            classDeclarations.Add($"    public partial class {classParts[i]}");
            classDeclarations.Add("    {");
            classClosings.Insert(0, "    }");
        }

        var propertyImplementation = $@"        public {field.Type} {field.PropertyName}
        {{
            get => {field.FieldName};
            set => this.RaiseAndSetIfChanged(ref {field.FieldName}, value);
        }}";

        var classHierarchy = string.Join("\n", classDeclarations);
        var closingBraces = string.Join("\n", classClosings);

        return $@"using ReactiveUI;

{namespaceDeclaration}{classHierarchy}

{propertyImplementation}
{closingBraces}{namespaceClosing}";
    }

    static string GenerateProxyProperties(List<ProxyPropertyInfo> infos)
    {
        var firstInfo = infos.First();

        var namespaceDeclaration = string.IsNullOrEmpty(firstInfo.Namespace)
            ? ""
            : $"namespace {firstInfo.Namespace}\n{{\n";
        var namespaceClosing = string.IsNullOrEmpty(firstInfo.Namespace)
            ? ""
            : "\n}";

        var classDeclarations = new List<string>();
        var classClosings = new List<string>();

        var classParts = firstInfo.ContainingClass.Split('.');
        for (int i = 0; i < classParts.Length; i++)
        {
            classDeclarations.Add($"    public partial class {classParts[i]}");
            classDeclarations.Add("    {");
            classClosings.Insert(0, "    }");
        }

        var properties = new StringBuilder();
        foreach (var info in infos)
        {
            var propertyImplementation = $@"        /// <inheritdoc cref=""{info.ProxyProperty}""/>
        public {info.Type} {info.PropertyName}
        {{
            get => {info.FieldName}.{info.ProxyProperty};
            set
            {{
                if (!System.Collections.Generic.EqualityComparer<{info.Type}>.Default.Equals({info.FieldName}.{info.ProxyProperty}, value))
                {{
                    {info.FieldName}.{info.ProxyProperty} = value;
                    this.RaisePropertyChanged(nameof({info.PropertyName}));
                }}
            }}
        }}";

            properties.AppendLine(propertyImplementation);
            properties.AppendLine();
        }

        var classHierarchy = string.Join("\n", classDeclarations);
        var closingBraces = string.Join("\n", classClosings);

        return $@"using ReactiveUI;

{namespaceDeclaration}{classHierarchy}
{properties}{closingBraces}{namespaceClosing}";
    }
}