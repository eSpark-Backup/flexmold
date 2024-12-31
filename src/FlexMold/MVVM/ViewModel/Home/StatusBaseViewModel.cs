using FlexMold.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlexMold.MVVM.ViewModel.Home
{
    public class StatusBaseViewModel : ObservableObject
    {
        public int[] GetMismatchedIndexes<T>(T[] array1, T[] array2)
        {
            var mismatchedIndexes = new List<int>();

            if (array1 == null)
                return Enumerable.Range(0, array2.Length).ToArray();
            
            if (array1.Length != array2.Length)
            {
                throw new ArgumentException("Arrays must have the same length.");
            }
            Parallel.For(0, array1.Length, i =>
            {
                if (!array1[i].Equals(array2[i]))
                {
                    mismatchedIndexes.Add(i);
                }
            });

            return mismatchedIndexes.ToArray();
        }
    }
}
