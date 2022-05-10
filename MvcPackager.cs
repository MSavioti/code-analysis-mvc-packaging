using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TP6.CustomTypes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.Options;

namespace TP6
{
    public class MvcPackager
    {
        private readonly string _mvcPath;
        private readonly string _packageName;
        private Workspace _currentWorkspace;
        private readonly string[] _csharpFiles;
        private readonly List<MvcViewFile> _mvcViewFiles;
        private readonly List<MvcModelFile> _mvcModelFiles;
        private readonly List<MvcControllerFile> _mvcControllerFiles;
        private readonly List<CompilationUnitSyntax> _packagedClasses;
        private readonly string _outputPath;
        private const string OutputSuffix = "-REMODULARIZADO";

        public MvcPackager(string mvcPath, string packageName, Workspace workspace)
        {
            _mvcPath = mvcPath;
            _packageName = packageName;
            _currentWorkspace = workspace;
            _csharpFiles = Helper.FindAllCsharpFiles(_mvcPath);
            _mvcViewFiles = new List<MvcViewFile>();
            _mvcModelFiles = new List<MvcModelFile>();
            _mvcControllerFiles = new List<MvcControllerFile>();
            _packagedClasses = new List<CompilationUnitSyntax>();
            _outputPath = _mvcPath + OutputSuffix;
        }

        public void PackageProject()
        {
            PrintStartUpMessage();
            ReferenceFiles();
            SetOutputPaths();
            AddClassesToNamespaces();
            CreatePackagedFiles();
            UpdateModelsInViews();
            PrintExitMessage();
        }

        private void PrintStartUpMessage()
        {
            Console.WriteLine("_______________________");
            Console.WriteLine("| INICIANDO OPERAÇÕES |");
            Console.WriteLine("_______________________\n");
            Console.WriteLine($"Iniciando organização em pacotes do projeto MVC localizado em \"{_mvcPath}\"\n");
        }

        private void PrintExitMessage()
        {
            Console.WriteLine("\nO processo foi concluído sem erros e o resultado se encontra em:\n");
            Console.WriteLine($"\"{_outputPath}\"\n");
        }

        private void ReferenceFiles()
        {
            ReferenceViews();
            ReferenceModels();
            ReferenceControllers();
            AssignControllerToViews();
        }

        private void ReferenceViews()
        {
            Console.WriteLine("Buscando arquivos de visão...\n");

            var cshtmlFiles = Helper.FindAllCshtmlFiles(_mvcPath);

            foreach (var cshtmlFile in cshtmlFiles)
            {
                Console.WriteLine($" - Arquivo de visão encontrado em: {cshtmlFile}");
                _mvcViewFiles.Add(new MvcViewFile(cshtmlFile));
            }

            if (_mvcViewFiles.Count == 0)
                Console.WriteLine("ATENÇÃO: não há arquivos de visão no projeto!\n");
            else
                Console.WriteLine();
        }

        private void ReferenceModels()
        {
            Console.WriteLine("Buscando arquivos de domínio...\n");

            foreach (var csharpFile in _csharpFiles)
            {
                var visitor = new MvcVisitor(csharpFile, _mvcViewFiles.ToArray());

                if (visitor.IsModel())
                {
                    Console.WriteLine($" - Arquivo de domínio encontrado em: {csharpFile}");
                    var model = new MvcModelFile(csharpFile);
                    model.ClassDeclarations = visitor.GetClasses();
                    model.UsingDirectives = visitor.GetUsingDirectives();
                    _mvcModelFiles.Add(model);
                }
            }

            if (_mvcModelFiles.Count == 0)
                Console.WriteLine("ATENÇÃO: não há arquivos de domínio no projeto!\n");
            else
                Console.WriteLine();
        }

        private void ReferenceControllers()
        {
            Console.WriteLine("Buscando arquivos de controle...\n");

            foreach (var csharpFile in _csharpFiles)
            {
                var visitor = new MvcVisitor(csharpFile, _mvcViewFiles.ToArray());

                if (visitor.IsController(out List<MethodDeclarationSyntax> controllerMethods))
                {
                    var controller = new MvcControllerFile(csharpFile);
                    controller.ClassDeclarations = visitor.GetClasses();
                    controller.UsingDirectives = visitor.GetUsingDirectives();
                    Console.WriteLine($"Arquivo de controle {controller.InferredName()} encontrado em: {controller.FileInputPath}");
                    _mvcControllerFiles.Add(AssignViewsToController(controller, controllerMethods));
                }
            }

            if (_mvcControllerFiles.Count == 0)
                Console.WriteLine("ATENÇÃO: não há arquivos de controle no projeto!\n");
            else
                Console.WriteLine();
        }

        private MvcControllerFile AssignViewsToController(MvcControllerFile controller,
            List<MethodDeclarationSyntax> controllerMethods)
        {
            controller.AddViews(controllerMethods, _mvcViewFiles.ToList());
            Console.WriteLine($"Referenciando views ao controlador {controller.InferredName()}\n");

            foreach (var file in controller.ReferencedViewFiles)
            {
                Console.WriteLine($" - Referenciando view {file.InferredName()} localizada em {file.FileInputPath}");
            }

            Console.WriteLine();
            return controller;
        }

        private void AssignControllerToViews()
        {
            Console.WriteLine("Atribuindo controladores aos arquivos de visão...\n");

            foreach (var viewFile in _mvcViewFiles)
            {
                foreach (var controllerFile in _mvcControllerFiles)
                {
                    if (controllerFile.HasView(viewFile))
                        viewFile.Controller = controllerFile;
                }
            }

            CheckViewsReferencing();
        }

        private void CheckViewsReferencing()
        {
            Console.WriteLine("Procurando arquivos de visão não referenciados por controladores...\n");

            foreach (var viewFile in _mvcViewFiles)
            {
                if (!ReferenceEquals(viewFile.Controller, null)) continue;
                if (viewFile.IsShared) continue;
                Console.WriteLine($" - ATENÇÃO: o arquivo de visão {viewFile.InferredName()} não é referenciado por um controlador!");
            }
        }

        private void SetOutputPaths()
        {
            var modelsPath = $"{_outputPath}\\Models";
            var controllersPath = $"{_outputPath}\\Controllers";
            var viewsPath = $"{_outputPath}\\Views";

            SetOutputPathsForModels(modelsPath);
            SetOutputPathsForControllers(controllersPath);
            SetOutputPathsForViews(viewsPath);
        }

        private void SetOutputPathsForModels(string path)
        {
            foreach (var modelFile in _mvcModelFiles)
            {
                modelFile.FileOutputPath = $"{path}\\{Path.GetFileName(modelFile.FileInputPath)}";
            }
        }

        private void SetOutputPathsForControllers(string path)
        {
            foreach (var controllerFile in _mvcControllerFiles)
            {
                controllerFile.FileOutputPath = $"{path}\\{Path.GetFileName(controllerFile.FileInputPath)}";
            }
        }

        private void SetOutputPathsForViews(string path)
        {
            foreach (var viewFile in _mvcViewFiles)
            {
                var outputDir = path;

                if (viewFile.IsShared)
                {
                    if (!Path.GetFileName(viewFile.FileInputPath).Contains("_ViewStart"))
                        outputDir += "\\Shared";
                }
                else
                {
                    outputDir += $"\\{viewFile.Controller.InferredName()}";
                }

                viewFile.FileOutputPath = $"{outputDir}\\{Path.GetFileName(viewFile.FileInputPath)}";
            }
        }

        private OptionSet GetOptions()
        {
            var options = _currentWorkspace.Options;
            return options.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInProperties, false);
        }

        private void AddClassesToNamespaces()
        {
            Console.WriteLine($"\nAdicionando classes ao pacote {_packageName}...\n");
            AddModelsToNamespace();
            AddControllersToNamespace();
        }

        private void AddModelsToNamespace()
        {
            foreach (var modelFile in _mvcModelFiles)
            {
                var identifiers = Helper.FindNamespaceIdentifiers(modelFile.FileOutputPath, _outputPath, _packageName);
                NamespaceDeclarationSyntax namespaceDeclaration = Helper.CreateNamespaceFromIdentifiers(identifiers);
                modelFile.QualifiedName = namespaceDeclaration.Name.ToString();
                Console.WriteLine($"Classe {modelFile.InferredName()} será movida ao pacote {modelFile.QualifiedName}");
                
                var compilationUnit = Helper.CreatePackagedCompilationUnit(modelFile, namespaceDeclaration);
                modelFile.CompilationUnit = compilationUnit;
                _packagedClasses.Add(compilationUnit);

                Console.WriteLine();
            }

        }

        private void AddControllersToNamespace()
        {
            foreach (var controllerFile in _mvcControllerFiles)
            {
                var identifiers = Helper.FindNamespaceIdentifiers(controllerFile.FileOutputPath, _outputPath, _packageName);
                NamespaceDeclarationSyntax namespaceDeclaration = Helper.CreateNamespaceFromIdentifiers(identifiers);
                controllerFile.QualifiedName = namespaceDeclaration.Name.ToString();
                Console.WriteLine($"Classe {controllerFile.InferredName()} será movida ao pacote {controllerFile.QualifiedName}");
                
                var compilationUnit = Helper.CreatePackagedCompilationUnit(controllerFile, namespaceDeclaration);
                controllerFile.CompilationUnit = compilationUnit;
                _packagedClasses.Add(compilationUnit);

                Console.WriteLine();
            }
        }

        private void CreatePackagedFiles()
        {
            Helper.CreateMvcDirectories(_outputPath, _mvcControllerFiles);
            CreatePackagedModels();
            CreatePackageControllers();
        }

        private void CreatePackagedModels()
        {
            foreach (var model in _mvcModelFiles)
            {
                using (var fileStream = File.CreateText(model.FileOutputPath))
                {
                    Console.WriteLine($"Salvando arquivo de domínio {model.InferredName()} em {model.FileOutputPath}");
                    var text = model.CompilationUnit.NormalizeWhitespace().ToFullString();
                    fileStream.WriteLine(text);
                }
            }
        }

        private void CreatePackageControllers()
        {
            foreach (var controller in _mvcControllerFiles)
            {
                using (var fileStream = File.CreateText(controller.FileOutputPath))
                {
                    Console.WriteLine($"Salvando arquivo de controle {controller.InferredName()} em {controller.FileOutputPath}\n");
                    var text = controller.CompilationUnit.NormalizeWhitespace().ToFullString();
                    fileStream.WriteLine(text);
                }
            }
        }

        private void UpdateModelsInViews()
        {
            Console.WriteLine("Atualizando pacotes das classes de domínio dentro das views...\n");

            foreach (var viewFile in _mvcViewFiles)
            {
                string viewContent = File.ReadAllText(viewFile.FileInputPath);
                bool hasUpdatedView = false;

                foreach (var modelFile in _mvcModelFiles)
                {
                    if (!viewContent.Contains(modelFile.InferredName()))
                        continue;

                    viewContent = viewContent.Replace(modelFile.InferredName(), $"{modelFile.QualifiedName}.{modelFile.InferredName()}");

                    using (var fileStream = File.CreateText(viewFile.FileOutputPath))
                    {
                        fileStream.WriteLine(viewContent);
                    }

                    hasUpdatedView = true;
                }

                if (!hasUpdatedView)
                {
                    File.Copy(viewFile.FileInputPath, viewFile.FileOutputPath);
                }
            }
        }
    }
}
