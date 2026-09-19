/*
Enter the requirements for this exercise here.
Client request

Our team wants class-like declarations to follow a consistent member order.

For every class, struct, interface, record class, and record struct, direct members must appear in this order:

1. Fields
2. Properties
3. Constructors
4. Methods
5. Nested class-like types

A category may be absent. Members within the same category may appear in any order.

For example, this is valid:

public class Game
{
    private int _score;
    private bool _finished;

    public int Score { get; set; }

    public Game()
    {
    }

    public void Start()
    {
    }

    public void Stop()
    {
    }

    public class State
    {
    }
}

This is invalid:

public class Game
{
    public void Start()
    {
    }

    private int _score;
}

The field appears after a method.

What counts as a nested type

For this exercise, these are all nested class-like types:

class
struct
interface
record class
record struct

Do not recursively inspect descendants to determine the containing declaration's ordering. Only its direct Members collection determines whether that declaration is ordered correctly.

Nested declarations are independently analyzed by the analyzer.

Interfaces

The same ordering rules apply to interfaces for the member categories that are legal in the supplied test source.

For example:

public interface IGame
{
    int Score { get; }

    void Start();
}

is correctly ordered.

Unsupported member kinds

There are other MemberDeclarationSyntax forms that we have not learned yet.

The unit tests for this exercise will therefore only use:

fields
properties
constructors
methods
nested class-like declarations

You do not need to account for events, operators, indexers, delegates, destructors, or other member forms yet.

Reporting rule

Report one diagnostic per incorrectly ordered declaration.

Do not report a diagnostic for a correctly ordered declaration.

If several members are out of order within the same declaration, still report only one diagnostic.

For example:

public class BadExample
{
    public void Run()
    {
    }

    public int Score { get; set; }

    private int _value;
}

has multiple ordering problems but receives only one diagnostic.

Location

Report the diagnostic on the identifier of the class-like declaration, not on the individual member that caused the ordering violation.

Diagnostic contract

Use:

ID: RPA030405
Title: Members are out of order
Message: {0} '{1}' contains members that are not in the required order.
Category: Syntax Analysis
Severity: Warning
Enabled by default: true

The two message arguments are:

{0} = declaration kind
{1} = declaration name

Use your existing declaration-kind values:

Class
Struct
Interface
Record Class
Record Struct

*/

using System.Data;

namespace AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson04ClassLikeDeclarationsAndMembers.Exercise05;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor _rule = new(
    id: "RPA030405",
    title: "Members are out of order",
    messageFormat: "{0} '{1}' contains members that are not in the required order",
    category: "Syntax Analysis",
    defaultSeverity: DiagnosticSeverity.Warning,
    isEnabledByDefault: true);

    private sealed class MemberOrderingDiagnosticData
    {
        public string DeclarationKind { get; set; } = "";
        public string DeclarationName { get; set; } = "";
    }

    private static Diagnostic CreateDiagnostic(
        DiagnosticDescriptor rule,
        Location location,
        MemberOrderingDiagnosticData data)
    {
        return Diagnostic.Create(
            rule,
            location,
            data.DeclarationKind,
            data.DeclarationName);
    }
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [_rule];
        }
    }
    private List<Type> _common =
        [
        typeof(FieldDeclarationSyntax),
        typeof(PropertyDeclarationSyntax),
        typeof(ConstructorDeclarationSyntax),
        typeof(MethodDeclarationSyntax),
        typeof(TypeDeclarationSyntax)
        ];
    private bool IsCommon(SyntaxNode x)
    {
        if (x is TypeDeclarationSyntax)
        {
            return true;
        }
        return _common.Contains(x.GetType());
    }
    private List<MemberDeclarationSyntax> GetCommonMembers(TypeDeclarationSyntax node)
    {
        var output = node.Members.Where(IsCommon).ToList();
        return output;
    }
    private int GetOrder(MemberDeclarationSyntax node)
    {
        Type categoryType;
        if (node is TypeDeclarationSyntax)
        {
            categoryType = typeof(TypeDeclarationSyntax);
        }
        else
        {
            categoryType = node.GetType();
        }
        return _common.IndexOf(categoryType);
    }


    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(context =>
        {

            var node = context.ToTypeDeclaration;
            string name = node.Name;
            Location location = node.GetIdentifierLocation;
            var list = GetCommonMembers(node);
            if (list.IsOrderedBy(GetOrder) == false)
            {
                MemberOrderingDiagnosticData data = new()
                {
                    DeclarationKind = node.KindName,
                    DeclarationName = node.Name
                };
                Diagnostic d = CreateDiagnostic(_rule, location, data);
                context.ReportDiagnostic(d);
            }

            

        }, SyntaxKind.StructDeclaration,
        SyntaxKind.RecordDeclaration,
        SyntaxKind.RecordStructDeclaration,
        SyntaxKind.ClassDeclaration,
        SyntaxKind.InterfaceDeclaration
        );
        context.EnableConcurrentExecution();
    }
}