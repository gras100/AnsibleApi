using plenidev.AnsiblePlayer.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace plenidev.AnsiblePlayer
{
    public static partial class JobTemplates
    {
        public static partial class Launch
        {
            public class GetOptions<ExVar>(): AnsibleOptionsBase<GetOptions<ExVar>>(apiVersionSupport: "api/v2") where ExVar : class 
            {

            }
        }
    }
   
}
