using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace GrokNet
{
    public sealed class GrokResult : ReadOnlyCollection<GrokItem>
    {
        public GrokResult(IList<GrokItem> grokItems)
            : base(grokItems ?? new List<GrokItem>())
        {
        }

        public IReadOnlyDictionary<string, IEnumerable<object>> ToDictionary() => Items
            .GroupBy(grokItem => grokItem.Key)
            .ToDictionary(groupName => groupName.Key, grokItems => grokItems.Select(grokItem => grokItem.Value));
    }
}
