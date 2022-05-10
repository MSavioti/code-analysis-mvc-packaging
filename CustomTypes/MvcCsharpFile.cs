using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TP6.CustomTypes
{
    public abstract class MvcCsharpFile : MvcFile
    {
        public List<ClassDeclarationSyntax> ClassDeclarations { get; set; }
        public List<UsingDirectiveSyntax> UsingDirectives { get; set; }
        public CompilationUnitSyntax CompilationUnit { get; set; }
        public string QualifiedName { get; set; }

        protected MvcCsharpFile(string fileInputPath) : base(fileInputPath)
        {
            ClassDeclarations = new List<ClassDeclarationSyntax>();
        }
    }
}
