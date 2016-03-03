using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RS500gaWCFService.data
{
    public class LfTag
    {
        private string _name;
        private string _url;
        private int _id;

        public string name
        {
            get
            {
                return this._name;
            }
            set
            {
                this._name = value;
            }
        }

        public string url
        {
            get
            {
                return this._url;
            }
            set
            {
                this._url = value;
            }
        }

        public int id
        {
            get
            {
                return this._id;
            }
            set
            {
                this._id = value;
            }
        }

        public override string ToString()
        {
            string retval =
            "<Id:" + this.id +
            "><TagName:" + this.name + ">";

            return retval;
        }
    }
}