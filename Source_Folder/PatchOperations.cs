using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Verse;

namespace AvaliMod
{
    public class ModuleDef : Def
    {
        public string name;
    }

    public class PatchOperationLiteMode : PatchOperation
    {
        protected override bool ApplyWorker(XmlDocument xml)
        {
            if (RimValiMod.settings.liteMode)
            {
                return hasLiteModeOn.Apply(xml);
            }
            Log.Message(RimValiMod.settings.liteMode.ToString());
            return true;
        }

        public PatchOperation hasLiteModeOn;
    }

    public class PatchOperationRandReplace : PatchOperationReplace
    {
        public List<XmlContainer> randValue = new List<XmlContainer>();

        protected override bool ApplyWorker(XmlDocument xml)
        {
            XmlNode node = randValue.RandomElement().node;
            bool result = false;
            if (node != null)
            {
                foreach (XmlNode xmlNode in xml.SelectNodes(xpath).Cast<XmlNode>().ToArray())
                {
                    result = true;
                    XmlNode parentNode = xmlNode.ParentNode;
                    foreach (object obj in node.ChildNodes)
                    {
                        XmlNode node2 = (XmlNode)obj;
                        parentNode.InsertBefore(parentNode.OwnerDocument.ImportNode(node2, true), xmlNode);
                    }
                    parentNode.RemoveChild(xmlNode);
                }
            }
            return result;
        }
    }

    public class PatchOperationRandInsert : PatchOperationInsert
    {
        public List<XmlContainer> randValue = new List<XmlContainer>();
        private readonly Order order = Order.Prepend;

        private enum Order
        {
            Append,
            Prepend
        }

        protected override bool ApplyWorker(XmlDocument xml)
        {
            XmlNode node = randValue.RandomElement().node;
            bool result = false;
            if (node != null)
            {
                foreach (XmlNode xmlNode in xml.SelectNodes(xpath))
                {
                    result = true;
                    XmlNode parentNode = xmlNode.ParentNode;
                    if (order == Order.Append)
                    {
                        foreach (XmlNode childNode in node.ChildNodes)
                        {
                            parentNode.InsertAfter(parentNode.OwnerDocument.ImportNode(childNode, true), xmlNode);
                        }
                    }
                    else if (order == Order.Prepend)
                    {
                        for (int i = node.ChildNodes.Count - 1; i >= 0; i--)
                        {
                            parentNode.InsertBefore(parentNode.OwnerDocument.ImportNode(node.ChildNodes[i], true), xmlNode);
                        }
                    }
                    if (xmlNode.NodeType == XmlNodeType.Text)
                    {
                        parentNode.Normalize();
                    }
                }
            }
            return result;
        }
    }
}