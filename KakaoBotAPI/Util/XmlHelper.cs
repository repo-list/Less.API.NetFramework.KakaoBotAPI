using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;

namespace Less.API.NetFramework.KakaoBotAPI.Util
{
    /// <summary>
    /// XML 파일을 처리하는 데 도움을 주는 유틸리티 클래스입니다.
    /// </summary>
    public class XmlHelper
    {
        /// <summary>
        /// XML 파일의 기본 확장자
        /// </summary>
        public const string FileExtension = ".xml";

        /// <summary>
        /// XML 파일의 경로<para/>
        /// 생성자 내부에서 변형된 값이 저장되므로, getter만을 허용합니다.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// XML 파일 최상단 요소의 태그명<para/>
        /// 이 값은 XML 파일 생성 시에만 사용되며, XML 파일을 읽어들일 경우는 ReadFile 메서드의 반환 값인 Document 객체 내의 속성이 사용됩니다.
        /// </summary>
        string RootElementName;

        /// <summary>
        /// 최상단 요소 내부의 자식 노드 목록<para/>
        /// 이 값은 XML 파일 생성 시에만 사용되며, XML 파일을 읽어들일 경우는 ReadFile 메서드의 반환 값인 Document 객체 내의 속성이 사용됩니다.
        /// </summary>
        List<Node> ChildNodes;

        XmlWriterSettings WriterSettings;

        const int ThreadProcessInterval = 20;

        /// <summary>
        /// XML 헬퍼 객체를 생성합니다.
        /// </summary>
        /// <param name="path">XML 파일의 경로</param>
        public XmlHelper(string path)
        {
            Path = new FileInfo(path.Contains(FileExtension) && path.IndexOf(FileExtension) == path.Length - 4 ? path : path + FileExtension).FullName;

            WriterSettings = new XmlWriterSettings();
            WriterSettings.Encoding = new UTF8Encoding(false);
            WriterSettings.Indent = true;
        }

        /// <summary>
        /// XML 파일을 생성합니다.<para/>
        /// 만약 기존 파일이 존재한다면, 덮어쓰기가 진행됩니다.
        /// </summary>
        /// <param name="rootElementName">XML 파일 최상단 요소의 태그명</param>
        /// <param name="childNodes">최상단 요소 내부의 자식 노드 목록</param>
        public void CreateFile(string rootElementName, List<Node> childNodes)
        {
            RootElementName = rootElementName;
            ChildNodes = childNodes;
            new Thread(new ThreadStart(CreateTask)).Start();
            lock (this) Monitor.Wait(this);
        }

        /// <summary>
        /// XML 파일을 읽어들입니다.<para/>
        /// 반환 값인 XmlHelper.Document 객체를 이용하여 노드들의 값을 얻어올 수 있습니다.
        /// </summary>
        /// <returns>XML 문서 객체</returns>
        public Document ReadFile()
        {
            new Thread(new ThreadStart(ReadTask)).Start();
            lock (this) Monitor.Wait(this);
            return new Document(RootElementName, ChildNodes);
        }

        /// <summary>
        /// 실제 파일 쓰기 작업을 기술한 메서드입니다.
        /// </summary>
        void CreateTask()
        {
            Thread.Sleep(ThreadProcessInterval);

            using (var writer = XmlWriter.Create(Path, WriterSettings))
            {
                writer.WriteStartDocument(true);
                writer.WriteStartElement(RootElementName);

                foreach (var node in ChildNodes)
                {
                    writer.WriteStartElement(node.Name);
                    foreach (var p in node.Properties) writer.WriteAttributeString(p.Key, $"{p.Value}");
                    foreach (var v in node.DataList) writer.WriteElementString(v.Key, v.Value);
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }

            lock (this) Monitor.Pulse(this);
        }

        /// <summary>
        /// 실제 파일 읽기 작업을 기술한 메서드입니다.
        /// </summary>
        void ReadTask()
        {
            Thread.Sleep(ThreadProcessInterval);

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
                                RootElementName = reader.Name;
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
                                for (int i = 0; i < vKeys.Count; i++) childNode.AddData(vKeys[i], vValues[i]);
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

        /// <summary>
        /// XML 문서 객체
        /// </summary>
        public struct Document
        {
            /// <summary>
            /// XML 문서 최상단 요소의 태그명
            /// </summary>
            public string RootElementName { get; }

            /// <summary>
            /// 최상단 요소 내부의 자식 노드 목록
            /// </summary>
            public List<Node> ChildNodes { get; }

            /// <summary>
            /// XML 문서 객체를 생성합니다.
            /// </summary>
            /// <param name="rootElementName">XML 문서 최상단 요소의 태그명</param>
            /// <param name="childNodes">최상단 요소 내부의 자식 노드 목록</param>
            public Document(string rootElementName, List<Node> childNodes)
            {
                RootElementName = rootElementName;
                ChildNodes = childNodes;
            }
        }

        /// <summary>
        /// XML 노드 객체
        /// </summary>
        public struct Node
        {
            /// <summary>
            /// 노드의 이름
            /// </summary>
            public string Name { get; }

            /// <summary>
            /// 노드의 속성 목록
            /// </summary>
            public List<NodeData> Properties { get; }

            /// <summary>
            /// 노드의 데이터 목록<para/>
            /// 내부에 Key-Value Pair가 존재합니다.
            /// </summary>
            public List<NodeData> DataList { get; }

            /// <summary>
            /// XML 노드 객체를 생성합니다.
            /// </summary>
            /// <param name="name"></param>
            public Node(string name)
            {
                Name = name;
                Properties = new List<NodeData>();
                DataList = new List<NodeData>();
            }

            /// <summary>
            /// XML 노드에 속성을 삽입합니다.
            /// </summary>
            /// <param name="key">속성 Key</param>
            /// <param name="value">속성 Value</param>
            public void AddProperty(string key, bool value) { Properties.Add(new NodeData(key, $"{value}")); }

            /// <summary>
            /// XML 노드에 속성을 삽입합니다.
            /// </summary>
            /// <param name="key">속성 Key</param>
            /// <param name="value">속성 Value</param>
            public void AddProperty(string key, sbyte value) { Properties.Add(new NodeData(key, $"{value}")); }

            /// <summary>
            /// XML 노드에 속성을 삽입합니다.
            /// </summary>
            /// <param name="key">속성 Key</param>
            /// <param name="value">속성 Value</param>
            public void AddProperty(string key, byte value) { Properties.Add(new NodeData(key, $"{value}")); }

            /// <summary>
            /// XML 노드에 속성을 삽입합니다.
            /// </summary>
            /// <param name="key">속성 Key</param>
            /// <param name="value">속성 Value</param>
            public void AddProperty(string key, char value) { Properties.Add(new NodeData(key, $"{value}")); }

            /// <summary>
            /// XML 노드에 속성을 삽입합니다.
            /// </summary>
            /// <param name="key">속성 Key</param>
            /// <param name="value">속성 Value</param>
            public void AddProperty(string key, short value) { Properties.Add(new NodeData(key, $"{value}")); }

            /// <summary>
            /// XML 노드에 속성을 삽입합니다.
            /// </summary>
            /// <param name="key">속성 Key</param>
            /// <param name="value">속성 Value</param>
            public void AddProperty(string key, ushort value) { Properties.Add(new NodeData(key, $"{value}")); }

            /// <summary>
            /// XML 노드에 속성을 삽입합니다.
            /// </summary>
            /// <param name="key">속성 Key</param>
            /// <param name="value">속성 Value</param>
            public void AddProperty(string key, int value) { Properties.Add(new NodeData(key, $"{value}")); }

            /// <summary>
            /// XML 노드에 속성을 삽입합니다.
            /// </summary>
            /// <param name="key">속성 Key</param>
            /// <param name="value">속성 Value</param>
            public void AddProperty(string key, uint value) { Properties.Add(new NodeData(key, $"{value}")); }

            /// <summary>
            /// XML 노드에 속성을 삽입합니다.
            /// </summary>
            /// <param name="key">Key</param>
            /// <param name="value">Value</param>
            public void AddProperty(string key, long value) { Properties.Add(new NodeData(key, $"{value}")); }

            /// <summary>
            /// XML 노드에 속성을 삽입합니다.
            /// </summary>
            /// <param name="key">속성 Key</param>
            /// <param name="value">속성 Value</param>
            public void AddProperty(string key, ulong value) { Properties.Add(new NodeData(key, $"{value}")); }

            /// <summary>
            /// XML 노드에 속성을 삽입합니다.
            /// </summary>
            /// <param name="key">속성 Key</param>
            /// <param name="value">속성 Value</param>
            public void AddProperty(string key, float value) { Properties.Add(new NodeData(key, $"{value}")); }

            /// <summary>
            /// XML 노드에 속성을 삽입합니다.
            /// </summary>
            /// <param name="key">속성 Key</param>
            /// <param name="value">속성 Value</param>
            public void AddProperty(string key, double value) { Properties.Add(new NodeData(key, $"{value}")); }

            /// <summary>
            /// XML 노드에 속성을 삽입합니다.
            /// </summary>
            /// <param name="key">속성 Key</param>
            /// <param name="value">속성 Value</param>
            public void AddProperty(string key, decimal value) { Properties.Add(new NodeData(key, $"{value}")); }

            /// <summary>
            /// XML 노드에 속성을 삽입합니다.
            /// </summary>
            /// <param name="key">속성 Key</param>
            /// <param name="value">속성 Value</param>
            public void AddProperty(string key, string value) { Properties.Add(new NodeData(key, $"{value}")); }

            /// <summary>
            /// XML 노드에서 속성을 제거합니다.
            /// </summary>
            /// <param name="key">속성 Key</param>
            public void RemoveProperty(string key)
            {
                for (int i = 0; i < Properties.Count; i++) if (Properties[i].Key == key) { Properties.RemoveAt(i); break; }
            }

            /// <summary>
            /// XML 노드 속성의 값을 가져옵니다.
            /// </summary>
            /// <param name="key">속성 Key</param>
            /// <returns>속성 Value</returns>
            public string GetProperty(string key)
            {
                for (int i = 0; i < Properties.Count; i++) if (Properties[i].Key == key) return Properties[i].Value;
                return null;
            }

            /// <summary>
            /// XML 노드에 데이터를 삽입합니다.
            /// </summary>
            /// <param name="key">데이터 Key</param>
            /// <param name="value">데이터 Value</param>
            public void AddData(string key, bool value) { DataList.Add(new NodeData(key, $"{value}")); }

            /// <summary>
            /// XML 노드에 데이터를 삽입합니다.
            /// </summary>
            /// <param name="key">데이터 Key</param>
            /// <param name="value">데이터 Value</param>
            public void AddData(string key, sbyte value) { DataList.Add(new NodeData(key, $"{value}")); }

            /// <summary>
            /// XML 노드에 데이터를 삽입합니다.
            /// </summary>
            /// <param name="key">데이터 Key</param>
            /// <param name="value">데이터 Value</param>
            public void AddData(string key, byte value) { DataList.Add(new NodeData(key, $"{value}")); }

            /// <summary>
            /// XML 노드에 데이터를 삽입합니다.
            /// </summary>
            /// <param name="key">데이터 Key</param>
            /// <param name="value">데이터 Value</param>
            public void AddData(string key, char value) { DataList.Add(new NodeData(key, $"{value}")); }

            /// <summary>
            /// XML 노드에 데이터를 삽입합니다.
            /// </summary>
            /// <param name="key">데이터 Key</param>
            /// <param name="value">데이터 Value</param>
            public void AddData(string key, short value) { DataList.Add(new NodeData(key, $"{value}")); }

            /// <summary>
            /// XML 노드에 데이터를 삽입합니다.
            /// </summary>
            /// <param name="key">데이터 Key</param>
            /// <param name="value">데이터 Value</param>
            public void AddData(string key, ushort value) { DataList.Add(new NodeData(key, $"{value}")); }

            /// <summary>
            /// XML 노드에 데이터를 삽입합니다.
            /// </summary>
            /// <param name="key">데이터 Key</param>
            /// <param name="value">데이터 Value</param>
            public void AddData(string key, int value) { DataList.Add(new NodeData(key, $"{value}")); }

            /// <summary>
            /// XML 노드에 데이터를 삽입합니다.
            /// </summary>
            /// <param name="key">데이터 Key</param>
            /// <param name="value">데이터 Value</param>
            public void AddData(string key, uint value) { DataList.Add(new NodeData(key, $"{value}")); }

            /// <summary>
            /// XML 노드에 데이터를 삽입합니다.
            /// </summary>
            /// <param name="key">데이터 Key</param>
            /// <param name="value">데이터 Value</param>
            public void AddData(string key, long value) { DataList.Add(new NodeData(key, $"{value}")); }

            /// <summary>
            /// XML 노드에 데이터를 삽입합니다.
            /// </summary>
            /// <param name="key">데이터 Key</param>
            /// <param name="value">데이터 Value</param>
            public void AddData(string key, ulong value) { DataList.Add(new NodeData(key, $"{value}")); }

            /// <summary>
            /// XML 노드에 데이터를 삽입합니다.
            /// </summary>
            /// <param name="key">데이터 Key</param>
            /// <param name="value">데이터 Value</param>
            public void AddData(string key, float value) { DataList.Add(new NodeData(key, $"{value}")); }

            /// <summary>
            /// XML 노드에 데이터를 삽입합니다.
            /// </summary>
            /// <param name="key">데이터 Key</param>
            /// <param name="value">데이터 Value</param>
            public void AddData(string key, double value) { DataList.Add(new NodeData(key, $"{value}")); }

            /// <summary>
            /// XML 노드에 데이터를 삽입합니다.
            /// </summary>
            /// <param name="key">데이터 Key</param>
            /// <param name="value">데이터 Value</param>
            public void AddData(string key, decimal value) { DataList.Add(new NodeData(key, $"{value}")); }

            /// <summary>
            /// XML 노드에 데이터를 삽입합니다.
            /// </summary>
            /// <param name="key">데이터 Key</param>
            /// <param name="value">데이터 Value</param>
            public void AddData(string key, string value) { DataList.Add(new NodeData(key, $"{value}")); }

            /// <summary>
            /// XML 노드에서 데이터를 삭제합니다.
            /// </summary>
            /// <param name="key">데이터 Key</param>
            public void RemoveData(string key)
            {
                for (int i = 0; i < DataList.Count; i++) if (DataList[i].Key == key) { DataList.RemoveAt(i); break; }
            }

            /// <summary>
            /// XML 노드에서 데이터의 값을 가져옵니다.
            /// </summary>
            /// <param name="key">데이터 key</param>
            /// <returns>데이터 Value</returns>
            public string GetData(string key)
            {
                for (int i = 0; i < DataList.Count; i++) if (DataList[i].Key == key) return DataList[i].Value;
                return null;
            }
        }

        /// <summary>
        /// XML 노드의 데이터 객체
        /// </summary>
        public struct NodeData
        {
            /// <summary>
            /// XML 노드 데이터의 Key
            /// </summary>
            public string Key { get; set; }

            /// <summary>
            /// XML 노드 데이터의 Value
            /// </summary>
            public string Value { get; set; }

            /// <summary>
            /// XML 노드 데이터 객체를 생성합니다.
            /// </summary>
            /// <param name="key">XML 노드 데이터의 Key</param>
            /// <param name="value">XML 노드 데이터의 Value</param>
            public NodeData(string key, string value)
            {
                Key = key;
                Value = value;
            }
        }
    }
}
