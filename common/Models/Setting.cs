using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Models
{
    public class Setting
    {
        public Setting(string domain,string key)
        {
            this.domain = domain;
            this.key = key;
        }
        public string domain;
        public string key;
        public string value;
    }
}
