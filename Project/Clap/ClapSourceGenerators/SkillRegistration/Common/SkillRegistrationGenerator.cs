using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ClapSourceGenerators.SkillRegistration.Common
{
    [Generator]
    public class SkillRegistrationGenerator : IIncrementalGenerator
    {
        private static readonly DiagnosticDescriptor WrongReturnTypeRule = new(
            id: "CLAP001",
            title: "Skill generator must return IDSLSourceFile",
            messageFormat: "'{0}' appears to be the generator for '{0}Check', but returns '{1}' instead of '{2}'. Change the return type to '{2}'.",
            category: "ClapSkillRegistration",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor MissingGeneratorRule = new(
            id: "CLAP002",
            title: "Skill checker has no matching generator",
            messageFormat: "'{0}' has no matching generator method '{1}'. Add a private '{1}' method returning '{2}' with a single '{3}' parameter.",
            category: "ClapSkillRegistration",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var provider = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: (node, _) => node is ClassDeclarationSyntax,
                    transform: (ctx, _) => GetPackageInfo(ctx))
                .Where(info => info is not null)
                .Collect();

            context.RegisterSourceOutput(provider, GenerateSource);
        }

        private static PackageInfo? GetPackageInfo(GeneratorSyntaxContext ctx)
        {
            var classDecl = (ClassDeclarationSyntax)ctx.Node;
            var symbol = ctx.SemanticModel.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;
            if (symbol is not { IsAbstract: false })
                return null;

            var baseType = FindClapSkillPackageBase(symbol);
            if (baseType is not { TypeArguments.Length: 2 })
                return null;

            var skillContextType = baseType.TypeArguments[0];
            var dslSourceFileType = baseType.TypeArguments[1];

            var methods = symbol.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(m => m is { DeclaredAccessibility: Accessibility.Private, IsStatic: false })
                .ToList();

            var skills = new List<SkillMethodInfo>();
            var diagnostics = new List<DiagnosticInfo>();

            foreach (var method in methods)
            {
                if (!method.Name.EndsWith("Check", StringComparison.Ordinal))
                    continue;
                if (method.ReturnType.SpecialType != SpecialType.System_Boolean)
                    continue;
                if (method.Parameters.Length != 1)
                    continue;
                if (!SymbolEqualityComparer.Default.Equals(
                        method.Parameters[0].Type, skillContextType))
                    continue;

                var skillName = method.Name.Substring(0, method.Name.Length - "Check".Length);

                var generatorMethod = methods.FirstOrDefault(m =>
                    m.Name == skillName &&
                    m.Parameters.Length == 1 &&
                    SymbolEqualityComparer.Default.Equals(
                        m.Parameters[0].Type, skillContextType) &&
                    SymbolEqualityComparer.Default.Equals(
                        m.ReturnType, dslSourceFileType));

                if (generatorMethod != null)
                {
                    skills.Add(new SkillMethodInfo(
                        skillName.ToLowerInvariant(),
                        method.Name,
                        generatorMethod.Name));
                    continue;
                }

                // Look for a near-miss: right name and parameters, wrong return type
                var nearMiss = methods.FirstOrDefault(m =>
                    m.Name == skillName &&
                    m.Parameters.Length == 1 &&
                    SymbolEqualityComparer.Default.Equals(
                        m.Parameters[0].Type, skillContextType));

                if (nearMiss != null)
                {
                    diagnostics.Add(new DiagnosticInfo(
                        DiagnosticKind.WrongReturnType,
                        nearMiss.Locations.FirstOrDefault(),
                        skillName,
                        nearMiss.ReturnType.ToDisplayString(),
                        dslSourceFileType.ToDisplayString()));
                }
                else
                {
                    diagnostics.Add(new DiagnosticInfo(
                        DiagnosticKind.MissingGenerator,
                        method.Locations.FirstOrDefault(),
                        method.Name,
                        skillName,
                        dslSourceFileType.ToDisplayString(),
                        skillContextType.ToDisplayString()));
                }
            }

            if (skills.Count == 0 && diagnostics.Count == 0)
                return null;

            var filePath = classDecl.SyntaxTree.FilePath;

            return new PackageInfo(
                symbol.Name,
                symbol.ContainingNamespace.ToDisplayString(),
                filePath,
                skills,
                diagnostics);
        }

        private static INamedTypeSymbol? FindClapSkillPackageBase(INamedTypeSymbol type)
        {
            var current = type.BaseType;
            while (current != null)
            {
                if (current.Name == "ClapSkillPackage" &&
                    current.TypeArguments.Length == 2 &&
                    current.ContainingNamespace.ToDisplayString() == "ClapInfra.ClapProfession")
                    return current;
                current = current.BaseType;
            }
            return null;
        }

        private static void GenerateSource(SourceProductionContext ctx, ImmutableArray<PackageInfo?> infos)
        {
            foreach (var info in infos)
            {
                if (info == null)
                    continue;

                foreach (var diag in info.Diagnostics)
                {
                    var descriptor = diag.Kind == DiagnosticKind.WrongReturnType
                        ? WrongReturnTypeRule
                        : MissingGeneratorRule;

                    var location = diag.Location
                        ?? Location.Create(info.FilePath, new Microsoft.CodeAnalysis.Text.TextSpan(0, 0), new Microsoft.CodeAnalysis.Text.LinePositionSpan(
                            new Microsoft.CodeAnalysis.Text.LinePosition(0, 0),
                            new Microsoft.CodeAnalysis.Text.LinePosition(0, 0)));

                    ctx.ReportDiagnostic(Diagnostic.Create(
                        descriptor,
                        location,
                        diag.Args));
                }

                if (info.Skills.Count > 0)
                {
                    ctx.AddSource(
                        $"{info.Namespace}.{info.ClassName}.SkillRegistration.g.cs",
                        GeneratePartialClass(info));
                }
            }
        }

        private static string GeneratePartialClass(PackageInfo info)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated/>");
            sb.AppendLine("#nullable enable");
            sb.AppendLine();
            sb.AppendLine($"namespace {info.Namespace};");
            sb.AppendLine();
            sb.AppendLine($"partial class {info.ClassName}");
            sb.AppendLine("{");
            sb.AppendLine("    protected override void RegistSkills()");
            sb.AppendLine("    {");
            foreach (var skill in info.Skills)
            {
                sb.AppendLine($"        RegistSkill(");
                sb.AppendLine($"            \"{skill.SkillName}\",");
                sb.AppendLine($"            {skill.CheckMethod},");
                sb.AppendLine($"            {skill.GeneratorMethod});");
            }
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }
    }

    internal enum DiagnosticKind
    {
        WrongReturnType,
        MissingGenerator
    }

    internal sealed class DiagnosticInfo
    {
        public DiagnosticKind Kind { get; }
        public Location? Location { get; }
        public string[] Args { get; }

        public DiagnosticInfo(DiagnosticKind kind, Location? location, params string[] args)
        {
            Kind = kind;
            Location = location;
            Args = args;
        }
    }

    internal sealed class PackageInfo
    {
        public string ClassName { get; }
        public string Namespace { get; }
        public string FilePath { get; }
        public List<SkillMethodInfo> Skills { get; }
        public List<DiagnosticInfo> Diagnostics { get; }

        public PackageInfo(string className, string @namespace, string filePath, List<SkillMethodInfo> skills, List<DiagnosticInfo> diagnostics)
        {
            ClassName = className;
            Namespace = @namespace;
            FilePath = filePath;
            Skills = skills;
            Diagnostics = diagnostics;
        }
    }

    internal sealed class SkillMethodInfo
    {
        public string SkillName { get; }
        public string CheckMethod { get; }
        public string GeneratorMethod { get; }

        public SkillMethodInfo(string skillName, string checkMethod, string generatorMethod)
        {
            SkillName = skillName;
            CheckMethod = checkMethod;
            GeneratorMethod = generatorMethod;
        }
    }
}