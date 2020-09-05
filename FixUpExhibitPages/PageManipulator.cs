using System.Collections.Generic;
using System.Linq;
using AngleSharp.Dom;

#nullable enable

namespace FixUpExhibitPages {

    internal static class PageManipulator {

        public const string CONTENT = "content";

        public static IElement upsertHeadElement(IDocument document, string elementName, string nameAttributeName, string nameAttributeValue, string contentAttributeValue) {
            IElement? el = document.Head.QuerySelector($"{elementName}[{nameAttributeName} = '{nameAttributeValue}']");
            if (el == null) {
                el = document.CreateElement(elementName);
                el.SetAttribute(nameAttributeName, nameAttributeValue);
                findEditableRegion(document.Head, "head")?.end.InsertBefore(new INode[] { el, document.CreateTextNode("\n") });
            }

            el.SetAttribute(CONTENT, contentAttributeValue);
            return el;
        }

        public static void replaceTemplateText(IDocument document, INode parent, string templateName, string templateContent) {
            if (findEditableRegion(parent, templateName) is EditableRegion editableRegion) {
                foreach (INode innerNode in editableRegion.innerNodes) {
                    innerNode.RemoveFromParent();
                }

                editableRegion.start.InsertAfter(document.CreateTextNode(templateContent));
            }
        }

        private static EditableRegion? findEditableRegion(INode parent, string name) {
            IComment? start = parent.GetNodes<IComment>(false, node => node.Data.Equals($@" InstanceBeginEditable name=""{name}"" "))
                .FirstOrDefault();

            IList<INode> contentNodes = new List<INode>();

            IComment? end = null;
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

            return start != null && end != null ? new EditableRegion(start, contentNodes, end) : null;
        }

    }

}