using System.Collections.Generic;
using AngleSharp.Dom;

#nullable enable

namespace FixUpExhibitPages {

    internal class EditableRegion {

        public IComment start { get; }
        public IEnumerable<INode> innerNodes { get; }
        public IComment end { get; }

        public EditableRegion(IComment start, IEnumerable<INode> innerNodes, IComment end) {
            this.start = start;
            this.innerNodes = innerNodes;
            this.end = end;
        }

    }

}