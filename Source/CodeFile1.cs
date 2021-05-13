using System;
using System.Collections;
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


	public class PatchOperationRandReplace : PatchOperationReplace
    {
		public List<XmlContainer> randValue = new List<XmlContainer>();
		protected override bool ApplyWorker(XmlDocument xml)
		{
			XmlNode node = this.randValue.RandomElement().node;
			bool result = false;
			if (node != null)
			{
				foreach (XmlNode xmlNode in xml.SelectNodes(this.xpath).Cast<XmlNode>().ToArray<XmlNode>())
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
		private PatchOperationRandInsert.Order order = PatchOperationRandInsert.Order.Prepend;

		// Token: 0x02000365 RID: 869
		private enum Order
		{
			// Token: 0x040010FE RID: 4350
			Append,
			// Token: 0x040010FF RID: 4351
			Prepend
		}

		protected override bool ApplyWorker(XmlDocument xml)
		{
			XmlNode node = randValue.RandomElement().node;
			bool result = false;
			if (node != null)
			{
				foreach (object obj in xml.SelectNodes(this.xpath))
				{
					result = true;
					XmlNode xmlNode = obj as XmlNode;
					XmlNode parentNode = xmlNode.ParentNode;
					if (this.order == PatchOperationRandInsert.Order.Append)
					{
						IEnumerator enumerator2 = node.ChildNodes.GetEnumerator();

						while (enumerator2.MoveNext())
						{
							object obj2 = enumerator2.Current;
							XmlNode node2 = (XmlNode)obj2;
							parentNode.InsertAfter(parentNode.OwnerDocument.ImportNode(node2, true), xmlNode);
						}
						goto IL_E0;
					}
					goto IL_98;
				IL_E0:
					if (xmlNode.NodeType == XmlNodeType.Text)
					{
						parentNode.Normalize();
						continue;
					}
					continue;
				IL_98:
					if (this.order == PatchOperationRandInsert.Order.Prepend)
					{
						for (int i = node.ChildNodes.Count - 1; i >= 0; i--)
						{
							parentNode.InsertBefore(parentNode.OwnerDocument.ImportNode(node.ChildNodes[i], true), xmlNode);
						}
						goto IL_E0;
					}
					goto IL_E0;
				}
			}
			return result;
		}
	}
}