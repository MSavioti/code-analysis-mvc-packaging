using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TP6.CustomTypes
{
    public sealed class MvcViewFile : MvcFile
    {
        public MvcControllerFile Controller { get; set; }
        public bool IsShared { get; set; }

        public MvcViewFile(string fileInputPath) : base(fileInputPath)
        {
            IsShared = (InferredName()[0] == '_') || (fileInputPath.Contains("Shared"));
        }

        public override string InferredName()
        {
            var fileName = Path.GetFileNameWithoutExtension(FileInputPath);
            return fileName.Replace("View", "");
        }
    }
}
