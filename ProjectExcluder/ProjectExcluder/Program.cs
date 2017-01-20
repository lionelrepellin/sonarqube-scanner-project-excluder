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
        private static int _processedFilesCount = 0;

        static void Main(string[] args)
        {
            if (args.Count() != 1)
                return;

            var mainProjectFilePath = args[0];

            if (!File.Exists(mainProjectFilePath))
                throw new FileNotFoundException($"Not found: {mainProjectFilePath}");

            FindChildProjects(mainProjectFilePath);

            Console.WriteLine($"The End: {_processedFilesCount} files have been processed");
            Console.Read();
        }

        private static void FindChildProjects(string filePath)
        {
            filePath = new DirectoryInfo(filePath).FullName;

            var baseDirectory = Path.GetDirectoryName(filePath);
            var document = XDocument.Load(filePath);
            var itemGroups = FindItemGroupElements(document);

            foreach (var itemGroup in itemGroups)
            {
                var projectReferences = FindProjectReferenceElements(itemGroup);
                if (projectReferences.Any())
                {
                    foreach (var projectReference in projectReferences)
                    {
                        var childProjectPath = new DirectoryInfo(Path.Combine(baseDirectory, projectReference.Attribute("Include").Value)).FullName;

                        if (!File.Exists(childProjectPath))
                            throw new FileNotFoundException($"Not found: {childProjectPath}");

                        AddSonarQubeElement(childProjectPath);
                        FindChildProjects(childProjectPath);
                    }
                }
            }
        }

        private static void AddSonarQubeElement(string filePath)
        {
            var document = XDocument.Load(filePath);
            var parentNameSpace = document.Root.Name.Namespace;
            var propertyGroup = FindPropertyGroup(document);

            if (!IsFileAlreadyUpdated(propertyGroup))
            {
                propertyGroup.Add(new XElement(parentNameSpace + "SonarQubeExclude", true));
                document.Save(filePath);

                _processedFilesCount++;
                Console.WriteLine($"Add SonarQubeExclude attribute to: {Path.GetFileName(filePath)}");
            }
        }

        private static bool IsFileAlreadyUpdated(XElement propertyGroup)
        {
            return propertyGroup.Elements()
                                .Any(p => p.Name.LocalName == "SonarQubeExclude");
        }

        private static IEnumerable<XElement> FindProjectReferenceElements(XElement itemGroup)
        {
            return itemGroup.Elements()
                            .Where(e =>
                                e.Name.LocalName == "ProjectReference" &&
                                e.HasAttributes &&
                                e.Attribute("Include") != null &&
                                e.Attribute("Include").Value.Contains(".csproj")
            ).ToList();
        }

        private static IEnumerable<XElement> FindItemGroupElements(XDocument document)
        {
            return document.Root.Elements()
                                .Where(e =>
                                    e.Name.LocalName == "ItemGroup" &&
                                    !e.HasAttributes &&
                                    e.HasElements
            ).ToList();
        }

        private static XElement FindPropertyGroup(XDocument document)
        {
            return document.Root.Elements()
                                .FirstOrDefault(e =>
                                    e.Name.LocalName == "PropertyGroup" &&
                                    !e.HasAttributes && 
                                    e.HasElements
            );
        }
    }
}