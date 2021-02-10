using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

/*
 * Words to Parse:
 *   - Keywords->Keyword (name, mask, message, symbol)
 *
 *   - Templates->Template (tid)
 *                  ->Data (name, inType)
 *                  ->UserData
 *
 *   - Events->Event (value, version, level, template, keywords, opcode, task, symbol, message)
 *
 *   - Tasks->Task (name, symbol, value, eventGUID, message)
 *              -> Opcode (name, message, symbol, value)
 */

namespace TraceEventParserAndRuntimeETW
{
    internal class ManifestNodeData
    {
        public string Name { get; set; }
        public string InType { get; set; }
        public string OutType { get; set; }

        public ManifestNodeData(string name = "", string inType = "", string outType = "")
        {
            Name = name;
            InType = inType;
            OutType = outType;
        }

        public string typesToString()
        {
            var sb = new StringBuilder();
            if (!String.IsNullOrEmpty(InType))
                sb.Append(InType);
            if (!String.IsNullOrEmpty(OutType))
                sb.AppendFormat(" ; {0}", OutType);
            return sb.ToString();
        }
    }

    public class ETWManifestAnalyzer
    {
        private XmlDocument SubsetDocument;
        private Dictionary<string, List<ManifestNodeData>> TemplatesDict;
        private string LastEntryAdded;

        public ETWManifestAnalyzer(string filename)
        {
            SubsetDocument = new XmlDocument();
            SubsetDocument.Load(filename);
            TemplatesDict = new Dictionary<string, List<ManifestNodeData>>();
            LastEntryAdded = "";
        }

        private void ProcessNode(XmlNode node)
        {
            // Console.WriteLine("\nNode Name: {0}", node.Name);
            string nodeName = node.Name;

            if (nodeName.Equals("template"))
            {
                XmlAttributeCollection templateAttrs = node.Attributes;
                string templateName = templateAttrs["tid"].Value;

                if (!TemplatesDict.TryAdd(templateName, new List<ManifestNodeData>()))
                    return ;

                LastEntryAdded = templateName;
            }

            if (nodeName.Equals("data"))
            {
                XmlAttributeCollection dataAttrs = node.Attributes;
                IEnumerator attrEnum = dataAttrs.GetEnumerator();
                ManifestNodeData nodeStorage = new ManifestNodeData();

                while (attrEnum.MoveNext())
                {
                    XmlAttribute attr = (XmlAttribute) attrEnum.Current;

                    switch (attr.Name)
                    {
                        case "name":
                            nodeStorage.Name = attr.Value;
                            break;
                        case "inType":
                            nodeStorage.InType = attr.Value;
                            break;
                        case "outType":
                            nodeStorage.OutType = attr.Value;
                            break;
                        default:
                            break;
                    }
                }

                TemplatesDict[LastEntryAdded].Add(nodeStorage);
            }

            // if (nodeName.Equals("template") || nodeName.Equals("data"))
            // {
            //     if (node.Attributes != null)
            //     {
            //         XmlAttributeCollection attrs = node.Attributes;
            //         IEnumerator attrEnum = attrs.GetEnumerator();

            //         // NEXT STEP: Create objects with Node Data in this phase.
            //         while (attrEnum.MoveNext())
            //         {
            //             XmlAttribute attr = (XmlAttribute) attrEnum.Current;
            //             Console.WriteLine("{0} = {1}", attr.Name, attr.Value);
            //         }
            //     }
            // }

            if (node.ChildNodes != null && node.Name != "UserData")
                ProcessNodeList(node.ChildNodes);
        }

        private void ProcessNodeList(XmlNodeList nodeList)
        {
            for (int i = 0; i < nodeList.Count; i++)
            {
                XmlNode nextNode = nodeList[i];
                ProcessNode(nextNode);
            }
        }

        private void PrintDict()
        {
            foreach (KeyValuePair<string, List<ManifestNodeData>> kvp in TemplatesDict)
            {
                Console.WriteLine(kvp.Key);
                Console.Write("{ ");

                foreach (ManifestNodeData attr in kvp.Value)
                    Console.Write("{0}, ", attr.Name);
                Console.WriteLine("}\n");
            }
        }

        private void ExportToTxtFile()
        {
            string currentDir = Directory.GetCurrentDirectory();
            string outputFilePath = Path.Combine(currentDir, "manifestParsed.txt");
        }

        public int AnalyzeTemplatesSubset()
        {
            XmlNodeList rootNodesList = SubsetDocument.GetElementsByTagName("templates");

            if (rootNodesList.Count <= 0)
            {
                Console.WriteLine("No groups found to parse in the XML Document.");
                return -1;
            }

            ProcessNodeList(rootNodesList);
            PrintDict();
            return 0;
        }

    }

    public class ClrTraceEventParserAnalyzer
    {
        private int LineIndex;
        private string[] Data;
        private int TotalLines;

        private Dictionary<string, string> ClassesPayloadNames;

        public ClrTraceEventParserAnalyzer(string filePath)
        {
            Data = File.ReadAllLines(filePath);
            LineIndex = 0;
            TotalLines = Data.Length;
            ClassesPayloadNames = new Dictionary<string, string>();
        }

        private string GetClassName(string classDefLine)
        {
            string[] classDefinition = classDefLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            int classNameIndex = Array.FindIndex(classDefinition, word => word.Equals("class")) + 1;
            return classDefinition[classNameIndex];
            // return Array.Find(classDefinition, word => word.Contains("TraceData"));
        }

        private string GetPayloadNames(string payloadNamesLine)
        {
            Match pNms = Regex.Match(payloadNamesLine, @"{.*}");
            return pNms.Success ? pNms.Value : "";
        }

        private string ProcessTraceDataClass()
        {
            string line = Data[++LineIndex];
            // Console.WriteLine("Class in line {0}...", lineIndex + 1);
            while (!line.StartsWith("namespace") && LineIndex < TotalLines-1)
            {
                if (line.Contains("sealed class"))
                {
                    --LineIndex;
                    return "";
                }

                if (line.Contains("payloadNames = new string"))
                {
                    if (line.Contains("}"))
                        return line;

                    StringBuilder sb = new StringBuilder(line);
                    while (!line.Contains("}"))
                    {
                        line = Data[++LineIndex];
                        sb.Append(line.Trim(' '));
                    }

                    return sb.ToString();
                }
                line = Data[++LineIndex];
            }
            return "";
        }

        private void ProcessNamespaceClasses()
        {
            string line = Data[LineIndex];
            while (!line.StartsWith("namespace") && LineIndex < TotalLines-1)
            {
                if (line.Contains("sealed class"))
                {
                    string className = GetClassName(line);
                    string payloadNamesLine = ProcessTraceDataClass();
                    // Console.WriteLine(className);
                    // Console.WriteLine("Exited class\n");

                    if (!String.IsNullOrEmpty(payloadNamesLine))
                    {
                        string payloadNames = GetPayloadNames(payloadNamesLine);
                        ClassesPayloadNames.Add(className, payloadNames);
                        // Console.WriteLine("{0}\n", payloadNames);
                    }
                }
                line = Data[++LineIndex];
            }
        }

        private void ExportToTxtFile()
        {
            string currentDir = Directory.GetCurrentDirectory();
            string outputFilePath = Path.Combine(currentDir, "payloadNames.txt");

            using (StreamWriter outputFile = new StreamWriter(outputFilePath))
            {
                foreach (KeyValuePair<string, string> entry in ClassesPayloadNames)
                {
                    outputFile.WriteLine(entry.Key);
                    outputFile.WriteLine(entry.Value);
                    outputFile.Write("\n");
                }
            }
        }

        public int AnalyzeTraceParserSource()
        {
            while (LineIndex < TotalLines)
            {
                string line = Data[LineIndex++];
                if (line.Contains("namespace Microsoft.Diagnostics.Tracing.Parsers.Clr"))
                {
                    Console.WriteLine("Namespace in line {0}...\n", LineIndex);
                    ProcessNamespaceClasses();
                }
            }

            ExportToTxtFile();
            return 0;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            XMLManifestRun(args[0]);
            // PerfViewParserRun(args[0]);
        }

        static void XMLManifestRun(string fileName)
        {
            var analyzer = new ETWManifestAnalyzer(fileName);
            int result = analyzer.AnalyzeTemplatesSubset();
            Console.WriteLine(result);
            return ;

            // XmlDocument doc = new XmlDocument();
            // doc.Load(fileName);

            // int result = ETWManifestAnalyzer.AnalyzeSubset(doc, "templates");
            // Console.WriteLine("\nResult: {0}", result);
        }

        static void PerfViewParserRun(string fileName)
        {
            var analyzer = new ClrTraceEventParserAnalyzer(fileName);
            int result = analyzer.AnalyzeTraceParserSource();
            Console.WriteLine(result);
            return ;
        }
    }
}
