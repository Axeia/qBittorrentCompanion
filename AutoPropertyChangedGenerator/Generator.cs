using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Generates reactive properties for annotated fields using ReactiveUI.
/// </summary>
[Generator(LanguageNames.CSharp)]
public class Generator : IIncrementalGenerator
{
    // Define diagnostic descriptors
    private static readonly DiagnosticDescriptor MissingReactiveObjectDiagnostic = new(
        id: "RG001",
        title: "Class must inherit from ReactiveObject or implement IReactiveNotifyPropertyChanged",
        messageFormat: "Class '{0}' uses ReactiveUI proxy properties but does not inherit from ReactiveObject or implement IReactiveNotifyPropertyChanged. Please make the class inherit from ReactiveObject or implement IReactiveNotifyPropertyChanged.",
        category: "ReactiveGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Classes using AutoProxyPropertyChanged must inherit from ReactiveObject or implement IReactiveNotifyPropertyChanged to use ReactiveUI's RaisePropertyChanged method.");

    public record FieldInfo(
        string FieldName,
        string PropertyName,
        string Type,
        string ContainingClass,
        string Namespace
    );

    public record ProxyPropertyInfo(
        string PropertyName, // The name of the *generated* proxy property (e.g., "SmartFilter" or "Episodes")
        string ProxyPropertyPath, // The full path to the source property (e.g., "Rule.SmartFilter")
        string Type,          // The type of the target property (e.g., "string")
        string FieldName,     // The name of the backing field (e.g., "_rule")
        string ContainingClass,
        string Namespace
    );

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Pipeline for [AutoPropertyChanged]
        var reactiveFields = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsFieldWithAutoPropertyChanged(node),
                transform: static (ctx, _) =>
                {
                    var info = GetFieldInfo(ctx); // Returns single FieldInfo or null
                    var fieldSyntax = (FieldDeclarationSyntax)ctx.Node;
                    var classDeclaration = fieldSyntax.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
                    var classSymbol = classDeclaration != null
                        ? ctx.SemanticModel.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol
                        : null;
                    return (Info: info, ClassSymbol: classSymbol, Location: fieldSyntax.GetLocation());
                }
            )
            .Where(t => t.Info is not null && t.ClassSymbol is not null);

        // Pipeline for [AutoProxyPropertyChanged]
        var proxyFields = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsFieldWithAutoProxyPropertyChanged(node),
                transform: static (ctx, _) =>
                {
                    var infos = GetProxyPropertyInfos(ctx); // Returns list of ProxyPropertyInfo
                    var fieldSyntax = (FieldDeclarationSyntax)ctx.Node;
                    var classDeclaration = fieldSyntax.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
                    var classSymbol = classDeclaration != null
                        ? ctx.SemanticModel.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol
                        : null;
                    return (Infos: infos, ClassSymbol: classSymbol, Location: fieldSyntax.GetLocation());
                }
            )
            .Where(t => t.Infos.Any() && t.ClassSymbol is not null);

        // Register source output for AutoPropertyChanged  
        context.RegisterSourceOutput(reactiveFields, (ctx, t) =>
        {
            var field = t.Info!;
            var classSymbol = t.ClassSymbol!;
            var hasReactiveUISupport = ClassHasReactiveUISupport(classSymbol);
            var source = GenerateReactiveProperty(field, hasReactiveUISupport);
            ctx.AddSource($"{field.ContainingClass.Replace(".", "_")}_{field.PropertyName}_Reactive.g.cs", SourceText.From(source, Encoding.UTF8));
        });

        // Register source output for AutoProxyPropertyChanged with diagnostic checking
        context.RegisterSourceOutput(proxyFields, (ctx, t) =>
        {
            var infos = t.Infos;
            var classSymbol = t.ClassSymbol!;
            var location = t.Location;

            // Check if the class has ReactiveUI support
            var hasReactiveUISupport = ClassHasReactiveUISupport(classSymbol);
            if (!hasReactiveUISupport)
            {
                // Report diagnostic error
                var diagnostic = Diagnostic.Create(
                    MissingReactiveObjectDiagnostic,
                    location,
                    classSymbol.ToDisplayString());
                ctx.ReportDiagnostic(diagnostic);
                return; // Don't generate code if there's an error
            }

            var source = GenerateProxyProperties(infos, hasReactiveUISupport);

            // Name the file based on the backing field, as multiple proxy properties come from it
            var fileName = $"{infos.First().ContainingClass.Replace(".", "_")}_{infos.First().FieldName}_Proxy.g.cs";
            ctx.AddSource(fileName, SourceText.From(source, Encoding.UTF8));
        });
    }

    static bool IsFieldWithAutoPropertyChanged(SyntaxNode node)
    {
        return node is FieldDeclarationSyntax field &&
               field.AttributeLists
                    .SelectMany(al => al.Attributes)
                    .Any(attr => attr.Name.ToString() == "AutoPropertyChanged" || attr.Name.ToString().EndsWith(".AutoPropertyChanged"));
    }

    static bool IsFieldWithAutoProxyPropertyChanged(SyntaxNode node)
    {
        return node is FieldDeclarationSyntax field &&
               field.AttributeLists
                    .SelectMany(al => al.Attributes)
                    .Any(attr => attr.Name.ToString() == "AutoProxyPropertyChanged" || attr.Name.ToString().EndsWith(".AutoProxyPropertyChanged"));
    }

    static FieldInfo? GetFieldInfo(GeneratorSyntaxContext context)
    {
        if (context.Node is not FieldDeclarationSyntax fieldSyntax) return null;

        var variable = fieldSyntax.Declaration.Variables.FirstOrDefault();
        if (variable is null) return null;

        IFieldSymbol? fieldSymbol = (IFieldSymbol?)context.SemanticModel.GetDeclaredSymbol(variable);
        if (fieldSymbol is null) return null;

        // Ensure it has the correct attribute (double-check needed because predicate is broad)
        if (!fieldSymbol.GetAttributes().Any(a => a.AttributeClass?.Name == "AutoPropertyChangedAttribute"))
        {
            return null;
        }

        var classSymbol = fieldSymbol.ContainingType;

        var classNames = new List<string>();
        var currentType = classSymbol;
        while (currentType != null && currentType.ContainingNamespace.ToDisplayString() != currentType.ToDisplayString())
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
        while (currentType != null && currentType.ContainingNamespace.ToDisplayString() != currentType.ToDisplayString())
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
            string? customName = null;

            // Constructor arguments
            if (attr.ConstructorArguments.Length >= 1 && attr.ConstructorArguments[0].Value is string arg1)
            {
                propertyPath = arg1;
            }
            if (attr.ConstructorArguments.Length >= 2 && attr.ConstructorArguments[1].Value is string arg2)
            {
                customName = arg2;
            }

            // Named arguments (override constructor arguments if present)
            foreach (var namedArg in attr.NamedArguments)
            {
                switch (namedArg.Key)
                {
                    case "propertyName":
                        if (namedArg.Value.Value is string namedPropName)
                            propertyPath = namedPropName;
                        break;
                    case "customName":
                        if (namedArg.Value.Value is string namedCustomName)
                            customName = namedCustomName;
                        break;
                }
            }

            propertyPath ??= string.Empty;

            var finalPropertyName = customName ?? ToPascal(propertyPath.Split('.').Last());
            var propertyType = DeterminePropertyType(fieldSymbol, propertyPath);

            result.Add(new ProxyPropertyInfo(
                PropertyName: finalPropertyName,
                ProxyPropertyPath: propertyPath,
                Type: propertyType,
                FieldName: fieldSymbol.Name,
                ContainingClass: fullClassName,
                Namespace: ns
            ));
        }

        return result;
    }

    static string DeterminePropertyType(IFieldSymbol fieldSymbol, string propertyPath)
    {
        var fieldType = fieldSymbol.Type;
        var parts = propertyPath.Split('.');

        var currentType = fieldType;
        foreach (var part in parts)
        {
            var property = FindPropertyInTypeHierarchy(currentType, part);
            if (property != null)
            {
                currentType = property.Type;
            }
            else
            {
                // Could not find property along the path, return object
                return "object";
            }
        }

        return currentType.ToDisplayString();
    }

    static IPropertySymbol? FindPropertyInTypeHierarchy(ITypeSymbol type, string propertyName)
    {
        var current = type;
        while (current != null)
        {
            // Look for the property in the current type
            var property = current.GetMembers(propertyName).OfType<IPropertySymbol>().FirstOrDefault();
            if (property != null)
            {
                return property;
            }

            // Move up the inheritance hierarchy
            current = current.BaseType;
        }

        // Also check interfaces if not found in class hierarchy
        foreach (var @interface in type.AllInterfaces)
        {
            var property = @interface.GetMembers(propertyName).OfType<IPropertySymbol>().FirstOrDefault();
            if (property != null)
            {
                return property;
            }
        }

        return null;
    }

    static string ToPascal(string name) =>
        name.TrimStart('_') is string trimmed && trimmed.Length > 0
            ? char.ToUpper(trimmed[0]) + trimmed.Substring(1)
            : name;

    static string GenerateReactiveProperty(FieldInfo field, bool hasReactiveUISupport)
    {
        // Add nullable enable directive
        var nullableDirective = "#nullable enable\n\n";

        // Only add ReactiveUI using if necessary, but it's probably always needed here
        var usingDirectives = "using ReactiveUI;\n";

        var namespaceDeclaration = string.IsNullOrEmpty(field.Namespace)
            ? ""
            : $"namespace {field.Namespace}\n{{\n";
        var namespaceClosing = string.IsNullOrEmpty(field.Namespace)
            ? ""
            : "\n}";

        List<string> classDeclarations = [];
        List<string> classClosings = [];

        var classParts = field.ContainingClass.Split('.');
        for (int i = 0; i < classParts.Length; i++)
        {
            var isLastClass = i == classParts.Length - 1;
            // Add INotifyPropertyChanged only if the class doesn't have ReactiveUI support
            var interfaceDeclaration = (isLastClass && !hasReactiveUISupport) ? " : global::System.ComponentModel.INotifyPropertyChanged" : "";
            classDeclarations.Add($"    public partial class {classParts[i]}{interfaceDeclaration}");
            classDeclarations.Add("    {");
            classClosings.Insert(0, "    }");
        }

        // Only generate INPC implementation if the class doesn't have ReactiveUI support
        var eventAndMethodDeclaration = hasReactiveUISupport ? "" : @"
        public event global::System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new global::System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }";

        var propertyImplementation = $@"    {eventAndMethodDeclaration}
        public {field.Type} {field.PropertyName}
        {{
            get => {field.FieldName};
            set => this.RaiseAndSetIfChanged(ref {field.FieldName}, value);
        }}";

        var classHierarchy = string.Join("\n", classDeclarations);
        var closingBraces = string.Join("\n", classClosings);

        return $@"{nullableDirective}{usingDirectives}{namespaceDeclaration}{classHierarchy}

{propertyImplementation}
{closingBraces}{namespaceClosing}";
    }

    static string GenerateProxyProperties(List<ProxyPropertyInfo> infos, bool hasReactiveUISupport)
    {
        // Add nullable enable directive
        var nullableDirective = "#nullable enable\n\n";

        // Always use ReactiveUI for proxy properties as they rely on RaisePropertyChanged
        var usingDirectives = "using ReactiveUI;\n";

        var firstInfo = infos.First(); // Get info from the first proxy property to determine common class details

        var namespaceDeclaration = string.IsNullOrEmpty(firstInfo.Namespace)
            ? ""
            : $"namespace {firstInfo.Namespace}\n{{\n";
        var namespaceClosing = string.IsNullOrEmpty(firstInfo.Namespace)
            ? ""
            : "\n}";

        List<string> classDeclarations = [];
        List<string> classClosings = [];

        var classParts = firstInfo.ContainingClass.Split('.');
        for (int i = 0; i < classParts.Length; i++)
        {
            var isLastClass = i == classParts.Length - 1;
            // Add INotifyPropertyChanged only if the class doesn't have ReactiveUI support
            var interfaceDeclaration = (isLastClass && !hasReactiveUISupport) ? " : global::System.ComponentModel.INotifyPropertyChanged" : "";
            classDeclarations.Add($"    public partial class {classParts[i]}{interfaceDeclaration}");
            classDeclarations.Add("    {");
            classClosings.Insert(0, "    }");
        }

        // Only generate INPC implementation if the class doesn't have ReactiveUI support
        var eventAndMethodDeclaration = hasReactiveUISupport ? "" : @"
        public event global::System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new global::System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }";

        var properties = new StringBuilder();
        foreach (var info in infos)
        {
            var propertyImplementation = $@"    /// <inheritdoc cref=""{info.FieldName}.{info.ProxyPropertyPath}""/>
        public {info.Type} {info.PropertyName}
        {{
            get => {info.FieldName}.{info.ProxyPropertyPath};
            set
            {{
                if (!global::System.Collections.Generic.EqualityComparer<{info.Type}>.Default.Equals({info.FieldName}.{info.ProxyPropertyPath}, value))
                {{
                    {info.FieldName}.{info.ProxyPropertyPath} = value;
                    this.RaisePropertyChanged(nameof({info.PropertyName})); // ReactiveUI's RaisePropertyChanged
                }}
            }}
        }}";

            properties.AppendLine(propertyImplementation);
            properties.AppendLine();
        }

        var classHierarchy = string.Join("\n", classDeclarations);
        var closingBraces = string.Join("\n", classClosings);

        // Combine the class hierarchy, optional INPC methods, and generated properties
        return $@"{nullableDirective}{usingDirectives}{namespaceDeclaration}{classHierarchy}
{eventAndMethodDeclaration}
{properties}{closingBraces}{namespaceClosing}";
    }

    // Check if class has ReactiveUI support (needed for RaisePropertyChanged)
    static bool ClassHasReactiveUISupport(INamedTypeSymbol classSymbol)
    {
        var current = classSymbol;
        while (current != null)
        {
            // Check if it inherits from ReactiveObject
            if (current.BaseType?.ToDisplayString() == "ReactiveUI.ReactiveObject")
            {
                return true;
            }

            // Check if it implements IReactiveNotifyPropertyChanged or IReactiveObject
            if (current.AllInterfaces.Any(i =>
                i.Name == "IReactiveNotifyPropertyChanged" ||
                i.Name == "IReactiveObject"))
            {
                return true;
            }

            current = current.BaseType;
        }
        return false;
    }
}