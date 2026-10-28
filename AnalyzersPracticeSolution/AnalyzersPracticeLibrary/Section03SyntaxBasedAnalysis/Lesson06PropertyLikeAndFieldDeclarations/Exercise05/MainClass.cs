/*
Lesson 06 — Property-Like and Field Declarations
Exercise 05 — Client Requirement: Audit a Public API

Client request:

Produce a diagnostic summarizing three kinds of public instance members
that violate our API policy.

Analyze class declarations only. For each class, inspect only members
declared directly inside that declaration.

A member qualifies as public only when its own declaration explicitly
contains the public modifier. Do not infer accessibility.

Exclude every member whose declaration contains the static modifier.

Count these three kinds of violations:

1. Mutable fields

   Count individual field variables when their field declaration:
   - Explicitly has the public modifier.
   - Does not have static, readonly, or const.

   Examples:
       public int first, second;             // 2 violations
       public int value = 10;                // 1 violation
       public readonly int value;            // 0 violations
       public const int Limit = 10;          // 0 violations
       public static int shared;             // 0 violations
       int hidden;                           // 0 violations

   Initializers do not affect whether a field is counted.
   Field-like events are not fields for this rule.

2. Writable properties

   Count each property once when:
   - Its declaration explicitly has public.
   - Its declaration does not have static.
   - It has a set accessor with no accessibility modifier.

   A set accessor marked private, protected, or internal is excluded.
   An init accessor does not count as a set accessor.

   Examples:
       public int Value { get; set; }             // 1 violation
       public int Value { set { } }               // 1 violation
       public int Value { get; private set; }     // 0 violations
       public int Value { get; protected set; }   // 0 violations
       public int Value { get; internal set; }    // 0 violations
       public int Value { get; init; }            // 0 violations
       public int Value => 10;                    // 0 violations
       public static int Value { get; set; }     // 0 violations

   Accessor bodies and property initializers do not affect this rule.
   Do not count indexers as properties.

3. Multi-parameter indexers

   Count each indexer once when:
   - Its declaration explicitly has public.
   - Its declaration does not have static.
   - It declares two or more parameters.

   Count parameters, not commas inside parameter types.
   Return types, accessor combinations, and body forms do not affect
   this rule.

Diagnostic contract:

- Report one diagnostic per class declaration when at least one
  violation count is greater than zero.
- Report nothing for a class with all three counts equal to zero.
- ID: APIAUDIT001
- Severity: Warning
- Highlight only the class identifier.
- Use the class identifier's ValueText in the message.
- Exact message:
  Class '{name}' exposes {fields} mutable fields, {properties} writable properties, and {indexers} multi-parameter indexers

Use that wording even when a count is 1.

Scope:

- Analyze nested classes independently.
- Never include nested-type members in an outer class's counts.
- Analyze partial declarations independently; do not merge their members.
- Do not report on structs, records, or interfaces.
- Classes nested inside those types are still analyzed.
- Methods, constructors, events, and local variables do not contribute.
- Use syntax only, without semantic information.
- Use the supplied diagnostic infrastructure.

Example:

    class Example
    {
        public int first, second;
        public readonly int SafeField;

        public int Count { get; set; }
        public int Limit { get; private set; }
        public string Name { get; init; }

        public int this[int row, int column] => 0;
        public int this[int index] => 0;

        public static int Shared { get; set; }
    }

Expected diagnostic on Example:

Class 'Example' exposes 2 mutable fields, 1 writable properties, and 1 multi-parameter indexers
*/

namespace AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson06PropertyLikeAndFieldDeclarations.Exercise05;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor _rule = new(
        id: "APIAUDIT001",
        title: "Class exposes members that violate the API policy",
        messageFormat:
            "Class '{0}' exposes {1} mutable fields, {2} writable properties, and {3} multi-parameter indexers",
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private sealed class ClassDiagnosticData
    {
        public string ClassName { get; set; } = "";
        public ViolationCounts Counts { get; set; } = null!;
    }

    private sealed class ViolationCounts
    {
        public int MutableFieldCount { get; set; }
        public int WritablePropertyCount { get; set; }
        public int MultiParameterIndexerCount { get; set; }
    }

    private static Diagnostic CreateDiagnostic(
        DiagnosticDescriptor rule,
        Location location,
        ClassDiagnosticData data)
    {
        return Diagnostic.Create(
            rule,
            location,
            data.ClassName,
            data.Counts.MutableFieldCount,
            data.Counts.WritablePropertyCount,
            data.Counts.MultiParameterIndexerCount);
    }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [_rule];
        }
    }

    private static int GetMutableFields(List<FieldDeclarationSyntax> list)
    {
        int count = 0;
        foreach (var field in list)
        {
            if (field.IsPublic == false)
            {
                continue;
            }
            if (field.IsStatic || field.IsConstant || field.IsReadOnly)
            {
                continue;
            }
            count += field.GetVariables.Count;
        }
        return count;
    }
    private static int GetWritableProperties(List<PropertyDeclarationSyntax> list)
    {
        int count = 0;
        foreach (var property in list)
        {
            if (property.IsPublic == false)
            {
                continue;
            }
            if (property.IsStatic)
            {
                continue;
            }
            if (property.HasUnrestrictedSetter)
            {
                count++;
            }
        }
        return count;
    }

    private static int GetMultiParameterIndexers(List<IndexerDeclarationSyntax> list)
    {
        int count = 0;

        foreach (var item in list)
        {
            if (item.IsPublic == false)
            {
                continue;
            }
            if (item.IsStatic)
            {
                continue;
            }
            if (item.ParameterCount >= 2)
            {
                count++;
            }
        }
        return count;
    }
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(context =>
        {
            ClassDeclarationSyntax node = context.ToClassSyntax;


            int mutableFields = GetMutableFields(node.GetAllFieldDeclarations);


            int writableProperies = GetWritableProperties(node.GetAllProperties);
            int multiparameterIndexers = GetMultiParameterIndexers(node.GetAllIndexers);




            if (mutableFields == 0 && writableProperies == 0 && multiparameterIndexers == 0)
            {
                return;
            }
            ViolationCounts counts = new()
            {
                MutableFieldCount = mutableFields,
                WritablePropertyCount = writableProperies,
                MultiParameterIndexerCount = multiparameterIndexers
            };
            ClassDiagnosticData data = new()
            {
                ClassName = node.Name,
                Counts = counts
            };
            Diagnostic diagnostic = CreateDiagnostic(_rule, node.GetIdentifierLocation, data);
            context.ReportDiagnostic(diagnostic);
        }, SyntaxKind.ClassDeclaration);
        context.EnableConcurrentExecution();

        // Implement your registration.
    }

    // Implement your analysis.
}