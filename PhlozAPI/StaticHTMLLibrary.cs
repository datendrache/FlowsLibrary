using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhlozLib
{
    public class StaticHTMLLibrary
    {
        public static string getStartScriptBlock()
        {
            string result = "";
            result += "<script type='text/javascript' src='//code.jquery.com/jquery-1.10.1.js'>//<![CDATA[\n";
            result += "\n$(window).load(function(){\n";
            return (result);
        }

        public static string getEndScriptBlock()
        {
            string result = "";
            result += "});";
            result += "//]]>\n</script>\n";
            return (result);
        }
        public static string getMagicLink(string frame)
        {
            string magicLink = "";
            magicLink += 
            magicLink += "$(\"a.magic-link\").click(function(e){\ne.preventDefault();\n$(\"#"+frame+"\").attr(\"src\", $(this).attr(\"href\"));\n});\n";
            return (magicLink);
        }

        public static string getResizeScript()
        {
            string resize = "";
            resize += "\n$(window).resize(function(e){\n"+
                  "$(\"#table-main\").css({height: ($(window).height() - 100) + \"px\"})\n" +
                  "$(\"#I2\").css({height: ($(window).height() - 100) + \"px\"})\n" +
                  "});\n";
            resize += "$(\"#table-main\").css({height: ($(window).height() - 100) + \"px\"});\n";
            resize += "$(\"#I1\").css({height: ($(window).height() - 100) + \"px\"});\n";
            return (resize);
        }

        public static string getLogStyle()
        {
            return ("<style type=\"text/css\">.auto-style1 {font-family: Cambria, Cochin, Georgia, Times, \"Times New Roman\", serif; font-size: x-small;}</style>\n");
        }

        public static string getConsole(CollectionState State)
        {
            string result = "";
            result += "<table id=\"table-main\" style=\"width: 100%; height: 100%\">\n" +
                       "<tr style=\"height: 1000px\">\n" +
                       "<td style=\"width: 22%\">\n" +
                       "<table>\n" +
                       //WebmodChannel.getChannelList(State,"I1") +
                       "</table>\n" +
                       "</td>\n" +
                       "<td>\n" +
                       "<iframe id=\"I1\" name=\"I1\" src=\"http://localhost:8080/Channel/1\" style=\"width: 100%; height: 100%\">Your browser does not support inline frames or is currently configured not to display inline frames.\n" +
                       "</iframe></td>\n" +
                       "</tr>\n" +
                       "</table>\n";
            return result;
        }

        public static string errorMessage(int type, string details)
        {
            string result = "<HTML><HEAD><meta http-equiv=\"refresh\" content=\"30\"></HEAD><BODY>";
            result += "<p><strong>Phloz Server Error Code "+type.ToString()+"</strong></p>";
            result += "<p>Details:</p>";
            result += "<block>" + details + "</block>";
            result += "</BODY></HTML>";
            return result;
        }

        public static string errorTypes(int type)
        {
            string result = "Undefined";
            switch (type)
            {
                case 1: result = "Cannot generate channel listing."; break;
                case 2: result = "Cannot generate console data."; break;
                case 3: result = "Cannot generate flow data."; break;
            }
            return result;
        }

        public static string redirectPage(string redirectURL)
        {
            string result = "<HTML><HEAD><meta http-equiv=\"refresh\" content=\"0; URL='"+redirectURL+"'\"></HEAD><BODY>";
            result += "<p>Click <a href=\""+redirectURL+"\">here</a> if not automatically redirected.</p>";
            result += "</BODY></HTML>";
            return result;
        }
    }
}
