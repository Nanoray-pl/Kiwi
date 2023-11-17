using System.Globalization;
using Nanoray.Kiwi;
using NUnit.Framework;

namespace Kiwi.Tests;

public sealed class RealWorldTests
{
    private const double Epsilon = 1.0e-8;

    private const string Left = "left";
    private const string Right = "right";
    private const string Top = "top";
    private const string Bottom = "bottom";
    private const string Height = "height";
    private const string Width = "width";

    private static readonly string[] Constraints = new[]
    {
        "container.columnWidth == container.width * 0.4",
        "container.thumbHeight == container.columnWidth / 2",
        "container.padding == container.width * (0.2 / 3)",
        "container.leftPadding == container.padding",
        "container.rightPadding == container.width - container.padding",
        "container.paddingUnderThumb == 5",
        "container.rowPadding == 15",
        "container.buttonPadding == 20",

        "thumb0.left == container.leftPadding",
        "thumb0.top == container.padding",
        "thumb0.height == container.thumbHeight",
        "thumb0.width == container.columnWidth",

        "title0.left == container.leftPadding",
        "title0.top == thumb0.bottom + container.paddingUnderThumb",
        "title0.height == title0.intrinsicHeight",
        "title0.width == container.columnWidth",

        "thumb1.right == container.rightPadding",
        "thumb1.top == container.padding",
        "thumb1.height == container.thumbHeight",
        "thumb1.width == container.columnWidth",

        "title1.right == container.rightPadding",
        "title1.top == thumb0.bottom + container.paddingUnderThumb",
        "title1.height == title1.intrinsicHeight",
        "title1.width == container.columnWidth",

        "thumb2.left == container.leftPadding",
        "thumb2.top >= title0.bottom + container.rowPadding",
        "thumb2.top == title0.bottom + container.rowPadding !weak",
        "thumb2.top >= title1.bottom + container.rowPadding",
        "thumb2.top == title1.bottom + container.rowPadding !weak",
        "thumb2.height == container.thumbHeight",
        "thumb2.width == container.columnWidth",

        "title2.left == container.leftPadding",
        "title2.top == thumb2.bottom + container.paddingUnderThumb",
        "title2.height == title2.intrinsicHeight",
        "title2.width == container.columnWidth",

        "thumb3.right == container.rightPadding",
        "thumb3.top == thumb2.top",

        "thumb3.height == container.thumbHeight",
        "thumb3.width == container.columnWidth",

        "title3.right == container.rightPadding",
        "title3.top == thumb3.bottom + container.paddingUnderThumb",
        "title3.height == title3.intrinsicHeight",
        "title3.width == container.columnWidth",

        "thumb4.left == container.leftPadding",
        "thumb4.top >= title2.bottom + container.rowPadding",
        "thumb4.top >= title3.bottom + container.rowPadding",
        "thumb4.top == title2.bottom + container.rowPadding !weak",
        "thumb4.top == title3.bottom + container.rowPadding !weak",
        "thumb4.height == container.thumbHeight",
        "thumb4.width == container.columnWidth",

        "title4.left == container.leftPadding",
        "title4.top == thumb4.bottom + container.paddingUnderThumb",
        "title4.height == title4.intrinsicHeight",
        "title4.width == container.columnWidth",

        "thumb5.right == container.rightPadding",
        "thumb5.top == thumb4.top",
        "thumb5.height == container.thumbHeight",
        "thumb5.width == container.columnWidth",

        "title5.right == container.rightPadding",
        "title5.top == thumb5.bottom + container.paddingUnderThumb",
        "title5.height == title5.intrinsicHeight",
        "title5.width == container.columnWidth",

        "line.height == 1",
        "line.width == container.width",
        "line.top >= title4.bottom + container.rowPadding",
        "line.top >= title5.bottom + container.rowPadding",

        "more.top == line.bottom + container.buttonPadding",
        "more.height == more.intrinsicHeight",
        "more.left == container.leftPadding",
        "more.right == container.rightPadding",

        "container.height == more.bottom + container.buttonPadding",

        "container.width == 300",
        "title0.intrinsicHeight == 100",
        "title1.intrinsicHeight == 110",
        "title2.intrinsicHeight == 120",
        "title3.intrinsicHeight == 130",
        "title4.intrinsicHeight == 140",
        "title5.intrinsicHeight == 150",
        "more.intrinsicHeight == 160"
    };

    private sealed class TestVariableResolver : ConstraintParser.ICassowaryVariableResolver
    {
        private SolverTransaction Transaction { get; init; }
        private Dictionary<string, Dictionary<string, IVariable>> Nodes { get; init; }

        public TestVariableResolver(SolverTransaction transaction, Dictionary<string, Dictionary<string, IVariable>> nodes)
        {
            this.Transaction = transaction;
            this.Nodes = nodes;
        }

        public IVariable? ResolveVariable(string variableName)
        {
            string[] stringArray = variableName.Split(".");
            if (stringArray.Length != 2)
                throw new ArgumentException("Can't resolve variable");

            var node = ObtainNode(stringArray[0]);
            return ObtainVariableFromNode(node, stringArray[1]);
        }

        public Expression? ResolveConstant(string name)
            => double.TryParse(name, NumberStyles.Number, CultureInfo.InvariantCulture, out double result) ? new(result) : null;

        private Dictionary<string, IVariable> ObtainNode(string nodeName)
        {
            if (!this.Nodes.TryGetValue(nodeName, out var node))
            {
                node = new();
                this.Nodes[nodeName] = node;
            }
            return node;
        }

        private IVariable ObtainVariableFromNode(Dictionary<string, IVariable> node, string variableName)
        {
            if (node.TryGetValue(variableName, out var variable))
                return variable;

            variable = new Variable(variableName);
            node[variableName] = variable;

            switch (variableName)
            {
                case Right:
                    this.Transaction.AddConstraint(Constraint.Make(variable, RelationalOperator.Equal, ObtainVariableFromNode(node, Left).Add(ObtainVariableFromNode(node, Width))));
                    break;
                case Bottom:
                    this.Transaction.AddConstraint(Constraint.Make(variable, RelationalOperator.Equal, ObtainVariableFromNode(node, Top).Add(ObtainVariableFromNode(node, Height))));
                    break;
            }

            return variable;
        }
    }

    [Test]
    public void TestGridLayout()
    {
        Solver solver = new();
        Dictionary<string, Dictionary<string, IVariable>> nodes = new();

        solver.WithTransaction(transaction =>
        {
            var variableResolver = new TestVariableResolver(transaction, nodes);

            foreach (string constraintString in Constraints)
            {
                var constraint = ConstraintParser.ParseConstraint(constraintString, variableResolver);
                transaction.AddConstraint(constraint);
            }
        });

        //Assert.AreEqual(20.0, nodes["thumb0"][Top].Value, Epsilon);
        //Assert.AreEqual(20.0, nodes["thumb1"][Top].Value, Epsilon);

        //Assert.AreEqual(85.0, nodes["title0"][Top].Value, Epsilon);
        //Assert.AreEqual(85.0, nodes["title1"][Top].Value, Epsilon);

        //Assert.AreEqual(210.0, nodes["thumb2"][Top].Value, Epsilon);
        //Assert.AreEqual(210.0, nodes["thumb3"][Top].Value, Epsilon);

        //Assert.AreEqual(275.0, nodes["title2"][Top].Value, Epsilon);
        //Assert.AreEqual(275.0, nodes["title3"][Top].Value, Epsilon);

        //Assert.AreEqual(420.0, nodes["thumb4"][Top].Value, Epsilon);
        //Assert.AreEqual(420.0, nodes["thumb5"][Top].Value, Epsilon);

        //Assert.AreEqual(485.0, nodes["title4"][Top].Value, Epsilon);
        //Assert.AreEqual(485.0, nodes["title5"][Top].Value, Epsilon);
    }
}
