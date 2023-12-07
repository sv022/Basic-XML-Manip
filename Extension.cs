using System.Xml;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;
public static class XElementExtensions
{
    public static XmlElement ToXmlElement(this XElement el)
    {
        var doc = new XmlDocument();
        doc.Load(el.CreateReader());
        return doc.DocumentElement;
    }
}

public static class DocumentExtensions
    {
        public static XmlDocument ToXmlDocument(this XDocument xDocument)
        {
            var xmlDocument = new XmlDocument();
            using(var xmlReader = xDocument.CreateReader())
            {
                xmlDocument.Load(xmlReader);
            }
            return xmlDocument;
        }

        public static XDocument ToXDocument(this XmlDocument xmlDocument)
        {
            using (var nodeReader = new XmlNodeReader(xmlDocument))
            {
                nodeReader.MoveToContent();
                return XDocument.Load(nodeReader);
            }
        }
    }