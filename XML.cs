using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;

public static class XML
{
    private static XmlDocument xDocument = null;
    private static XmlNodeList xNodeList = null;
    private static Dictionary<string, XmlNodeList> xSavedLists = new Dictionary<string, XmlNodeList>();

    static XML()
    {
    }

    static public XmlDocument OpenDocument(string path)
    {
        xDocument = new XmlDocument();
        XmlTextReader reader = new XmlTextReader(path);
        xDocument.Load(reader);
        reader.Close();
        return xDocument;
    }

    static public XmlDocument WriteDocument(string path)
    {
        xDocument = new XmlDocument();
        XmlTextWriter writer = new XmlTextWriter(path, System.Text.Encoding.UTF8);
        xDocument.WriteTo(writer);
        writer.Close();
        return xDocument;
    }

    static public void CloseDocument()
    {
        xDocument = null;
        xNodeList = null;
        xSavedLists.Clear();
    }

    static public XmlNode SelectSingleNode(string xPath)
    {
        return xDocument.SelectSingleNode(xPath);
    }

    /// <summary>
    /// Used with singular nodes
    /// </summary>
    /// <param name="node"></param>
    /// <param name="xPath"></param>
    /// <returns></returns>
    static public string ReadAttributeFrom(string node, string attribute)
    {
        return ReadAttributeFrom(xDocument.SelectSingleNode(node), attribute);
    }

    static private string ReadAttributeFrom(XmlNode node, string attribute)
    {
        if (node == null || null == node.Attributes[attribute])
            return string.Empty;
        return node.Attributes[attribute].Value;
    }

    /// <summary>
    /// Used with singular nodes
    /// </summary>
    /// <param name="xPath"></param>
    /// <returns></returns>
    static public string ReadInnerTextFrom(string xPath)
    {
        return xDocument.SelectSingleNode(xPath).InnerText;
    }

    /// <summary>
    /// Creates a node list from the xml path.
    /// </summary>
    /// <param name="xPath">Element to make a list of</param>
    static public void CreateNodeListOf(string xPath)
    {
        xNodeList = xDocument.SelectNodes(xPath);
    }

    /// <summary>
    /// Creates a node list from a previous node list
    /// </summary>
    /// <param name="parentList">The parent XmlNodeList</param>
    /// <param name="indexInParentList">The index of an XmlNode in the parentList</param>
    /// <param name="xPath">The elements inside the XmlNode to turn into an XmlNodeList</param>
    static public void CreateNodeListOf(string parentList, int indexInParentList, string xPath)
    {
        xNodeList = xSavedLists[parentList][indexInParentList].SelectNodes(xPath);
    }

    /// <summary>
    /// Calls SaveCurrentNodeListAs and then CreateNodeListOf
    /// </summary>
    /// <param name="saveAs">Name to save list as</param>
    /// <param name="index">Index of the list</param>
    /// <param name="element">Element to find</param>
    static public void SaveThenMakeActive(string saveAs, int index, string element)
    {
        SaveCurrentNodeListAs(saveAs);
        CreateNodeListOf(index, element);
    }

    /// <summary>
    /// Calls SaveCurrentNodeListAs and then CreateNodeListOf
    /// </summary>
    /// <param name="saveAs">Name to save list as</param>
    /// <param name="element">Element to find</param>
    static public void SaveThenMakeActive(string saveAs, string element)
    {
        SaveCurrentNodeListAs(saveAs);
        CreateNodeListOf(element);
    }

    /// <summary>
    /// Saves the current active list away to make room for another active list
    /// </summary>
    /// <param name="name">Save the node list as</param>
    static public void SaveCurrentNodeListAs(string name)
    {
        xSavedLists.Add(name, xNodeList);
    }

    /// <summary>
    /// Restores a previous node list to being the active list
    /// </summary>
    /// <param name="name">Name of the node list</param>
    static public void RestoreOldNodeList(string name)
    {
        xNodeList = xSavedLists[name];
        xSavedLists.Remove(name);
    }

    /// <summary>
    /// Creates an XmlNodeList from an XmlNode inside the currently active list.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="element"></param>
    static public void CreateNodeListOf(int index, string element)
    {
        xNodeList = xNodeList[index].SelectNodes(element);
    }

    /// <summary>
    /// Count of elements in the node list
    /// </summary>
    /// <returns>Int..</returns>
    static public int NodeListCount()
    {
        return xNodeList.Count;
    }

    /// <summary>
    /// Used with an active node list
    /// </summary>
    /// <param name="index"></param>
    /// <param name="xPath"></param>
    /// <returns></returns>
    static public string ReadAttributeFrom(int index, string xPath)
    {
        return ReadAttributeFrom(xNodeList[index], xPath);
    }

    /// <summary>
    /// Used with an active node list
    /// </summary>
    /// <param name="index"></param>
    /// <param name="xPath"></param>
    /// <returns></returns>
    static public string ReadInnerTextFrom(int index, string element)
    {
        if (xNodeList[index].SelectSingleNode(element) == null)
            return string.Empty;
        return xNodeList[index].SelectSingleNode(element).InnerText;
    }

    static public string ReadAttributeFromChildNode(int index, string element, string attribute)
    {
        return ReadAttributeFrom(xNodeList[index].SelectSingleNode(element), attribute);
    }
}

