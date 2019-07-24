using System.Collections.Generic;
using System.Linq;
using AngleSharp.Dom;

namespace FixUpExhibitPages {

    internal static class PageManipulator {

        public const string CONTENT = "content";

        public static IElement UpsertHeadElement(IDocument document, string elementName, string nameAttributeName,
            string nameAttributeValue, string contentAttributeValue) {
            IElement el = document.Head.QuerySelector($"{elementName}[{nameAttributeName} = '{nameAttributeValue}']");
            if (el == null) {
                el = document.CreateElement(elementName);
                el.SetAttribute(nameAttributeName, nameAttributeValue);
                FindEditableRegion(document.Head, "head").End.InsertBefore(new INode[] { el, document.CreateTextNode("\n") });
            }

            el.SetAttribute(CONTENT, contentAttributeValue);
            return el;
        }

        private static EditableRegion FindEditableRegion(INode parent, string name) {
            IComment start = parent.GetNodes<IComment>(false, node => node.Data.Equals($@" InstanceBeginEditable name=""{name}"" "))
                .FirstOrDefault();

            IList<INode> contentNodes = new List<INode>();

            IComment end = null;
            if (start != null) {
                for (INode innerNode = start.NextSibling; innerNode != null; innerNode = innerNode.NextSibling) {
                    if (innerNode is IComment comment && comment.Data == @" InstanceEndEditable ") {
                        end = comment;
                        break;
                    } else {
                        contentNodes.Add(innerNode);
                    }
                }
            }

            return new EditableRegion(start, contentNodes, end);
        }

        public static void ReplaceTemplateText(IDocument document, INode parent, string templateName, string templateContent) {
            EditableRegion editableRegion = FindEditableRegion(parent, templateName);
            foreach (INode innerNode in editableRegion.InnerNodes) {
                innerNode.RemoveFromParent();
            }

            editableRegion.Start.InsertAfter(document.CreateTextNode(templateContent));
        }

    }

}