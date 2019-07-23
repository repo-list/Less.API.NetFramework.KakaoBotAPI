using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Threading;

namespace Less.API.NetFramework.KakaoBotAPI.Util
{
    public class XmlHelper
    {
        public const string DefaultParentName = "list";
        public const string FileExtension = ".xml";

        public string Path { get; }

        XmlWriterSettings WriterSettings;
        string ParentName;
        List<Node> ChildNodes;

        public XmlHelper(string path)
        {
            Path = new FileInfo(path.Contains(FileExtension) && path.IndexOf(FileExtension) == path.Length - 4 ? path : path + FileExtension).FullName;

            WriterSettings = new XmlWriterSettings();
            WriterSettings.Indent = true;
        }

        public Node GetNewNode(string name)
        {
            return new Node(name);
        }

        public void CreateFile(string parentName, List<Node> childNodes)
        {
            ParentName = parentName;
            ChildNodes = childNodes;
            new Thread(new ThreadStart(CreateTask)).Start();
            lock (this) Monitor.Wait(this);
        }

        public Document ReadFile()
        {
            new Thread(new ThreadStart(ReadTask)).Start();
            lock (this) Monitor.Wait(this);
            return new Document(ParentName, ChildNodes);
        }

        void CreateTask()
        {
            using (var writer = XmlWriter.Create(Path, WriterSettings))
            {
                writer.WriteStartDocument(true);
                writer.WriteStartElement(ParentName);

                foreach (var node in ChildNodes)
                {
                    writer.WriteStartElement(node.Name);
                    foreach (var p in node.Properties) writer.WriteAttributeString(p.Key, $"{p.Value}");
                    foreach (var v in node.Values) writer.WriteElementString(v.Key, v.Value);
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }

            lock (this) Monitor.Pulse(this);
        }

        void ReadTask()
        {
            bool isRootNode = true;
            string childNodeName = null;
            var pKeys = new List<string>();
            var pValues = new List<string>();
            var vKeys = new List<string>();
            var vValues = new List<string>();
            Node childNode;

            if (ChildNodes == null) ChildNodes = new List<Node>();
            else ChildNodes.Clear();
            
            using (var reader = XmlReader.Create(Path))
            {
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (isRootNode)
                            {
                                isRootNode = false;
                                ParentName = reader.Name;
                            }
                            else
                            {
                                if (childNodeName == null) childNodeName = reader.Name;
                                else
                                {
                                    vKeys.Add(reader.Name);
                                    reader.Read();
                                    vValues.Add(reader.Value);
                                    reader.Read();
                                }
                            }
                            break;
                        case XmlNodeType.Attribute:
                            if (childNodeName != null) { pKeys.Add(reader.Name); pValues.Add(reader.Value); }
                            break;
                        case XmlNodeType.EndElement:
                            if (childNodeName != null)
                            {
                                childNode = new Node(childNodeName);
                                for (int i = 0; i < pKeys.Count; i++) childNode.AddProperty(pKeys[i], pValues[i]);
                                for (int i = 0; i < vKeys.Count; i++) childNode.AddValue(vKeys[i], vValues[i]);
                                ChildNodes.Add(childNode);

                                pKeys.Clear();
                                pValues.Clear();
                                vKeys.Clear();
                                vValues.Clear();
                                childNodeName = null;
                            }
                            break;
                    }
                }
            }

            lock (this) Monitor.Pulse(this);
        }

        public struct Document
        {
            public string ParentName { get; }
            public List<Node> ChildNodes { get; }

            public Document(string parentName, List<Node> childNodes)
            {
                ParentName = parentName;
                ChildNodes = childNodes;
            }
        }

        public struct Node
        {
            public string Name { get; }
            public List<NodeData> Properties { get; }
            public List<NodeData> Values { get; }

            public Node(string name)
            {
                Name = name;
                Properties = new List<NodeData>();
                Values = new List<NodeData>();
            }

            public Node AddProperty(string key, bool value) { Properties.Add(new NodeData(key, $"{value}")); return this; }
            public Node AddProperty(string key, sbyte value) { Properties.Add(new NodeData(key, $"{value}")); return this; }
            public Node AddProperty(string key, byte value) { Properties.Add(new NodeData(key, $"{value}")); return this; }
            public Node AddProperty(string key, char value) { Properties.Add(new NodeData(key, $"{value}")); return this; }
            public Node AddProperty(string key, short value) { Properties.Add(new NodeData(key, $"{value}")); return this; }
            public Node AddProperty(string key, ushort value) { Properties.Add(new NodeData(key, $"{value}")); return this; }
            public Node AddProperty(string key, int value) { Properties.Add(new NodeData(key, $"{value}")); return this; }
            public Node AddProperty(string key, uint value) { Properties.Add(new NodeData(key, $"{value}")); return this; }
            public Node AddProperty(string key, long value) { Properties.Add(new NodeData(key, $"{value}")); return this; }
            public Node AddProperty(string key, ulong value) { Properties.Add(new NodeData(key, $"{value}")); return this; }
            public Node AddProperty(string key, float value) { Properties.Add(new NodeData(key, $"{value}")); return this; }
            public Node AddProperty(string key, double value) { Properties.Add(new NodeData(key, $"{value}")); return this; }
            public Node AddProperty(string key, decimal value) { Properties.Add(new NodeData(key, $"{value}")); return this; }
            public Node AddProperty(string key, string value) { Properties.Add(new NodeData(key, $"{value}")); return this; }

            public Node RemoveProperty(string key)
            {
                for (int i = 0; i < Properties.Count; i++) if (Properties[i].Key.Equals(key)) { Properties.RemoveAt(i); break; }
                return this;
            }

            public string GetProperty(string key)
            {
                for (int i = 0; i < Properties.Count; i++) if (Properties[i].Key.Equals(key)) return Properties[i].Value;
                return null;
            }

            public Node AddValue(string key, bool value) { Values.Add(new NodeData(key, $"{value}")); return this; }
            public Node AddValue(string key, sbyte value) { Values.Add(new NodeData(key, $"{value}")); return this; }
            public Node AddValue(string key, byte value) { Values.Add(new NodeData(key, $"{value}")); return this; }
            public Node AddValue(string key, char value) { Values.Add(new NodeData(key, $"{value}")); return this; }
            public Node AddValue(string key, short value) { Values.Add(new NodeData(key, $"{value}")); return this; }
            public Node AddValue(string key, ushort value) { Values.Add(new NodeData(key, $"{value}")); return this; }
            public Node AddValue(string key, int value) { Values.Add(new NodeData(key, $"{value}")); return this; }
            public Node AddValue(string key, uint value) { Values.Add(new NodeData(key, $"{value}")); return this; }
            public Node AddValue(string key, long value) { Values.Add(new NodeData(key, $"{value}")); return this; }
            public Node AddValue(string key, ulong value) { Values.Add(new NodeData(key, $"{value}")); return this; }
            public Node AddValue(string key, float value) { Values.Add(new NodeData(key, $"{value}")); return this; }
            public Node AddValue(string key, double value) { Values.Add(new NodeData(key, $"{value}")); return this; }
            public Node AddValue(string key, decimal value) { Values.Add(new NodeData(key, $"{value}")); return this; }
            public Node AddValue(string key, string value) { Values.Add(new NodeData(key, $"{value}")); return this; }

            public Node RemoveValue(string key)
            {
                for (int i = 0; i < Values.Count; i++) if (Values[i].Key.Equals(key)) { Values.RemoveAt(i); break; }
                return this;
            }

            public string GetValue(string key)
            {
                for (int i = 0; i < Values.Count; i++) if (Values[i].Key.Equals(key)) return Values[i].Value;
                return null;
            }
        }

        public struct NodeData
        {
            public string Key { get; }
            public string Value { get; }

            public NodeData(string key, string value)
            {
                Key = key;
                Value = value;
            }
        }
    }
}
