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
        public PatchOperation hasLiteModeOn;

        protected override bool ApplyWorker(XmlDocument xml)
        {
            if (RimValiMod.settings.liteMode)
            {
                return hasLiteModeOn.Apply(xml);
            }

            Log.Message(RimValiMod.settings.liteMode.ToString());
            return true;
        }
    }

    public class PatchOperationRandReplace : PatchOperationReplace
    {
        public List<XmlContainer> randValue = new List<XmlContainer>();

        protected override bool ApplyWorker(XmlDocument xml)
        {
            XmlNode node = randValue.RandomElement().node;
            if (node == null)
            {
                return false;
            }

            var result = false;
            foreach (XmlNode xmlNode in xml.SelectNodes(xpath).Cast<XmlNode>().ToArray())
            {
                result = true;
                XmlNode parentNode = xmlNode.ParentNode;
                foreach (XmlNode childNode in node.ChildNodes)
                {
                    parentNode.InsertBefore(parentNode.OwnerDocument.ImportNode(childNode, true), xmlNode);
                }

                parentNode.RemoveChild(xmlNode);
            }

            return result;
        }
    }

    public class PatchOperationRandInsert : PatchOperationInsert
    {
        private readonly Order order = Order.Prepend;
        public List<XmlContainer> randValue = new List<XmlContainer>();

        protected override bool ApplyWorker(XmlDocument xml)
        {
            XmlNode node = randValue.RandomElement().node;
            if (node == null || xml == null)
            {
                return false;
            }

            var result = false;
            foreach (XmlNode xmlNode in xml.SelectNodes(xpath))
            {
                result = true;
                XmlNode parentNode = xmlNode.ParentNode;
                if (parentNode?.OwnerDocument == null)
                {
                    continue;
                }

                switch (order)
                {
                    case Order.Append:
                    {
                        foreach (XmlNode childNode in node.ChildNodes)
                        {
                            parentNode.InsertAfter(parentNode.OwnerDocument.ImportNode(childNode, true), xmlNode);
                        }

                        break;
                    }
                    case Order.Prepend:
                    {
                        for (int i = node.ChildNodes.Count - 1; i >= 0; i--)
                        {
                            parentNode.InsertBefore(parentNode.OwnerDocument.ImportNode(node.ChildNodes[i], true),
                                xmlNode);
                        }

                        break;
                    }
                }

                if (xmlNode.NodeType == XmlNodeType.Text)
                {
                    parentNode.Normalize();
                }
            }

            return result;
        }

        private enum Order
        {
            Append,
            Prepend,
        }
    }
}
