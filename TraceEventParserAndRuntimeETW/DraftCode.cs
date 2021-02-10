
    /*
    public struct ManifestNode
    {
        public string nodeName;
        public string[] attributes;
        public ManifestNode[] childNodes;

        public ManifestNode(string name, string[] attrs, ManifestNode[] children)
        {
            nodeName = name;
            attributes = attrs;
            childNodes = children;
        }
    }
    */

        // public static Dictionary<string, ManifestNode> nodeTemplates;

        /*
        public static void Init()
        {
            var templatesNode = new ManifestNode(
                "templates",
                null,
                new ManifestNode[]
                {
                    new ManifestNode(
                        "template",
                        new string[] { "tid" },
                        new ManifestNode[]
                        {
                            new ManifestNode(
                                "data",
                                new string[] { "name", "inType" },
                                null
                            ),
                            new ManifestNode("UserData", null, null)
                        }
                    )
                }
            );

            // If possible, refactor to initialize with the constructor
            // instead of calling Add() for each node set individually.
            nodeTemplates = new Dictionary<string, ManifestNode>();
            nodeTemplates.Add(templatesNode.nodeName, templatesNode);
        }
        */

        /*
        public static void ParseSubsetDraft(XmlDocument doc, string nodeSubset)
        {
            ManifestNode nodeTemplate = nodeTemplates[nodeSubset];
            XmlNodeList nodeList = doc.GetElementsByTagName(nodeSubset);
            Console.WriteLine(nodeList.Count);

            if (nodeList.Count <= 0)
            {
                Console.WriteLine("No groups to parse.");
                return ;
            }

            for (int i = 0; i < nodeList.Count; i++)
            {
                XmlNode nextNode = nodeList[i];
                ManifestNode nextTemplate = nodeTemplate.childNodes[i];
                Console.WriteLine("First loop");
                Console.WriteLine("Node Name: {0}", nextNode.Name);
                Console.WriteLine("Next Template: {0}", nextTemplate.nodeName);

                for (int j = 0; j < nextNode.ChildNodes.Count; j++)
                {
                    var nextElem = nextNode.ChildNodes[j];
                    Console.WriteLine("\n\nSecond Loop");
                    Console.WriteLine("Next Elem: {0}", nextElem.Name);

                    if (nextTemplate.attributes != null)
                    {
                        Console.Write("\n");
                        foreach (string attr in nextTemplate.attributes)
                            Console.WriteLine("Attr: {0}", nextElem.Attributes[attr].Value);
                    }

                    if (nextTemplate.childNodes != null)
                    {
                        var nextTemplate2 = nextTemplate.childNodes[0];
                        Console.WriteLine("Next Template 2: {0}", nextTemplate2.nodeName);
                        for (int k = 0; k < nextElem.ChildNodes.Count; k++)
                        {
                            var nextElem2 = nextElem.ChildNodes[k];
                            Console.WriteLine("\n\nThird Loop");
                            Console.WriteLine("Next Elem2: {0}", nextElem2.Name);

                            if (nextElem2.Name != "data")
                                nextTemplate2 = nextTemplate.childNodes[1];

                            if (nextTemplate2.attributes != null)
                            {
                                Console.Write("\n");
                                foreach (string attr in nextTemplate2.attributes)
                                    Console.WriteLine("Attr: {0}", nextElem2.Attributes[attr].Value);
                            }
                        }
                    }
                }
            }
        }
        */
