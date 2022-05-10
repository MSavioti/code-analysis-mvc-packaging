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
    public static class Helper
    {
        public static SyntaxTree ParseProgramFile(string filePath)
        {
            var streamReader = new StreamReader(filePath, Encoding.UTF8);
            return CSharpSyntaxTree.ParseText(streamReader.ReadToEnd());
        }

        public static string[] FindAllCshtmlFiles(string path)
        {
            return Directory.GetFiles(path, "*.cshtml", SearchOption.AllDirectories);
        }

        public static string[] FindAllCsharpFiles(string path)
        {
            return Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories);
        }

        public static void CreateMvcDirectories(string rootPath, List<MvcControllerFile> mvcControllerFiles)
        {
            var dirExists = Directory.Exists(rootPath);

            if (dirExists)
            {
                Console.WriteLine($"Diretório {rootPath} já existe, sobrescrevendo...\n");
                Directory.Delete(rootPath, true);
            }

            Console.WriteLine($"Criando diretórios no caminho de saída: {rootPath}\n");

            var directoryInfos = new List<DirectoryInfo>();
            directoryInfos.Add(Directory.CreateDirectory($"{rootPath}\\Models"));
            directoryInfos.Add(Directory.CreateDirectory($"{rootPath}\\Controllers"));
            directoryInfos.Add(Directory.CreateDirectory($"{rootPath}\\Views"));
            directoryInfos.Add(Directory.CreateDirectory($"{rootPath}\\Views\\Shared"));

            foreach (var controllerFile in mvcControllerFiles)
            {
                directoryInfos.Add(Directory.CreateDirectory($"{rootPath}\\Views\\{controllerFile.InferredName()}"));
            }

            Console.WriteLine("Diretórios criados:");

            foreach (var directoryInfo in directoryInfos)
            {
                Console.WriteLine($" - \"{directoryInfo.FullName}\"");
            }

            Console.WriteLine();
        }

        public static List<IdentifierNameSyntax> FindNamespaceIdentifiers(string filePath, string mvcRootPath, string packageName)
        {
            var namespaceIdentifiers = new List<IdentifierNameSyntax>();
            var foundRoot = false;
            var currentPath = Path.GetDirectoryName(filePath);

            while (!foundRoot && currentPath != null)
            {
                if (currentPath.Equals(mvcRootPath))
                {
                    foundRoot = true;
                }
                else
                {
                    var newIdentifier = new DirectoryInfo(currentPath).Name;
                    namespaceIdentifiers.Add(SyntaxFactory.IdentifierName(newIdentifier));
                    currentPath = Directory.GetParent(currentPath)?.FullName;
                }
            }

            namespaceIdentifiers.Add(SyntaxFactory.IdentifierName(packageName));
            namespaceIdentifiers.Reverse();

            return namespaceIdentifiers;
        }

        public static QualifiedNameSyntax IdentifiersToQualifiedName(List<IdentifierNameSyntax> identifiers)
        {
            if (identifiers.Count <= 1)
                return null;

            var qualifiedName = SyntaxFactory.QualifiedName(identifiers[0], identifiers[1]);
            SyntaxToken dotToken = SyntaxFactory.Token(SyntaxKind.DotToken);

            if (identifiers.Count == 2)
                return qualifiedName;

            for (int i = 2; i < identifiers.Count; i++)
            {
                qualifiedName = SyntaxFactory.QualifiedName(qualifiedName, dotToken, identifiers[i]);
            }

            Console.WriteLine($"qualified name: {qualifiedName}");
            return qualifiedName;
        }

        public static CompilationUnitSyntax CreatePackagedCompilationUnit(MvcCsharpFile csharpFile, NamespaceDeclarationSyntax package)
        {
            var compilationUnit = SyntaxFactory.CompilationUnit();
            compilationUnit = compilationUnit.AddUsings(csharpFile.UsingDirectives.ToArray());

            foreach (var @class in csharpFile.ClassDeclarations)
            {
                package = package.AddMembers(@class);
            }

            return compilationUnit.AddMembers(package);
        }

        public static NamespaceDeclarationSyntax CreateNamespaceFromIdentifiers(List<IdentifierNameSyntax> identifiers)
        {
            var qualifiedName = IdentifiersToQualifiedName(identifiers);

            return qualifiedName == null ?
                SyntaxFactory.NamespaceDeclaration(identifiers[0]) :
                SyntaxFactory.NamespaceDeclaration(qualifiedName);
        }
    }
}
