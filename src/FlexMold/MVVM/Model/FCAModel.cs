using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FlexMold.MVVM.Model
{
    internal static class FCAModel
    {
        public static List<TreeItem> MyBoxes = new List<TreeItem>();

        public static ObservableCollection<Foe> FoeStatuses = new ();
    }
}