using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TP6.CustomTypes
{
    public abstract class MvcFile : IEquatable<MvcFile>
    {
        public string FileInputPath { get; }
        public string FileOutputPath { get; set; }

        protected MvcFile(string fileInputPath)
        {
            FileInputPath = fileInputPath;
        }

        public abstract string InferredName();


        public bool Equals(MvcFile other)
        {
            return FileInputPath.Equals(other?.FileInputPath);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MvcFile) obj);
        }

        public override int GetHashCode()
        {
            return (FileInputPath != null ? FileInputPath.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return FileInputPath;
        }
    }
}
