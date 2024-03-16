using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCUI_UI
{
    internal class FixedSizeQueue<T> : Queue<T>
    {
        public int MaxSize { get; private set; }
        public FixedSizeQueue(int maxSize)
        {
            MaxSize = maxSize;
        }
        public new void Enqueue(T item)
        {
            base.Enqueue(item);
            while (base.Count > MaxSize)
            {
                base.TryDequeue(out _);
            }
        }
    }

}
