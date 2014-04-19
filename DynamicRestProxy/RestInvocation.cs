using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicRestProxy
{
    class RestInvocation
    {
        public RestInvocation()
        {
            Verb = "get";
        }

        public string Verb { get; set; }

        public string Name { get; set; }



    }
}
