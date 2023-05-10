//   Phloz
//   Copyright (C) 2003-2019 Eric Knight


using System;
using System.Collections.Generic;
using FatumCore;
using System.Text;
using Microsoft.Win32;
using System.IO;

namespace PhlozLib
{
    public class RegTools
    {
        public Tree checkForInterestingInfo(string hive, string path, string valuename, string value)
        {
            Tree result = null;
            string lowervalue = valuename.ToLower();
            Tree newFact = null;
            switch (lowervalue)
            {
                case "account":
                case "username":
                case "user":
                case "logon":
                case "login":
                case "accountid":
                case "userid":
                case "acct":
                case "id":
                case "usr":
                case "usrname":
                case "person":
                case "identification":
                case "ident":
                case "self":
                case "remoteid":
                case "loginid":
                case "logonid":
                case "auth":
                    if (accountprecheck(lowervalue, value))
                    {
                        newFact = new Tree();
                        newFact.addElement("Hive", hive);
                        newFact.addElement("Regkey", path + "\\" + valuename);
                        newFact.addElement("Value", value);
                        newFact.addElement("Type", "AccountID");
                    }
                    break;
                case "password":
                case "passwd":
                case "pass":
                case "pwd":
                case "pw":
                    newFact = new Tree();
                    newFact.addElement("Hive", hive);
                    newFact.addElement("Regkey", path + "\\" + valuename);
                    newFact.addElement("Value", value);
                    newFact.addElement("Type", "Password");
                    break;
                case "userpasswordhint" :
                case "passwdhint" :
                case "pwhint" :
                    newFact = new Tree();
                    newFact.addElement("Hive", hive);
                    newFact.addElement("Regkey", path + "\\" + valuename);
                    newFact.addElement("Value", value);
                    newFact.addElement("Type", "Password Hint");
                    break;
            }
            result = newFact;
            return result;
        }

        //  accountprecheck is used to eliminate accounts that appear to be numbered references rather
        //  than actual user ids.  If a user ID is actually just a number or some combination of hex values
        //  this is going to be noticed later on:
        //     1)  Analysisning will pinpoint specific user references, so this unstructured scan
        //         won't be the last resort.
        //  Also eliminate Object references in this format: {57E71847-D1CE-48A2-A0E5-882CE36BDD42}

        Boolean accountprecheck(string lowervalue, string value)
        {
            Boolean result = true;

            // We can't rule out too much, but if its hex, has a number, and is even, appears in an
            // id field, lets drop it.  Microsoft uses hex IDs everywhere.
            string lowered = lowervalue.ToLower();

            if (lowered == "id")
            {
                Boolean allHex = true;
                Boolean foundNumber = false;

                String hexcase = value.ToLower();
                for (int i = 0; i < hexcase.Length; i++)
                {
                    switch (hexcase[i])
                    {
                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                            foundNumber = true;
                            break;
                        case 'a':
                        case 'b':
                        case 'c':
                        case 'd':
                        case 'e':
                        case 'f':
                        case '{':
                        case '}':
                        case '-':
                            break;
                        default:
                            allHex = false;
                            result = true;
                            i = hexcase.Length;
                            break;
                    }
                }

                //  The logic here is that the name "Eadee" isn't accidentially declared hex,
                //  so if there isn't a number but does look "hexy", the whole thing is approved as
                //  a possible username.

                if (allHex == true)
                {
                    if (foundNumber == true)
                    {
                        result = false;
                    }
                }
            }

            return result;
        }

        public Tree loadRegistryKeyValue(string fullkeyname)
        {
            Tree result = null;
            RegistryKey tmpKey = null;

            string hive = "";
            string key = "";
            int locationindex = fullkeyname.IndexOf("\\");
            if (locationindex < 3)
            {
                return result;
            }
            else
            {
                hive = fullkeyname.Substring(0, locationindex);
                key = fullkeyname.Substring(locationindex + 1);

                if (hive.CompareTo("HKEY_LOCAL_MACHINE") == 0)
                {
                    tmpKey = Registry.LocalMachine;
                }

                if (hive.CompareTo("HKEY_CURRENT_USERS") == 0)
                {
                    tmpKey = Registry.CurrentUser;
                }

                if (hive.CompareTo("HKEY_CLASSES_ROOT") == 0)
                {
                    tmpKey = Registry.ClassesRoot;
                }

                if (hive.CompareTo("HKEY_CURRENT_CONFIG") == 0)
                {
                    tmpKey = Registry.CurrentConfig;
                }

                if (hive.CompareTo("HKEY_USERS") == 0)
                {
                    tmpKey = Registry.Users;
                }

                // SHORTENED

                if (hive.CompareTo("HKLM") == 0)
                {
                    tmpKey = Registry.LocalMachine;
                }

                if (hive.CompareTo("HKCU") == 0)
                {
                    tmpKey = Registry.CurrentUser;
                }

                if (hive.CompareTo("HKCR") == 0)
                {
                    tmpKey = Registry.ClassesRoot;
                }

                if (hive.CompareTo("HKCC") == 0)
                {
                    tmpKey = Registry.CurrentConfig;
                }

                if (hive.CompareTo("HKU") == 0)
                {
                    tmpKey = Registry.Users;
                }

                if (tmpKey != null)
                {
                    char[] sep = new char[1];
                    sep[0] = '\\';
                    string[] tmp = key.Split(sep);
                    Boolean okiedokie = true;
                    Boolean ErrorOccured = false;
                    int tmpLength = tmp.Length - 1;

                    for (int i = 0; i < tmpLength; i++)
                    {
                        if (okiedokie)
                        {
                            try
                            {
                                if (tmpKey == null)
                                {
                                    okiedokie = false;
                                }
                                else
                                {
                                    RegistryKey nav = tmpKey.OpenSubKey(tmp[i]);
                                    tmpKey.Close();
                                    tmpKey = nav;
                                }
                            }
                            catch (IOException xyz)
                            {
                                int xyzzz = 0;
                                okiedokie = false;
                            }
                        }
                    }

                    if (okiedokie && (tmpKey != null))
                    {
                        // Process Key

                        Tree newPrivacyValue = new Tree();
                        string updatedKeyname = key;

                        try
                        {
                            Object tmpValue = (Object)tmpKey.GetValue(tmp[tmp.Length - 1]);
                            string invalue = null;
                            string simplification = null;

                            if (tmpValue != null)
                            {
                                string inType = tmpValue.GetType().ToString();
                                if (inType.CompareTo("System.String") == 0)
                                {
                                    invalue = (string)tmpKey.GetValue(tmp[tmp.Length - 1]);
                                }
                                else
                                {
                                    if (inType.CompareTo("System.Byte[]") == 0)
                                    {
                                        invalue = FatumLib.convertBytesTostring((byte[])tmpKey.GetValue(tmp[tmp.Length - 1]));
                                        simplification = simplifier((byte[])tmpKey.GetValue(tmp[tmp.Length - 1]));
                                    }
                                    else
                                    {
                                        if (inType.CompareTo("System.Int32") == 0)
                                        {
                                            int tmpint = (int)tmpKey.GetValue(tmp[tmp.Length - 1]);
                                            invalue = tmpint.ToString();
                                        }
                                    }
                                }
                            }

                            if (invalue == null)
                            {
                                RegistryKey anothertmp = tmpKey.OpenSubKey(tmp[tmp.Length - 1]);

                                if (anothertmp != null)
                                {
                                    invalue = (string)anothertmp.GetValue("");
                                    updatedKeyname = key + "\\" + tmp[tmp.Length - 1];
                                    anothertmp.Close();
                                }
                                else
                                {
                                    ErrorOccured = true;
                                }
                            }

                            if (!ErrorOccured)
                            {
                                newPrivacyValue.setElement("Hive", hive);
                                newPrivacyValue.setElement("Key", updatedKeyname);
                                if (invalue == null)
                                {
                                    newPrivacyValue.setElement("Value", "(null)");
                                }
                                else
                                {
                                    newPrivacyValue.setElement("Value", invalue);
                                }
                                if (simplification != null)
                                {
                                    if (simplification.CompareTo("") != 0) newPrivacyValue.setElement("Simplification", simplification);
                                }
                                result = newPrivacyValue;
                            }
                        }
                        catch (InvalidCastException xyz)
                        {
                            newPrivacyValue.dispose();
                        }
                    }
                }
                if (tmpKey != null) tmpKey.Close();
            }
            return result;
        }

        public Tree loadRegistryKey(string fullkeyname)
        {
            Tree result = null;
            RegistryKey tmpKey = null;

            string hive = "";
            string key = "";
            int locationindex = fullkeyname.IndexOf("\\");
            if (locationindex < 3)
            {
                return result;
            }
            else
            {
                hive = fullkeyname.Substring(0, locationindex);
                key = fullkeyname.Substring(locationindex + 1);

                if (hive.CompareTo("HKEY_LOCAL_MACHINE") == 0)
                {
                    tmpKey = Registry.LocalMachine;
                }

                if (hive.CompareTo("HKEY_CURRENT_USERS") == 0)
                {
                    tmpKey = Registry.CurrentUser;
                }

                if (hive.CompareTo("HKEY_CLASSES_ROOT") == 0)
                {
                    tmpKey = Registry.ClassesRoot;
                }

                if (hive.CompareTo("HKEY_CURRENT_CONFIG") == 0)
                {
                    tmpKey = Registry.CurrentConfig;
                }

                if (hive.CompareTo("HKEY_USERS") == 0)
                {
                    tmpKey = Registry.Users;
                }

                // SHORTENED

                if (hive.CompareTo("HKLM") == 0)
                {
                    tmpKey = Registry.LocalMachine;
                }

                if (hive.CompareTo("HKCU") == 0)
                {
                    tmpKey = Registry.CurrentUser;
                }

                if (hive.CompareTo("HKCR") == 0)
                {
                    tmpKey = Registry.ClassesRoot;
                }

                if (hive.CompareTo("HKCC") == 0)
                {
                    tmpKey = Registry.CurrentConfig;
                }

                if (hive.CompareTo("HKU") == 0)
                {
                    tmpKey = Registry.Users;
                }

                if (tmpKey != null)
                {
                    char[] sep = new char[1];
                    sep[0] = '\\';
                    string[] tmp = key.Split(sep);
                    Boolean okiedokie = true;
                    int tmpLength = tmp.Length;

                    for (int i = 0; i < tmpLength; i++)
                    {
                        if (okiedokie)
                        {
                            try
                            {
                                if (tmpKey == null)
                                {
                                    okiedokie = false;
                                }
                                else
                                {
                                    RegistryKey nav = tmpKey.OpenSubKey(tmp[i]);
                                    tmpKey.Close();
                                    tmpKey = nav;
                                }
                            }
                            catch (IOException xyz)
                            {
                                okiedokie = false;
                            }
                        }
                    }

                    if (okiedokie && (tmpKey != null))
                    {
                        // Process Key

                        string[] allvalues = tmpKey.GetValueNames();
                        Tree Registry_Privacy = new Tree();

                        for (int i = 0; i < allvalues.Length; i++)
                        {
                            Tree newPrivacyValue = new Tree();

                            try
                            {
                                Object tmpValue = (Object)tmpKey.GetValue(allvalues[i]);
                                string invalue = null;
                                string simplification = null;

                                if (tmpValue != null)
                                {
                                    string inType = tmpValue.GetType().ToString();
                                    if (inType.CompareTo("System.String") == 0)
                                    {
                                        invalue = (string)tmpKey.GetValue(allvalues[i]);
                                    }
                                    else
                                    {
                                        if (inType.CompareTo("System.Byte[]") == 0)
                                        {
                                            invalue = FatumLib.convertBytesTostring((byte[])tmpKey.GetValue(allvalues[i]));
                                            simplification = simplifier((byte[])tmpKey.GetValue(allvalues[i]));
                                        }
                                        else
                                        {
                                            if (inType.CompareTo("System.Int32") == 0)
                                            {
                                                int tmpint = (int)tmpKey.GetValue(allvalues[i]);
                                                invalue = tmpint.ToString();
                                            }
                                        }
                                    }
                                }

                                if (invalue != null)
                                {
                                    newPrivacyValue.setElement("Hive", hive);
                                    newPrivacyValue.setElement("Key", key + "\\" + allvalues[i]);
                                    newPrivacyValue.setElement("Value", invalue);
                                   
                                    if (simplification != null)
                                    {
                                        if (simplification.CompareTo("") != 0) newPrivacyValue.setElement("Simplification", simplification);
                                    }
                                    //newPrivacyValue.setElement("Action", "Investigate");
                                    Registry_Privacy.addNode(newPrivacyValue, "Data");
                                }
                            }
                            catch (InvalidCastException xyz)
                            {
                                newPrivacyValue.dispose();
                            }
                        }
                        result = Registry_Privacy;
                    }
                }
                if (tmpKey != null) tmpKey.Close();
            }
            return result;
        }

        public Tree loadSubkeys(string fullkeyname)
        {
            Tree result = null;
            RegistryKey tmpKey = null;

            string hive = "";
            string key = "";
            int locationindex = fullkeyname.IndexOf("\\");
            if (locationindex < 3)
            {
                return result;
            }
            else
            {
                hive = fullkeyname.Substring(0, locationindex);
                key = fullkeyname.Substring(locationindex + 1);

                if (hive.CompareTo("HKEY_LOCAL_MACHINE") == 0)
                {
                    tmpKey = Registry.LocalMachine;
                }

                if (hive.CompareTo("HKEY_CURRENT_USERS") == 0)
                {
                    tmpKey = Registry.CurrentUser;
                }

                if (hive.CompareTo("HKEY_CLASSES_ROOT") == 0)
                {
                    tmpKey = Registry.ClassesRoot;
                }

                if (hive.CompareTo("HKEY_CURRENT_CONFIG") == 0)
                {
                    tmpKey = Registry.CurrentConfig;
                }

                if (hive.CompareTo("HKEY_USERS") == 0)
                {
                    tmpKey = Registry.Users;
                }

                // SHORTENED

                if (hive.CompareTo("HKLM") == 0)
                {
                    tmpKey = Registry.LocalMachine;
                }

                if (hive.CompareTo("HKCU") == 0)
                {
                    tmpKey = Registry.CurrentUser;
                }

                if (hive.CompareTo("HKCR") == 0)
                {
                    tmpKey = Registry.ClassesRoot;
                }

                if (hive.CompareTo("HKCC") == 0)
                {
                    tmpKey = Registry.CurrentConfig;
                }

                if (hive.CompareTo("HKU") == 0)
                {
                    tmpKey = Registry.Users;
                }

                if (tmpKey != null)
                {
                    char[] sep = new char[1];
                    sep[0] = '\\';
                    string[] tmp = key.Split(sep);
                    Boolean okiedokie = true;
                    int tmpLength = tmp.Length;

                    for (int i = 0; i < tmpLength; i++)
                    {
                        if (okiedokie)
                        {
                            try
                            {
                                if (tmpKey == null)
                                {
                                    okiedokie = false;
                                }
                                else
                                {
                                    RegistryKey nav = tmpKey.OpenSubKey(tmp[i]);
                                    tmpKey.Close();
                                    tmpKey = nav;
                                }
                            }
                            catch (IOException xyz)
                            {
                                okiedokie = false;
                            }
                        }
                    }

                    if (okiedokie && (tmpKey != null))
                    {
                        // Process Key

                        Tree Registry_Privacy = new Tree();

                        try
                        {
                            string[] subkeynames = tmpKey.GetSubKeyNames();
                            for (int x = 0; x < subkeynames.Length; x++)
                            {
                                Tree newsubkey = new Tree();
                                newsubkey.setElement("Name", subkeynames[x]);
                                Registry_Privacy.addNode(newsubkey, "Subkey");
                            }
                        }
                        catch (Exception xyz)
                        {

                        }

                        result = Registry_Privacy;
                    }
                }
                if (tmpKey != null) tmpKey.Close();
            }
            return result;
        }

        private string simplifier(byte[] inbytes)
        {
            string tmpstring = "";

            if (inbytes != null)
            {
                for (int i = 0; i < inbytes.Length; i++)
                {
                    byte tmpByte = inbytes[i];
                    if (tmpByte > 128)
                    {
                        tmpByte = (byte)(tmpByte - 128);
                    }

                    int tmpInt = (int)tmpByte;

                    if (tmpInt > 31)
                    {
                        if (tmpInt != 127)
                        {
                            char convertedChar = (char)tmpInt;
                            tmpstring = tmpstring + convertedChar;
                        }
                    }
                    else
                    { //  We're going to strip out all non-printables
                        if (tmpInt == 10)
                        {
                            char convertedChar = (char)10;
                            tmpstring = tmpstring + convertedChar;
                        }
                        else
                        {
                            if (tmpInt == 13)
                            {
                                char convertedChar = (char)13;
                                tmpstring = tmpstring + convertedChar;
                            }
                        }
                    }
                }
            }
            return tmpstring;
        }
    }
}
