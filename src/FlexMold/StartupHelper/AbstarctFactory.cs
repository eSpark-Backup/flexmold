using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexMold.StartupHelper
{
    public class AbstarctFactory<T> : IAbstarctFactory<T>
    {
        private readonly Func<T> _factory;
        public AbstarctFactory(Func<T> factory)
        {
            _factory = factory;
        }
        public T Create()
        {
            return _factory();
        }
    }
}
