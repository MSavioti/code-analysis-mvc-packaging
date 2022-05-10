using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TP6.CustomTypes
{
    public class MvcControllerFile : MvcCsharpFile
    {
        public List<MvcViewFile> ReferencedViewFiles { get; }

        public MvcControllerFile(string fileInputPath) : base(fileInputPath)
        {
            ReferencedViewFiles = new List<MvcViewFile>();
        }

        public override string InferredName()
        {
            var fileName = Path.GetFileNameWithoutExtension(FileInputPath);
            return fileName.Replace("Controller", "");
        }

        public bool HasView(MvcViewFile viewFile)
        {
            return ReferencedViewFiles.Contains(viewFile);
        }

        public void AddViews(List<MethodDeclarationSyntax> controllerMethods, List<MvcViewFile> availableViews)
        {
            foreach (var controllerMethod in controllerMethods)
            {
                foreach (var view in availableViews)
                {
                    if (controllerMethod.Identifier.ToString().Equals(view.InferredName()))
                    {
                        ReferencedViewFiles.Add(view);
                        break;
                    }
                }
            }
        }
    }
}
