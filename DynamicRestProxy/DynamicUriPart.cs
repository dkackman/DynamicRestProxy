using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;

using RestSharp;

namespace DynamicRestProxy
{
    class DynamicUriPart : DynamicObject
    {
        private string _name;
        public DynamicUriPart(string name)
        {
            _name = name;
        }
    }
}
