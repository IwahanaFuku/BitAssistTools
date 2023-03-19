using UnityEngine;

namespace SelectionAsistance
{
    public struct SelectionObjectsData
    {
        public bool isChecked;
        public string name;
        public Object[] objects;

        public SelectionObjectsData(bool isChecked, string name, Object[] objects)
        {
            this.isChecked = isChecked;
            this.name = name;
            this.objects = objects;
        }
    }
}