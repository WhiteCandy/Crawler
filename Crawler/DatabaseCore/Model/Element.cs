using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseCore.Model
{
    public class Table
    {
        public string Name;
        public List<Element> ElementList = new List<Element>();
    }

    public class Element
    {
        public string Name;
        public string Type;
        public bool IsKey;
    }

}
