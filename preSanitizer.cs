//   Phloz
//   Copyright (C) 2003-2019 Eric Knight

using System;
using System.Collections.Generic;
using System.Text;
using FatumCore;

namespace PhlozLib
{
    public class preSanitizer
    {
        fatumconfig fatumConfig = null;

        public long SearchTotal = 0;
        public long SearchReduction = 0;

        public System.Threading.Thread SearchSanitizer = null;
        public Tree SearchResults = null;
        public String STATUS = "";

        // Events and Delegates

        public event EventHandler OnFinished;

        public preSanitizer(fatumconfig FC)
        {

            fatumConfig = FC;
        }

        public int sanitize(Tree precheck)
        {
            int result = 2; // -1 appears if the Type field has not been declared, so it will self-remove
            string identifier = precheck.getElement("Type");
            if (identifier != null)
            {
                switch (identifier) {
                    case "IPv4Address": result = ipaddressCheck(precheck);
                        break;
                    case "USAddress": result = sanitizeUSAddress(precheck);
                        break;
                    case "EmailAddress": result = checkEmail(precheck);
                        break;
                    case "IPv6Address": result = ipv6addressCheck(precheck);
                        break;
                    default: result = 2;  // 2 = no sanitizer exists so allowing
                        break;
                }
            }
            return result;
        }

        int ipv6addressCheck(Tree precheck)
        {
            Boolean valid = true;
            int result = -1;

            if (precheck.getElement("Type") == "IPv6Address")
            {
                //  Examples of stuff that's not helpful:  a::e a:: ::e ::1 (localhost)

                string tmpString = precheck.getElement("Match");
                if (tmpString.Length < 5)
                {
                    valid = false;  // Too short to be useful
                }
            }

            if (!valid)
            {
                result = -1;
            }
            else
            {
                result = 1;
            }

            return result;
        }

        int ipaddressCheck(Tree precheck)
        {
            Boolean valid = true;
            int result = -1;

            if (precheck.getElement("Type") == "IPv4Address")
            {
                string tmpString = precheck.getElement("Match");
                char[] sep = new char[1];
                sep[0] = '.';

                string[] octets = tmpString.Split(sep);

                // First check to see if the ip address has four values

                if (octets.Length != 4) valid = false;

                if (valid)
                {
                    try
                    {
                        //  First sanity check, we attempt to parse the values.  If they don't succeed, it will
                        //  be catched and the IP address check will fail.

                        int oct1 = int.Parse(octets[0]);
                        int oct2 = int.Parse(octets[1]);
                        int oct3 = int.Parse(octets[2]);
                        int oct4 = int.Parse(octets[3]);

                        //  Second, we check that all numbers are no greater than $FF in size (255)

                        if (oct1 > 255) valid = false;
                        if (oct2 > 255) valid = false;
                        if (oct3 > 255) valid = false;
                        if (oct4 > 255) valid = false;

                        //  Third, the last number must not be either 0 or 255.

                        if (oct4 == 255 || oct4 == 0 || oct1 < 10)
                        {
                            valid = false;
                        }
                    }
                    catch (Exception xyz)
                    {
                        valid = false;
                    }
                }
            }

            if (!valid)
            {
                result = -1;
            }
            else
            {
                result = 1;
            }

            return result;
        }

        int sanitizeUSAddress(Tree precheck)
        {
            Boolean valid = true;
            int result = -1;

            Tree current = precheck;

            if (current != null)
            {
                if (current.getElement("Type") == "USAddress")
                {
                    string tmpString = current.getElement("Match");
                    char[] sep = new char[1];
                    sep[0] = ' ';

                    string[] parsed = tmpString.Split(sep);

                    try
                    {
                        // is street address number a real number?

                        string addressnumber = parsed[0];

                        for (int x = 0; x < addressnumber.Length; x++)
                        {
                            char digit = addressnumber[x];
                            if (!Char.IsNumber(digit))
                            {
                                valid = false;
                            }
                        }

                        // too big to be an address?

                        if (valid)
                        {
                            if ((parsed.Length > 10) || (parsed.Length < 3))
                            {
                                valid = false;  // Too many or few parsed elements to be an actual street address
                            }
                        }

                        // are we looking at hexidecimal?  If so, it would look like 81 FF FF or something...

                        if (valid)
                        {
                            if (parsed.Length > 2)
                            {
                                if ((parsed[1].Length == 2) && (parsed[2].Length == 2))
                                {
                                    valid = false;
                                }
                            }
                        }

                        // is the street address 0?

                        if (valid)
                        {
                            if (int.Parse(parsed[0]) == 0)
                            {
                                valid = false;
                            }
                        }

                        // does this contain words that imply this isn't really a street address?

                        if (valid)
                        {
                            for (int streetname = 1; streetname < parsed.Length; streetname++)
                            {
                                switch (parsed[streetname])
                                {
                                    // References to sentance construction

                                    case "for":
                                    case "For":
                                    case "was":
                                    case "not":
                                    case "as":
                                    case "to":
                                    case "and":
                                    case "AND":
                                    case "with":
                                    case "I":
                                    case "an":
                                    case "when":
                                    case "Size":
                                    case "her":
                                    case "his":
                                    case "whom":
                                    case "a":
                                    case "too":
                                    case "if":
                                    case "then":
                                    case "else":
                                    case "based":
                                    case "is":
                                    case "These":
                                    case "these":
                                    case "it":
                                    case "my":
                                    case "My":
                                    case "are":
                                    case "in":
                                    case "all":
                                    case "All":
                                    case "par":
                                    case "or":
                                    case "by":
                                    case "By":
                                    case "since":
                                    case "which":
                                    case "from":
                                    case "already":
                                    case "Not":
                                    case "into":
                                    case "has":
                                    case "depending":
                                    case "accompanying":
                                    case "so":
                                    case "that":
                                    case "adds":
                                    case "make":
                                    case "Type":
                                    case "types":
                                    case "AS":
                                    case "use":
                                    case "disagree":


                                    // Roman numerals are also not street name worthy

                                    case "II":
                                    case "III":
                                    case "IV":
                                    case "V":
                                    case "VI":
                                    case "VII":
                                    case "VIII":
                                    case "IX":
                                    case "X":
                                    case "XI":
                                    case "XII":
                                    case "XIII":

                                    //  References to technology

                                    case "USB":
                                    case "RAM":
                                    case "Megs":
                                    case "MB":
                                    case "Megabytes":
                                    case "Kilobytes":
                                    case "KB":
                                    case "PCI":
                                    case "LPC":
                                    case "Interface":
                                    case "Chipset":
                                    case "SMBus":
                                    case "ProtocolLayer":
                                    case "Tunneling":
                                    case "ARP":
                                    case "Series":
                                    case "SeriesAutomatically":
                                    case "SeriesFax":
                                    case "BorderlessLetter":
                                    case "BorderlessHagaki":
                                    case "cmCustom":
                                    case "ExtraLetter":
                                    case "TransverseLetter":
                                    case "TransverseJapan":
                                    case "RotatedJapan":
                                    case "RotatedPRC":
                                    case "GT":
                                    case "nvd":
                                    case "GTIntegrated":
                                    case "CPU":
                                    case "Firmware":
                                    case "Compatible":
                                    case "ATA":
                                    case "DeviceGEARAspiW":
                                    case "Devicedisk":
                                    case "Enhanced":
                                    case "ServiceShellSvc":
                                    case "UAA":
                                    case "MCE":
                                    case "NVIDIA":
                                    case "GTS":
                                    case "GTX":
                                    case "Miniport":
                                    case "Driver":
                                    case "FilterPnP":
                                    case "Filter":
                                    case "disc":
                                    case "Enabled":
                                    case "enabled":
                                    case "DriverExtended":
                                    case "Database":
                                    case "database":
                                    case "AudioOutlook":
                                    case "Playlist":
                                    case "PlaylistApple":
                                    case "EditionBuild":
                                    case "NGBibliorom":
                                    case "olderAtom":
                                    case "ACM":
                                    case "GC":
                                    case "expirations":
                                    case "ClassVisual":
                                    case "Bitmap":
                                    case "Bitstream":
                                    case "URW":
                                    case "designed":
                                    case "Conexant":
                                    case "indicate":
                                    case "desktop":
                                    case "SCSI":
                                    case "ISDN":
                                    case "TDK":
                                    case "NTT":
                                    case "PC":
                                    case "commented":
                                    case "LSI":
                                    case "Subunit":
                                    case "INF":
                                    case "Realtek":
                                    case "devices":
                                    case "format":
                                    case "SiS":
                                    case "ControlC":
                                    case "MAPI":
                                    case "LibraryC":
                                    case "FATAL":
                                    case "TLB":
                                    case "VoiceAge":
                                    case "Editionwndclass":
                                    case "Edition":
                                    case "Malware":
                                    case "Apps":
                                    case "Files":
                                    case "Document":
                                    case "Signing":
                                    case "Format":
                                    case "Plugin":
                                    case "EditionSync":
                                    case "Renderer":
                                    case "Scheme":
                                    case "Codec":
                                    case "NAT":
                                    case "WSock":
                                    case "WARNING":
                                    case "warning":
                                    case "Warning":
                                    case "Error":
                                    case "CSS":
                                    case "URL":
                                    case "Update":
                                    case "update":
                                    case "SERVICE":
                                    case "service":
                                    case "Please":
                                    case "please":
                                    case "Request":
                                    case "request":
                                    case "DecoderAbout":
                                    case "DecoderAboutC":
                                    case "ScopeC":
                                    case "DithererNero":
                                    case "Configuration":
                                    case "ScopeM":
                                    case "About":
                                    case "about":
                                    case "DMO":
                                    case "DMOC":
                                    case "PropertiesC":
                                    case "Applet":
                                    case "TCPIP":
                                    case "tcpip":
                                    case "Offload":
                                    case "Checksum":
                                    case "NT":
                                    case "DriverPNP":
                                    case "DriverNDIS":
                                    case "PlatformsPnP":
                                    case "CodecXvid":
                                    case "Extra":
                                    case "of":
                                    case "ProviderRSVP":
                                    case "ContentHost":
                                    case "Degrees":
                                    case "FileMovie":
                                    case "EditionWordPad":
                                    case "AutoPlay":
                                    case "RealPlayer":
                                    case "PlayList":
                                    case "IDE":
                                    case "WorkingTreeForm":
                                    case "encoded":
                                    case "Gtk":
                                    case "TreeView":
                                    case "CPANPLUS":
                                    case "Encoding":
                                    case "object":
                                    case "HMAC":
                                    case "EMX":
                                    case "Updates":
                                    case "updates":
                                    case "SGR":
                                    case "smtp":
                                    case "md":
                                    case "db":
                                    case "password":
                                    case "errors":
                                    case "SHA":
                                    case "HTML":
                                    case "API":
                                    case "APIs":
                                    case "handling":
                                    case "Suspected":
                                    case "Identified":
                                    case "PIPE":
                                    case "TERM":
                                    case "copyright":
                                    case "Copyright":
                                    case "COPYRIGHT":
                                    case "notice":
                                    case "document":
                                    case "ImageMagick":
                                    case "specification":
                                    case "methods":
                                    case "International Business":
                                    case "Computing":
                                    case "computing":
                                    case "Red Hat":
                                    case "compression":
                                    case "Opaque Industries":
                                    case "length":
                                    case "operations":
                                    case "The OpenSSL":
                                    case "Trolltech":
                                    case "system header":
                                    case "Epic Games":
                                    case "The Regents":
                                    case "ELF":
                                    case "UNIX":
                                    case "conversion":
                                    case "data":
                                    case "Lucent Technologies":
                                    case "Sun Microsystems":
                                    case "Silicon Graphics":
                                    case "Ajuba Solutions":
                                    case "LOC":
                                    case "IBM Corp":
                                    case "fixes":
                                    case "Script":
                                    case "script":
                                    case "long long":
                                    case "IOCTL":
                                    case "Service Provider":
                                    case "always run":
                                    case "DSC":
                                    case "doc":
                                    case "AM":
                                    case "PM":
                                    case "CACHE":

                                    //  References to network metrics

                                    case "mbps":
                                    case "Mbps":
                                    case "MBPS":
                                    case "Kbps":
                                    case "kbps":
                                    case "KBPS":
                                    case "KBytes":
                                    case "KBits":
                                    case "MBytes":
                                    case "MBits":
                                    case "Byte":
                                    case "byte":
                                    case "Bytes":
                                    case "bytes":
                                    case "bit":
                                    case "bits":
                                    case "Bit":
                                    case "Bits":

                                    // References to time

                                    case "month":
                                    case "Month":
                                    case "MONTH":
                                    case "year":
                                    case "Year":
                                    case "YEAR":
                                    case "Week":
                                    case "week":
                                    case "WEEK":
                                    case "day":
                                    case "Day":
                                    case "DAY":
                                    case "hour":
                                    case "Hour":
                                    case "HOUR":
                                    case "hours":
                                    case "Hours":
                                    case "HOURS":
                                    case "minute":
                                    case "Minute":
                                    case "MINUTE":
                                    case "minutes":
                                    case "Minutes":
                                    case "MINUTES":
                                    case "seconds":
                                    case "Seconds":
                                    case "SECONDS":

                                        valid = false;
                                        break;

                                }
                            }

                            if (valid)
                            {
                                string tmp = parsed[1] + " " + parsed[2];

                                switch (tmp)
                                {
                                    case "Microsoft Office":
                                    case "Component Type":
                                    case "Component Class":
                                    case "Express Edition":
                                    case "Search Scope":
                                    case "Color Ditherer":
                                    case "Audio Encoder":
                                    case "Microsoft Corp":
                                    case "Windows Longhorn":
                                    case "ATI Technologies":
                                    case "Hauppauge Computer":
                                    case "National Semiconductor":
                                    case "Integrated Technology":
                                    case "Pinnacle Systems":
                                    case "Promise Technology":
                                    case "Times New":
                                    case "Second Edition":
                                    case "Third Edition":
                                    case "Fourth Edition":
                                    case "Fifth Edition":
                                    case "Shader Compiler":
                                    case "Books Online":
                                    case "Service Pack":
                                    case "Video Splitter":
                                    case "Transport Information":
                                    case "Video Stream":
                                    case "File System":
                                    case "Free Software":
                                    case "Ada Core":
                                    case "Institut National":
                                    case "lexer type":
                                    case "message digest":
                                    case "Parameterized dialog":
                                    case "Basic functions":
                                    case "module loaded":
                                    case "AUTHOR INFORMATION":
                                    case "SEE ALSO":
                                    case "OVERRIDING CORE":
                                    case "possible values":
                                    case "What distribution":
                                    case "What modules":
                                    case "CLASS METHODS":
                                    case "BUG REPORTS":
                                    case "programs which":
                                    case "unless defined":
                                    case "Your Name":
                                    case "bit ASCII":
                                    case "indicates recursive":
                                    case "The Perl":
                                    case "Python Software":
                                    case "generated from":
                                    case "For Dummies":
                                    case "Canonical Ltd":
                                    case "class hierarchy":
                                    case "The Apache":
                                    case "CrystalClear Software":
                                    case "Read Me":
                                    case "Torrent List":
                                    case "Active Directory":
                                    case "Express Ed":
                                    case "Not Found":
                                    case "Microsoft Corporation":
                                        valid = false;
                                        break;

                                }
                            }

                            // "the" also makes a terrible street name if its comes in the second name slot

                            if (valid)
                            {
                                switch (parsed[2])
                                {
                                    case "the":
                                    case "The":
                                    case "THE":
                                        valid = false;
                                        break;
                                }
                            }
                        }
                    }
                    catch (Exception xyz)
                    {
                        valid = false;
                    }
                }
            }

            if (valid)
            {
                result = 1;
            }
            return result;
        }

        int checkEmail(Tree precheck)
        {
            Boolean valid = true;
            Tree current = precheck;
            int result = -1;

            if (current != null)
            {
                if (current.getElement("Type") == "EmailAddress")
                {
                    string tmpString = current.getElement("Match");
                    char[] sep = new char[2];
                    sep[0] = '.';
                    sep[1] = '@';

                    string[] parsed = tmpString.Split(sep);

                    try
                    {

                        // First, lets make sure the email address belongs to an actual TLD.

                        string TLD = parsed[parsed.Length - 1];
                        TLD = TLD.ToLower();

                        switch (TLD)
                        {
                            case "ac":
                            case "ad":
                            case "ae":
                            case "aero":
                            case "af":
                            case "ag":
                            case "ai":
                            case "al":
                            case "am":
                            case "an":
                            case "ao":
                            case "aq":
                            case "ar":
                            case "arpa":
                            case "as":
                            case "asia":
                            case "at":
                            case "au":
                            case "aw":
                            case "ax":
                            case "az":
                            case "ba":
                            case "bb":
                            case "bd":
                            case "be":
                            case "bf":
                            case "bg":
                            case "bh":
                            case "bi":
                            case "biz":
                            case "bj":
                            case "bl":
                            case "bm":
                            case "bn":
                            case "bo":
                            case "br":
                            case "bs":
                            case "bt":
                            case "bv":
                            case "bw":
                            case "by":
                            case "bz":
                            case "ca":
                            case "cat":
                            case "cc":
                            case "cd":
                            case "cf":
                            case "cg":
                            case "ch":
                            case "ci":
                            case "ck":
                            case "cl":
                            case "cm":
                            case "cn":
                            case "co":
                            case "com":
                            case "coop":
                            case "cr":
                            case "cu":
                            case "cv":
                            case "cx":
                            case "cy":
                            case "cz":
                            case "de":
                            case "dj":
                            case "dk":
                            case "dm":
                            case "do":
                            case "dz":
                            case "ec":
                            case "edu":
                            case "ee":
                            case "eg":
                            case "eh":
                            case "er":
                            case "es":
                            case "et":
                            case "eu":
                            case "fi":
                            case "fj":
                            case "fk":
                            case "fm":
                            case "fo":
                            case "fr":
                            case "ga":
                            case "gb":
                            case "gd":
                            case "ge":
                            case "gf":
                            case "gg":
                            case "gh":
                            case "gi":
                            case "gl":
                            case "gm":
                            case "gn":
                            case "gov":
                            case "gp":
                            case "gq":
                            case "gr":
                            case "gs":
                            case "gt":
                            case "gu":
                            case "gw":
                            case "gy":
                            case "hk":
                            case "hm":
                            case "hn":
                            case "hr":
                            case "ht":
                            case "hu":
                            case "id":
                            case "ie":
                            case "il":
                            case "im":
                            case "in":
                            case "info":
                            case "int":
                            case "io":
                            case "iq":
                            case "ir":
                            case "is":
                            case "it":
                            case "je":
                            case "jm":
                            case "jo":
                            case "jobs":
                            case "jp":
                            case "ke":
                            case "kg":
                            case "kh":
                            case "ki":
                            case "km":
                            case "kn":
                            case "kp":
                            case "kr":
                            case "kw":
                            case "ky":
                            case "kz":
                            case "la":
                            case "lb":
                            case "lc":
                            case "li":
                            case "lk":
                            case "lr":
                            case "ls":
                            case "lt":
                            case "lu":
                            case "lv":
                            case "ly":
                            case "ma":
                            case "mc":
                            case "md":
                            case "me":
                            case "mf":
                            case "mg":
                            case "mh":
                            case "mil":
                            case "mk":
                            case "ml":
                            case "mm":
                            case "mn":
                            case "mo":
                            case "mobi":
                            case "mp":
                            case "mq":
                            case "mr":
                            case "ms":
                            case "mt":
                            case "mu":
                            case "museum":
                            case "mv":
                            case "mw":
                            case "mx":
                            case "my":
                            case "mz":
                            case "na":
                            case "name":
                            case "nc":
                            case "ne":
                            case "net":
                            case "nf":
                            case "ng":
                            case "ni":
                            case "nl":
                            case "no":
                            case "np":
                            case "nr":
                            case "nu":
                            case "nz":
                            case "om":
                            case "org":
                            case "pa":
                            case "pe":
                            case "pf":
                            case "pg":
                            case "ph":
                            case "pk":
                            case "pl":
                            case "pm":
                            case "pn":
                            case "pr":
                            case "pro":
                            case "ps":
                            case "pt":
                            case "pw":
                            case "py":
                            case "qa":
                            case "re":
                            case "ro":
                            case "rs":
                            case "ru":
                            case "rw":
                            case "sa":
                            case "sb":
                            case "sc":
                            case "sd":
                            case "se":
                            case "sg":
                            case "sh":
                            case "si":
                            case "sj":
                            case "sk":
                            case "sl":
                            case "sm":
                            case "sn":
                            case "so":
                            case "sr":
                            case "st":
                            case "su":
                            case "sv":
                            case "sy":
                            case "sz":
                            case "tc":
                            case "td":
                            case "tel":
                            case "tf":
                            case "tg":
                            case "th":
                            case "tj":
                            case "tk":
                            case "tl":
                            case "tm":
                            case "tn":
                            case "to":
                            case "tp":
                            case "travel":
                            case "tt":
                            case "tv":
                            case "tw":
                            case "tz":
                            case "ua":
                            case "ug":
                            case "uk":
                            case "um":
                            case "us":
                            case "uy":
                            case "uz":
                            case "va":
                            case "vc":
                            case "ve":
                            case "vg":
                            case "vi":
                            case "vn":
                            case "vu":
                            case "wf":
                            case "ws":
                                break;

                            default: valid = false;
                                break;
                        }

                    }
                    catch (Exception xyz)
                    {
                        valid = false;
                    }
                }
            }

            if (valid)
            {
                result = 1;
            }
            else
            {
                result = -1;
            }

            return result;
        }
    }
}
