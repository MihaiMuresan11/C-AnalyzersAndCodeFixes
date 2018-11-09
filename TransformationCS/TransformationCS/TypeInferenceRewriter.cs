namespace TransformationCS
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    /// <summary>The type inference rewriter.</summary>
    public class TypeInferenceRewriter : CSharpSyntaxRewriter
    {
        /// <summary>The semantic model.</summary>
        private readonly SemanticModel SemanticModel;

        /// <summary>Initializes a new instance of the <see cref="TypeInferenceRewriter"/> class.</summary>
        /// <param name="semanticModel">The semantic model.</param>
        public TypeInferenceRewriter(SemanticModel semanticModel)
        {
            this.SemanticModel = semanticModel;
        }

        /// <summary>The visit local declaration statement.</summary>
        /// <param name="node">The node.</param>
        /// <returns>The <see cref="SyntaxNode"/>.</returns>
        public override SyntaxNode VisitLocalDeclarationStatement(
            LocalDeclarationStatementSyntax node)
        {
            if (node.Declaration.Variables.Count > 1)
            {
                return node;
            }

            if (node.Declaration.Variables[0].Initializer == null)
            {
                return node;
            }

            VariableDeclaratorSyntax declarator = node.Declaration.Variables.First();
            TypeSyntax variableTypeName = node.Declaration.Type;

            ITypeSymbol variableType =
                (ITypeSymbol)SemanticModel.GetSymbolInfo(variableTypeName)
                    .Symbol;

            TypeInfo initializerInfo =
                SemanticModel.GetTypeInfo(declarator
                    .Initializer
                    .Value);

            if (variableType == initializerInfo.Type)
            {
                TypeSyntax varTypeName =
                    IdentifierName("var")
                        .WithLeadingTrivia(
                            variableTypeName.GetLeadingTrivia())
                        .WithTrailingTrivia(
                            variableTypeName.GetTrailingTrivia());

                return node.ReplaceNode(variableTypeName, varTypeName);
            }

            return node;
        }
    }
}
