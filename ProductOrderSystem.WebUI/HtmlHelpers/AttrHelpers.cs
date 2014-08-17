using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProductOrderSystem.WebUI.HtmlHelpers
{
    public class AttrHelpers
    {
        public static Dictionary<string, object> LabelAttributes()
        {
            return new Dictionary<string, object>
            {
                { "class", "control-label" }
            };
        }

        public static Dictionary<string, object> DisabledTextboxAttributes(string css = null)
        {
            Dictionary<string, object> ret = new Dictionary<string, object>
            {
                { "disabled", "disabled" }
            };

            if (css != null)
                ret.Add("class", css);

            return ret;
        }

        public static Dictionary<string, object> TextboxAttributes(string placeholder, string css = null, string model = null, string disable = null)
        {
            Dictionary<string, object> ret = new Dictionary<string, object>
            {
                //{ "class", "k-textbox normal" },
                { "placeholder", placeholder },
                { "x-webkit-speech", "x-webkit-speech" }
            };

            if (css != null)
                ret.Add("class", css);

            if (model != null)
                ret.Add("ng-model", model);

            if (disable != null)
                ret.Add("ng-disabled", disable);

            return ret;
        }

        public static Dictionary<string, object> ValidationAttributes()
        {
            return new Dictionary<string, object>
            {
                { "class", "label label-important" }
            };
        }

        public static Dictionary<string, object> DateAttributes(string placeholder = "", string model = "")
        {
            Dictionary<string, object> ret = new Dictionary<string, object>
            {
                { "class", "input-medium" },
                { "datepicker-popup", "yyyy-MM-dd" },
                { "is-open", "opened" },
                { "ng-model", model }
            };

            if (model != "")
                ret["is-open"] = ret["is-open"] + model;

            return ret;
        }

        public static Dictionary<string, object> TimeAttributes()
        {
            return new Dictionary<string, object>
            {
                { "class", "input-medium" },
                { "bs-timepicker", "" }
            };
        }

        public static Dictionary<string, object> TypeaheadAttributes(string placeholder, string css = null, string x = "")
        {
            Dictionary<string, object> ret = TextboxAttributes(placeholder, css);
            ret["typeahead"] = x;
            ret["autocomplete"] = "off";
            ret["ng-model"] = "x";
            return ret;
        }

        public static Dictionary<string, object> UISelectAttributes(string model = "", string css = null, bool multiple = false)
        {
            Dictionary<string, object> ret = new Dictionary<string, object>
            {
                { "ui-select2", "" },
                { "ng-model", model },
                { "data-placeholder", "Please select" },
                { "class", "span3" }
            };

            if (css != null)
                ret["class"] = css;

            if (multiple)
                ret["multiple"] = "";

            return ret;
        }
    }
}