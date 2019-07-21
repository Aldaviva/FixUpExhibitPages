using System.Collections.Generic;
using AngleSharp.Dom;

namespace FixUpExhibitPages {

    internal class EditableRegion {

        public IComment Start { get; }
        public IEnumerable<INode> InnerNodes { get; }
        public IComment End { get; }

        public EditableRegion(IComment start, IEnumerable<INode> innerNodes, IComment end) {
            Start = start;
            InnerNodes = innerNodes;
            End = end;
        }

    }

}