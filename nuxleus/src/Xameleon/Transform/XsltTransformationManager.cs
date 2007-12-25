using System;
using System.Collections;
using System.Collections.Generic;
using Saxon.Api;
using System.Xml;
using System.IO;
using System.Web;
using Nuxleus.Cryptography;

namespace Nuxleus.Transform
{

    //NOTE: TransformEngine enum PLACEHOLDER FOR FUTURE USE
    public enum TransformEngine { SAXON, MVPXML, NET_1_1, NET_2_0, NET_3_0, NET_3_5, SILVERLIGHT }

    public struct XsltTransformationManager
    {

        Dictionary<string, XsltTransformer> m_xsltHashtable;
        Dictionary<string, Stream> m_sourceHashtable;
        Dictionary<string, XdmNode> m_xdmNodeHashtable;
        Dictionary<string, Uri> m_xdmNodeETagIndex;
        Dictionary<string, Uri> m_namedXsltHashtable;
        Dictionary<string, string> m_namedXsltETagIndex;
        Processor m_processor;
        Serializer m_serializer;
        DocumentBuilder m_builder;
        XsltCompiler m_compiler;
        XmlUrlResolver m_resolver;
        Transform m_transform;
        Uri m_baseXsltUri;
        String m_baseXsltUriHash;
        String m_baseXsltName;
        static HashAlgorithm _hashAlgorithm;
        //NOTE: TransformEngine enum PLACEHOLDER FOR FUTURE USE
        static TransformEngine m_transformEngine;

        public XsltTransformationManager(Processor processor)
            : this(processor, new Transform(), new XmlUrlResolver(), new Serializer(), new Dictionary<string, XsltTransformer>(), new Dictionary<string, Stream>(), new Dictionary<string, XdmNode>(), new Dictionary<string, Uri>(), new Dictionary<string, string>(), new Dictionary<string, Uri>(), null, null, null)
        {
        }
        public XsltTransformationManager(Processor processor, Transform transform)
            : this(processor, transform, new XmlUrlResolver(), new Serializer(), new Dictionary<string, XsltTransformer>(), new Dictionary<string, Stream>(), new Dictionary<string, XdmNode>(), new Dictionary<string, Uri>(), new Dictionary<string, string>(), new Dictionary<string, Uri>(), null, null, null)
        {
        }
        public XsltTransformationManager(Processor processor, Transform transform, XmlUrlResolver resolver)
            : this(processor, transform, resolver, new Serializer(), new Dictionary<string, XsltTransformer>(), new Dictionary<string, Stream>(), new Dictionary<string, XdmNode>(), new Dictionary<string, Uri>(), new Dictionary<string, string>(), new Dictionary<string, Uri>(), null, null, null)
        {
        }
        public XsltTransformationManager(Processor processor, Transform transform, XmlUrlResolver resolver, Serializer serializer)
            : this(processor, transform, resolver, serializer, new Dictionary<string, XsltTransformer>(), new Dictionary<string, Stream>(), new Dictionary<string, XdmNode>(), new Dictionary<string, Uri>(), new Dictionary<string, string>(), new Dictionary<string, Uri>(), null, null, null)
        {
        }
        public XsltTransformationManager (Processor processor, Transform transform, XmlUrlResolver resolver, Serializer serializer, Dictionary<string, XsltTransformer> xsltHashtable)
            : this(processor, transform, resolver, serializer, xsltHashtable, new Dictionary<string, Stream>(), new Dictionary<string, XdmNode>(), new Dictionary<string, Uri>(), new Dictionary<string, string>(), new Dictionary<string, Uri>(), null, null, null)
        {
        }
        public XsltTransformationManager (Processor processor, Transform transform, XmlUrlResolver resolver, Serializer serializer, Dictionary<string, XsltTransformer> xsltHashtable, Dictionary<string, Uri> namedXsltHashtable)
            : this(processor, transform, resolver, serializer, xsltHashtable, new Dictionary<string, Stream>(), new Dictionary<string, XdmNode>(), namedXsltHashtable, new Dictionary<string, string>(), new Dictionary<string, Uri>(), null, null, null)
        {
        }
        public XsltTransformationManager (Processor processor, Transform transform, XmlUrlResolver resolver, Serializer serializer, Dictionary<string, XsltTransformer> xsltHashtable, Dictionary<string, Stream> xmlSourceHashtable, Dictionary<string, Uri> namedXsltHashtable)
            : this(processor, transform, resolver, serializer, xsltHashtable, xmlSourceHashtable, new Dictionary<string, XdmNode>(), namedXsltHashtable, new Dictionary<string, string>(), new Dictionary<string, Uri>(), null, null, null)
        {
        }
        public XsltTransformationManager (Processor processor, Transform transform, XmlUrlResolver resolver, Serializer serializer, Dictionary<string, XsltTransformer> xsltHashtable, Dictionary<string, Stream> xmlSourceHashtable, Dictionary<string, XdmNode> xdmNodeHashtable, Dictionary<string, Uri> namedXsltHashtable)
            : this(processor, transform, resolver, serializer, xsltHashtable, xmlSourceHashtable, xdmNodeHashtable, namedXsltHashtable, new Dictionary<string, string>(), new Dictionary<string, Uri>(), null, null, null)
        {
        }
        public XsltTransformationManager (Processor processor, Transform transform, XmlUrlResolver resolver, Serializer serializer, Dictionary<string, XsltTransformer> xsltHashtable, Dictionary<string, Stream> xmlSourceHashtable, Dictionary<string, XdmNode> xdmNodeHashtable, Dictionary<string, Uri> namedXsltHashtable, Dictionary<string, string> namedXsltETagIndex)
            : this(processor, transform, resolver, serializer, xsltHashtable, xmlSourceHashtable, xdmNodeHashtable, namedXsltHashtable, namedXsltETagIndex, new Dictionary<string, Uri>(), null, null, null)
        {
        }
        public XsltTransformationManager (Processor processor, Transform transform, XmlUrlResolver resolver, Serializer serializer, Dictionary<string, XsltTransformer> xsltHashtable, Dictionary<string, Stream> xmlSourceHashtable, Dictionary<string, XdmNode> xdmNodeHashtable, Dictionary<string, Uri> namedXsltHashtable, Dictionary<string, string> namedXsltETagIndex, Dictionary<string, Uri> xdmNodeETagIndex)
            : this(processor, transform, resolver, serializer, xsltHashtable, xmlSourceHashtable, xdmNodeHashtable, namedXsltHashtable, namedXsltETagIndex, xdmNodeETagIndex, null, null, null)
        {
        }
        public XsltTransformationManager
          (
            Processor processor,
            Transform transform,
            XmlUrlResolver resolver,
            Serializer serializer,
            Dictionary<string, XsltTransformer> xsltHashtable,
            Dictionary<string, Stream> xmlSourceHashtable,
            Dictionary<string, XdmNode> xdmNodeHashtable,
            Dictionary<string, Uri> namedXsltHashtable,
            Dictionary<string, string> namedXsltETagIndex,
            Dictionary<string, Uri> xdmNodeETagIndex,
            Uri baseXsltUri,
            String baseXsltUriHash,
            String baseXsltName
          )
        {
            m_baseXsltUri = baseXsltUri;
            m_baseXsltUriHash = baseXsltUriHash;
            m_baseXsltName = baseXsltName;
            m_transform = transform;
            m_xsltHashtable = xsltHashtable;
            m_processor = processor;
            m_compiler = m_processor.NewXsltCompiler();
            m_sourceHashtable = xmlSourceHashtable;
            m_resolver = resolver;
            m_compiler.XmlResolver = m_resolver;
            m_builder = m_processor.NewDocumentBuilder();
            m_serializer = serializer;
            m_xdmNodeHashtable = xdmNodeHashtable;
            m_xdmNodeETagIndex = xdmNodeETagIndex;
            m_namedXsltHashtable = namedXsltHashtable;
            m_namedXsltETagIndex = namedXsltETagIndex;
            _hashAlgorithm = HashAlgorithm.SHA1;
            //NOTE: TransformEngine enum PLACEHOLDER FOR FUTURE USE
            m_transformEngine = TransformEngine.SAXON;
        }

        public void SetBaseXsltContext(BaseXsltContext baseXsltContext)
        {
            m_baseXsltUri = baseXsltContext.BaseXsltUri;
            m_baseXsltName = baseXsltContext.Name;
            m_baseXsltUriHash = baseXsltContext.UriHash;
            m_compiler.BaseUri = m_baseXsltUri;
            m_builder.BaseUri = m_baseXsltUri;
        }

        public bool HasXmlSourceChanged(string eTag)
        {
            if (m_xdmNodeETagIndex[eTag] != null)
                return false;
            else
                return true;
        }

        public string GetXdmNodeHashtableCount()
        {
            return m_xdmNodeETagIndex.Count.ToString();
        }

        public bool HasBaseXsltSourceChanged()
        {
            string namedETag = (string)m_namedXsltETagIndex[m_baseXsltName];
            if (namedETag != null && namedETag == GenerateNamedETagKey(m_baseXsltName, m_baseXsltUri))
                return false;
            else
            {
                m_namedXsltETagIndex[m_baseXsltName] = (string)m_baseXsltUriHash;
                return true;
            }
        }

        public void AddTransformer(Uri uri)
        {
            addTransformer(GenerateNamedETagKey(uri.LocalPath, uri), uri.LocalPath, uri, m_resolver, null, null, null);
        }
        public void AddTransformer(string name, Uri uri)
        {
            addTransformer(GenerateNamedETagKey(name, uri), name, uri, m_resolver, null, null, null);
        }
        public void AddTransformer(string name, Uri uri, XmlUrlResolver resolver)
        {
            addTransformer(GenerateNamedETagKey(name, uri), name, uri, resolver, null, null, null);
        }
        public void AddTransformer(string name, Uri uri, XmlUrlResolver resolver, string initialMode)
        {
            addTransformer(GenerateNamedETagKey(name, uri), name, uri, resolver, initialMode, null, null);
        }
        public void AddTransformer(string name, Uri uri, XmlUrlResolver resolver, string initialMode, string initialTemplate)
        {
            addTransformer(GenerateNamedETagKey(name, uri), name, uri, resolver, initialMode, initialTemplate, null);
        }
        public void AddTransformer(string name, Uri uri, XmlUrlResolver resolver, string initialMode, string initialTemplate, string baseOutputUri)
        {
            addTransformer(GenerateNamedETagKey(name, uri), name, uri, resolver, initialMode, initialTemplate, baseOutputUri);
        }
        private void addTransformer(string key, string name, Uri uri, XmlUrlResolver resolver, string initialMode, string initialTemplate, string baseOutputUri)
        {
            XsltTransformer transformer = createNewTransformer(uri);
            transformer.InputXmlResolver = resolver;
            if (baseOutputUri != null && baseOutputUri != String.Empty)
                transformer.BaseOutputUri = new Uri(baseOutputUri);
            if (initialMode != null && initialTemplate != String.Empty)
                transformer.InitialMode = new QName("", "", initialMode);
            if (initialTemplate != null && initialTemplate != String.Empty)
                transformer.InitialTemplate = new QName("xsl", "http://www.w3.org/1999/XSL/Transform", initialTemplate);
            m_xsltHashtable[key] = transformer;
            m_namedXsltETagIndex[name] = (string)key;
        }

        public void AddXmlSource(string name, Uri uri)
        {
            Stream xmlStream = createNewXmlStream(uri);
            m_sourceHashtable[name] = xmlStream;
        }

        public XdmNode GetXdmNode(string name, string xmlSource)
        {
            Uri xmlSourceUri = new Uri(xmlSource);
            return GetXdmNode(name, xmlSourceUri);
        }
        public XdmNode GetXdmNode(string name, Uri xmlSourceUri)
        {

            Uri xdmNodeUri = m_xdmNodeETagIndex[name];
            if (xdmNodeUri != null && xdmNodeUri == xmlSourceUri)
            {
                return getXdmNode(name, xmlSourceUri, false);
            }
            else
            {
                m_xdmNodeETagIndex[name] = xmlSourceUri;
                return getXdmNode(name, xmlSourceUri, true);
            }
        }
        private XdmNode getXdmNode(string key, Uri xmlSourceUri, bool replaceExistingXdmNode)
        {

            XdmNode node = m_xdmNodeHashtable[key];

            if (node != null && !replaceExistingXdmNode)
            {
                return node;
            }
            else
            {
                using (Stream stream = createNewXmlStream(xmlSourceUri))
                {
                    node = createNewXdmNode(stream);
                }
            }
            m_xdmNodeHashtable[key] = node;
            return node;
        }

        private Stream createNewXmlStream(Uri xmlSourceUri)
        {
            return (Stream)m_resolver.GetEntity(xmlSourceUri, null, typeof(Stream));
        }

        private XdmNode createNewXdmNode(Stream xmlSourceStream)
        {
            return (XdmNode)m_builder.Build(xmlSourceStream);
        }

        public XsltTransformer GetTransformer(string eTag, Uri xsltUri)
        {
            return getTransformer(eTag, xsltUri, true);
        }
        public XsltTransformer GetTransformer(string name, string href, Uri baseUri)
        {
            Uri xsltUri = new Uri(baseUri, href);
            return getTransformer(GenerateNamedETagKey(name, xsltUri), name, xsltUri);
        }
        public XsltTransformer GetTransformer(string name)
        {
            Uri xsltUri = m_xdmNodeETagIndex[name];
            return getTransformer(GenerateNamedETagKey(name, xsltUri), name, xsltUri);
        }
        private XsltTransformer getTransformer(string key, string xsltName, Uri xsltUri)
        {
            string namedETag = (string)m_namedXsltETagIndex[xsltName];
            if (namedETag != null && namedETag == key)
            {
                return getTransformer(namedETag, xsltUri, false);
            }
            else
            {
                m_namedXsltETagIndex[xsltName] = key;
                return getTransformer(key, xsltUri, true);
            }
        }
        private XsltTransformer getTransformer(string key, Uri xsltUri, bool replaceExistingXsltTransformer)
        {
            XsltTransformer transformer;
            transformer = m_xsltHashtable[key];

            if (transformer != null && !replaceExistingXsltTransformer)
            {
                return transformer;
            }
            else
                transformer = createNewTransformer(xsltUri);

            m_xsltHashtable[key] = transformer;
            return transformer;
        }

        public static String GenerateNamedETagKey(String name, Uri sourceUri, params object[] objectParams)
        {
            FileInfo fileInfo = new FileInfo(sourceUri.LocalPath);
            return name + ":" + Context.GenerateETag((string)HttpContext.Current.Application["hashkey"], _hashAlgorithm, fileInfo.LastWriteTimeUtc, fileInfo.Length, sourceUri, objectParams);
        }

        private XsltTransformer createNewTransformer(Uri xsltUri)
        {
            using (Stream stream = createNewXmlStream(xsltUri))
            {
                return m_compiler.Compile(stream).Load();
            }
        }

        public Dictionary<string, XsltTransformer> XsltHashtable
        {
            get { return m_xsltHashtable; }
            set { m_xsltHashtable = value; }
        }
        public Dictionary<string, Stream> XmlSourceHashtable
        {
            get { return m_sourceHashtable; }
            set { m_sourceHashtable = value; }
        }
        public Dictionary<string, Uri> NamedXsltHashtable
        {
            get { return m_namedXsltHashtable; }
            set { m_namedXsltHashtable = value; }
        }
        public HashAlgorithm HashAlgorithm
        {
            get { return _hashAlgorithm; }
            set { _hashAlgorithm = value; }
        }
        public Processor Processor
        {
            get { return m_processor; }
            set { m_processor = value; }
        }
        public DocumentBuilder DocumentBuilder
        {
            get { return m_builder; }
            set { m_builder = value; }
        }
        public XmlUrlResolver Resolver
        {
            get { return m_resolver; }
            set { m_resolver = value; }
        }
        public XsltCompiler Compiler
        {
            get { return m_compiler; }
            set { m_compiler = value; }
        }
        public Serializer Serializer
        {
            get { return m_serializer; }
            set { m_serializer = value; }
        }
        public Transform Transform
        {
            get { return m_transform; }
            set { m_transform = value; }
        }
        public Uri BaseXsltUri
        {
            get { return m_baseXsltUri; }
            set { m_baseXsltUri = value; }
        }
        public String BaseXsltUriHash
        {
            get { return m_baseXsltUriHash; }
            set { m_baseXsltUriHash = value; }
        }
        public String BaseXsltName
        {
            get { return m_baseXsltName; }
            set { m_baseXsltName = value; }
        }
    }
}
