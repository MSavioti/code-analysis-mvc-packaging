using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TP6.CustomTypes
{
    public class MvcModelFile : MvcCsharpFile
    {
        public MvcModelFile(string fileInputPath) : base(fileInputPath)
        {
        }

        public override string InferredName()
        {
            var fileName = Path.GetFileNameWithoutExtension(FileInputPath);
            return fileName.Replace("Models", "").Replace("Model", "");
        }
    }
}
