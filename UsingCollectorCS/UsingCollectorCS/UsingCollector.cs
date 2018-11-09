namespace UsingCollectorCS
{
    using System.Collections.Generic;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>The using collector.</summary>
    public class UsingCollector : CSharpSyntaxWalker
    {
        /// <summary>The usings.</summary>
        public readonly List<UsingDirectiveSyntax> Usings = new List<UsingDirectiveSyntax>();

        /// <summary>The visit using directive.</summary>
        /// <param name="node">The node.</param>
        public override void VisitUsingDirective(UsingDirectiveSyntax node)
        {
            if (node.Name.ToString() != "System" &&
                !node.Name.ToString().StartsWith("System."))
            {
                this.Usings.Add(node);
            }
        }

    }
}
