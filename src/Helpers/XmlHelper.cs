using System.Xml;

namespace BaseUISupport.Helpers;

public static class XmlHelper
{
    public static void NewDocument(out XmlDocument document, out XmlNode? rootNode, string? baseNode = null)
    {
        XmlDocument xml = new();
        XmlDeclaration xmlDeclaration = xml.CreateXmlDeclaration("1.0", "UTF-8", null);
        XmlElement? root = xml.DocumentElement;
        xml.InsertBefore(xmlDeclaration, root);
        if (baseNode is not null)
        {
            XmlNode node = xml.CreateElement(string.Empty, baseNode, string.Empty);
            xml.AppendChild(node);
            rootNode = node;
        }
        else
        {
            rootNode = null;
        }
        document = xml;
    }
    
    public static XmlNode NewNode(XmlDocument xml, string nodename, XmlNode? append = null)
    {
        XmlNode newnode = xml.CreateElement(string.Empty, nodename, string.Empty);
        append?.AppendChild(newnode);
        return newnode;
    }

    public static XmlAttribute NewAttribute(XmlDocument xml, string attrname, string value, XmlNode? append = null)
    {
        XmlAttribute newattr = xml.CreateAttribute(string.Empty, attrname, string.Empty);
        newattr.Value = value;
        append?.Attributes?.Append(newattr);
        return newattr;
    }

    public static XmlNode NewTextNode(XmlDocument xml, string nodename, string value, XmlNode? append = null)
    {
        XmlNode newnode = xml.CreateElement(string.Empty, nodename, string.Empty);
        XmlText newtextnode = xml.CreateTextNode(value);
        newnode.AppendChild(newtextnode);
        append?.AppendChild(newnode);
        return newnode;
    }
}