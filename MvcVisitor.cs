using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TP6.CustomTypes;

namespace TP6
{
    public class MvcVisitor : CSharpSyntaxVisitor
    {
        private readonly SyntaxTree _tree;
        private bool _hasVisited;
        private readonly List<SyntaxNode> _fieldDeclarations;
        private readonly List<MethodDeclarationSyntax> _controllerMethods;
        private readonly List<ClassDeclarationSyntax> _classes;
        private readonly List<UsingDirectiveSyntax> _dependencies;
        private readonly string[] _viewNames;

        public MvcVisitor(string filePath, MvcViewFile[] viewFiles)
        {
            _tree = Helper.ParseProgramFile(filePath);
            _fieldDeclarations = new List<SyntaxNode>();
            _controllerMethods = new List<MethodDeclarationSyntax>();
            _classes = new List<ClassDeclarationSyntax>();
            _dependencies = new List<UsingDirectiveSyntax>();
            _viewNames = ExtractViewNames(viewFiles);
        }

        public bool IsModel()
        {
            VisitAllNodes();

            if (_fieldDeclarations.Count == 0)
                return false;

            if (_controllerMethods.Count > 0)
                return false;

            return true;
        }

        public bool IsController(out List<MethodDeclarationSyntax> controllerMethods)
        {
            VisitAllNodes();
            controllerMethods = _controllerMethods;
            return _controllerMethods.Count != 0;
        }

        public List<ClassDeclarationSyntax> GetClasses()
        {
            return _classes;
        }

        public List<UsingDirectiveSyntax> GetUsingDirectives()
        {
            return _dependencies;
        }

        #region Overridden methods

        public override void VisitUsingDirective(UsingDirectiveSyntax node)
        {
            _dependencies.Add(node);
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            _classes.Add(node);
        }

        public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            _fieldDeclarations.Add(node);
        }

        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            _fieldDeclarations.Add(node);
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            if (_viewNames.Contains(node.Identifier.ToString()))
            {
                _controllerMethods.Add(node);
            }
        }

        #endregion

        #region Private methods

        private void VisitAllNodes()
        {
            if (_hasVisited)
                return;

            var root = _tree.GetRoot();
            Visit(root);
            VisitChildrenNodes(root);
            _hasVisited = true;
        }

        private void VisitChildrenNodes(SyntaxNode node)
        {
            foreach (var childNode in node.ChildNodes())
            {
                Visit(childNode);

                if (childNode.ChildNodes().Any())
                    VisitChildrenNodes(childNode);
            }
        }

        private string[] ExtractViewNames(MvcViewFile[] viewFiles)
        {
            var viewNames = new string[viewFiles.Length];

            for (int i = 0; i < viewFiles.Length; i++)
            {
                viewNames[i] = viewFiles[i].InferredName();
            }

            return viewNames;
        }

        #endregion
    }
}
