using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace ProjectExcluder
{
    class Program
    {
        static void Main(string[] args)
        {
            //if (args.Count() == 1)
            //{

            //var fullSourceFilePath = args.Single();
            var fullSourceFilePath = Path.Combine(Environment.CurrentDirectory, "monprojet.csproj");

            if (!File.Exists(fullSourceFilePath))
                throw new FileNotFoundException("not found !");

            var baseDirectory = Path.GetDirectoryName(fullSourceFilePath);

            var doc = AddSonarQubeElement(fullSourceFilePath);

            doc.Save(fullSourceFilePath);

            Console.WriteLine("End");
            Console.Read();
            //}

        }

        private static void FindChildProjects(string filePath)
        {
            var document = XDocument.Load(filePath);

            foreach (var element in document.Root.Elements())
            {
                if (element.Name.LocalName == "PropertyGroup" && !element.HasAttributes && element.HasElements) // && !firstPropertyGroupFound)
                {
                    element.Add(new XElement(parentNameSpace + "SonarQubeExclude", true));
                    //firstPropertyGroupFound = true;
                    break;
                }
            }
        }

        private static XDocument AddSonarQubeElement(string filePath)
        {
            var document = XDocument.Load(filePath);
            var parentNameSpace = document.Root.Name.Namespace;
            //var firstPropertyGroupFound = false;

            foreach (var element in document.Root.Elements())
            {
                if (element.Name.LocalName == "PropertyGroup" && !element.HasAttributes && element.HasElements) // && !firstPropertyGroupFound)
                {
                    element.Add(new XElement(parentNameSpace + "SonarQubeExclude", true));
                    //firstPropertyGroupFound = true;
                    break;
                }
            }

            return document;
        }
    }
}
