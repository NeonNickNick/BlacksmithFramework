using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ClapSourceGenerators.SkillRegistration.BlacksmithOnly
{
    [Generator]
    public class ModifierBindingGenerator : IIncrementalGenerator
    {
        private static readonly SymbolDisplayFormat FullTypeFormat = SymbolDisplayFormat.FullyQualifiedFormat;

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // ── Phase 1: MainProfession 子类 → 生成 [BindingContract] ──
            // 编译进 DLL，供 Phase 2 跨项目读取
            var mainInSource = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: (node, _) => node is ClassDeclarationSyntax,
                    transform: (ctx, _) => GetMainInfo(ctx))
                .Where(info => info is not null)
                .Select((info, _) => info!);

            context.RegisterSourceOutput(mainInSource.Collect(), GenerateContracts);

            // ── Phase 2: ProfessionModifier → 发现 target 私有成员 → 生成 Bind() ──
            var modifierInSource = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: (node, _) => node is ClassDeclarationSyntax,
                    transform: (ctx, _) => GetModifierInfo(ctx))
                .Where(info => info is not null)
                .Select((info, _) => info!);

            var combined = modifierInSource.Collect().Combine(context.CompilationProvider);
            context.RegisterSourceOutput(combined, GenerateBinding);
        }

        // ════════════════════════════════════════════════════════════════
        //  数据模型
        // ════════════════════════════════════════════════════════════════

        private sealed class MainInfo
        {
            public string ClassName, Namespace, FullTypeName;
            public ImmutableArray<Member> Fields, Properties;
            public MainInfo(string cn, string ns, string ft, ImmutableArray<Member> f, ImmutableArray<Member> p)
            { ClassName = cn; Namespace = ns; FullTypeName = ft; Fields = f; Properties = p; }
        }

        private sealed class ModifierInfo
        {
            public string ClassName, Namespace, TargetName;
            public ModifierInfo(string c, string ns, string t) { ClassName = c; Namespace = ns; TargetName = t; }
        }

        private sealed class Member
        {
            public string Name, TypeFull;
            /// <summary>
            /// For fields: IsReadonly (true → ref readonly, false → ref).
            /// For properties: HasSetter (true → generate Action setter delegate).
            /// </summary>
            public bool Flag;
            public Member(string n, string t, bool f) { Name = n; TypeFull = t; Flag = f; }
        }

        // ════════════════════════════════════════════════════════════════
        //  Phase 1: 收集 MainProfession 私有成员 → 生成 [BindingContract]
        //
        //  字段：ref 属性（零拷贝共享，readonly 字段用 ref readonly）
        //  属性：Func/Action 委托捕获（属性本质是方法，通过 get_/set_ 访问）
        //  过滤：编译器生成的 backing field（名称含 '<'）
        // ════════════════════════════════════════════════════════════════

        private static MainInfo? GetMainInfo(GeneratorSyntaxContext ctx)
        {
            var cds = (ClassDeclarationSyntax)ctx.Node;
            var sym = ctx.SemanticModel.GetDeclaredSymbol(cds) as INamedTypeSymbol;
            if (sym is not { IsAbstract: false }) return null;

            var bt = sym.BaseType;
            if (bt == null || bt.TypeArguments.Length != 0
                || bt.ContainingNamespace.ToDisplayString() != "BlacksmithCore.Infra.Profession"
                || bt.Name != "MainProfession")
                return null;

            var fields = ImmutableArray.CreateBuilder<Member>();
            var props = ImmutableArray.CreateBuilder<Member>();

            foreach (var f in sym.GetMembers().OfType<IFieldSymbol>())
                if (f is { DeclaredAccessibility: Accessibility.Private, IsStatic: false } && !f.Name.Contains('<'))
                    fields.Add(new Member(f.Name, f.Type.ToDisplayString(FullTypeFormat), f.IsReadOnly));

            foreach (var p in sym.GetMembers().OfType<IPropertySymbol>())
                if (p is { DeclaredAccessibility: Accessibility.Private, IsStatic: false } && p.GetMethod != null)
                    props.Add(new Member(p.Name, p.Type.ToDisplayString(FullTypeFormat), p.SetMethod != null));

            if (fields.Count == 0 && props.Count == 0) return null;

            return new MainInfo(sym.Name, sym.ContainingNamespace.ToDisplayString(),
                sym.ToDisplayString(FullTypeFormat), fields.ToImmutable(), props.ToImmutable());
        }

        private static void GenerateContracts(SourceProductionContext ctx, ImmutableArray<MainInfo> mains)
        {
            //顺便生成外部可调用的统一构造函数入口
            foreach (var m in mains)
            {
                var sb = new StringBuilder();
                sb.AppendLine("// <auto-generated/>");
                sb.AppendLine("using BlacksmithCore.Infra.Attributes.Profession;");
                sb.AppendLine($"namespace {m.Namespace};");
                sb.AppendLine();
                foreach (var f in m.Fields)
                    sb.AppendLine($"[BindingContract(\"{f.Name}\", \"{f.TypeFull}\", true, {(f.Flag ? "true" : "false")})]");
                foreach (var p in m.Properties)
                    sb.AppendLine($"[BindingContract(\"{p.Name}\", \"{p.TypeFull}\", false, {(p.Flag ? "true" : "false")})]");

                sb.AppendLine($"partial class {m.ClassName}");
                sb.AppendLine("{");
                sb.AppendLine("}");

                ctx.AddSource($"{m.Namespace}.{m.ClassName}.Contract.g.cs", sb.ToString());
            }
        }

        // ════════════════════════════════════════════════════════════════
        //  Phase 2: 收集 ProfessionModifier 信息
        // ════════════════════════════════════════════════════════════════

        private static ModifierInfo? GetModifierInfo(GeneratorSyntaxContext ctx)
        {
            var cds = (ClassDeclarationSyntax)ctx.Node;
            var sym = ctx.SemanticModel.GetDeclaredSymbol(cds) as INamedTypeSymbol;
            if (sym is not { IsAbstract: false }) return null;

            var bt = sym.BaseType;
            if (bt == null || bt.TypeArguments.Length != 0
                || bt.ContainingNamespace.ToDisplayString() != "BlacksmithCore.Infra.Profession"
                || bt.Name != "ProfessionModifier")
                return null;

            var attr = sym.GetAttributes().FirstOrDefault(a =>
                a.AttributeClass?.Name == "IsProfessionModifier"
                || a.AttributeClass?.ToDisplayString() == "BlacksmithCore.Infra.Attributes.Profession.IsProfessionModifier");
            if (attr == null) return null;

            var target = attr.ConstructorArguments.FirstOrDefault().Value?.ToString();
            if (target is null or "") return null;

            return new ModifierInfo(sym.Name, sym.ContainingNamespace.ToDisplayString(), target);
        }

        // ════════════════════════════════════════════════════════════════
        //  Phase 2: 生成 Bind()
        //
        //  双渠道收集（seen 去重，渠道A优先）：
        //    渠道A — target.GetMembers()：同项目源码类型
        //    渠道B — [BindingContract] attribute：跨项目 DLL 元数据
        //
        //  字段 → ref / ref readonly 属性（存储 _target，零拷贝）
        //  属性 → Func< T > / Action< T > 委托（捕获 getter/setter 方法）
        // ════════════════════════════════════════════════════════════════

        private static void GenerateBinding(
            SourceProductionContext ctx,
            (ImmutableArray<ModifierInfo> Modifiers, Compilation Compilation) data)
        {
            var comp = data.Compilation;

            var mainBase = comp.GetTypeByMetadataName("BlacksmithCore.Infra.Profession.MainProfession");
            if (mainBase == null) return;

            var contractAttr = comp.GetTypeByMetadataName("BlacksmithCore.Infra.Attributes.Profession.BindingContractAttribute");

            foreach (var mod in data.Modifiers)
            {
                var target = FindMainByName(comp, mainBase, mod.TargetName);
                if (target == null) continue;
                var targetFull = target.ToDisplayString(FullTypeFormat);

                var fields = new List<Member>();
                var props = new List<Member>();
                var seen = new HashSet<string>();

                // 渠道A: GetMembers() — 同项目源码类型
                foreach (var f in target.GetMembers().OfType<IFieldSymbol>())
                {
                    if (f is not { DeclaredAccessibility: Accessibility.Private, IsStatic: false } || f.Name.Contains('<')) continue;
                    if (seen.Add(f.Name))
                        fields.Add(new Member(f.Name, f.Type.ToDisplayString(FullTypeFormat), f.IsReadOnly));
                }
                foreach (var p in target.GetMembers().OfType<IPropertySymbol>())
                {
                    if (p is not { DeclaredAccessibility: Accessibility.Private, IsStatic: false, GetMethod: not null }) continue;
                    if (seen.Add(p.Name))
                        props.Add(new Member(p.Name, p.Type.ToDisplayString(FullTypeFormat), p.SetMethod != null));
                }

                // 渠道B: [BindingContract] — 跨项目元数据类型
                if (contractAttr != null)
                {
                    foreach (var a in target.GetAttributes())
                    {
                        if (!SymbolEqualityComparer.Default.Equals(a.AttributeClass, contractAttr)) continue;
                        var args = a.ConstructorArguments;
                        if (args.Length < 4) continue;
                        var n = args[0].Value?.ToString();
                        var t = args[1].Value?.ToString();
                        var isF = args[2].Value as bool?;
                        var fl = args[3].Value as bool?;
                        if (n == null || t == null || isF == null || fl == null) continue;
                        if (!seen.Add(n)) continue;
                        if (isF.Value) fields.Add(new Member(n, t, fl.Value));
                        else props.Add(new Member(n, t, fl.Value));
                    }
                }

                var hasMembers = fields.Count > 0 || props.Count > 0;

                var sb = new StringBuilder();
                sb.AppendLine("// <auto-generated/>");
                sb.AppendLine("#nullable enable");
                sb.AppendLine();
                sb.AppendLine("using System;");
                if (fields.Count > 0)
                    sb.AppendLine("using System.Runtime.CompilerServices;");
                sb.AppendLine("using BlacksmithCore.Infra.Profession;");
                sb.AppendLine($"namespace {mod.Namespace};");
                sb.AppendLine();
                sb.AppendLine($"partial class {mod.ClassName}");
                sb.AppendLine("{");

                // ── UnsafeAccessor extern：字段 ──
                foreach (var f in fields)
                {
                    sb.AppendLine($"    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = \"{f.Name}\")]");
                    sb.AppendLine($"    private static extern ref {f.TypeFull} __ref_{f.Name}({targetFull} __t);");
                }

                // ── UnsafeAccessor extern：属性 ──
                foreach (var p in props)
                {
                    sb.AppendLine($"    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = \"get_{p.Name}\")]");
                    sb.AppendLine($"    private static extern {p.TypeFull} __get_{p.Name}({targetFull} __t);");
                    if (p.Flag)
                    {
                        sb.AppendLine($"    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = \"set_{p.Name}\")]");
                        sb.AppendLine($"    private static extern void __set_{p.Name}({targetFull} __t, {p.TypeFull} value);");
                    }
                }

                if (hasMembers)
                    sb.AppendLine();

                // ── target 存储 ──
                sb.AppendLine($"    private {targetFull} _target = null!;");

                // ── 字段公开：ref 属性（零拷贝共享）──
                // readonly 字段 → ref readonly，不可写
                // 非 readonly 字段 → ref，完全共享
                foreach (var f in fields)
                {
                    if (f.Flag)
                        sb.AppendLine($"    public ref readonly {f.TypeFull} {f.Name} => ref __ref_{f.Name}(_target);");
                    else
                        sb.AppendLine($"    public ref {f.TypeFull} {f.Name} => ref __ref_{f.Name}(_target);");
                }

                // ── 属性公开：Func / Action 委托（方法捕获）──
                foreach (var p in props)
                {
                    sb.AppendLine($"    public Func<{p.TypeFull}> {p.Name} = null!;");
                    if (p.Flag)
                        sb.AppendLine($"    public Action<{p.TypeFull}> Set{p.Name} = null!;");
                }

                // ── Bind() ──
                sb.AppendLine();
                sb.AppendLine("    public override void Bind(MainProfession package)");
                sb.AppendLine("    {");
                sb.AppendLine($"        _target = ({targetFull})package;");
                foreach (var p in props)
                {
                    sb.AppendLine($"        {p.Name} = () => __get_{p.Name}(_target);");
                    if (p.Flag)
                        sb.AppendLine($"        Set{p.Name} = v => __set_{p.Name}(_target, v);");
                }
                sb.AppendLine("    }");
                sb.AppendLine("}");

                ctx.AddSource($"{mod.Namespace}.{mod.ClassName}.Binding.g.cs", sb.ToString());
            }
        }

        // ════════════════════════════════════════════════════════════════
        //  在 Compilation 中按名字查找 MainProfession 子类
        // ════════════════════════════════════════════════════════════════

        private static INamedTypeSymbol? FindMainByName(Compilation comp, INamedTypeSymbol baseType, string name)
        {
            var visited = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
            return Walk(comp.GlobalNamespace, baseType, name, visited);
        }

        private static INamedTypeSymbol? Walk(INamespaceOrTypeSymbol c, INamedTypeSymbol baseType, string name, HashSet<ISymbol> v)
        {
            if (!v.Add(c)) return null;
            foreach (var m in c.GetMembers())
            {
                if (m is INamedTypeSymbol t)
                {
                    if (t is { IsAbstract: false } && t.Name == name && t.BaseType != null
                        && SymbolEqualityComparer.Default.Equals(t.BaseType, baseType))
                        return t;
                    var r = Walk(t, baseType, name, v);
                    if (r != null) return r;
                }
                else if (m is INamespaceSymbol ns)
                {
                    var r = Walk(ns, baseType, name, v);
                    if (r != null) return r;
                }
            }
            return null;
        }
    }
}
