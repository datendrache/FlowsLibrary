//   Flows Libraries -- Flows Common Classes and Methods
//
//   Copyright (C) 2003-2023 Eric Knight
//   This software is distributed under the GNU Public v3 License
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.

//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.

//   You should have received a copy of the GNU General Public License
//   along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System.Collections;
using Proliferation.Fatum;

namespace Proliferation.Flows;

public class fatumconfig
{
    public static string FatumVersion = "0.9.0 Alpha";
    public string DBDirectory = "c:\\Program Files\\Flows\\Databases\\";
    public string ServerURL = "http://localhost:7777/";
    public string ConfigDirectory = "c:\\Program Files\\Flows\\";
    public string ProgramDirectory = "c:\\Program Files\\Flows\\";
    public string ReportingDirectory = "c:\\Program Files\\Flows\\Reporting\\";
    public string GraphicsDirectory = "c:\\Program Files\\Flows\\Graphics\\";
    public string HtmlDirectory = "c:\\Program Files\\Flows\\html";
    public string TempDirectory = "c:\\Temp";
    public string ServerHost = "master.phloz.com";
    public string ServerMode = "Standalone";
    public string ConnectionString = "";
    public string ServerID = "default";
    public string CloudServer = "cloudmaster.phloz.com";
    public string CloudToken = "123456789";
    public string UniqueFlowsIdentifier = "";
    public Boolean RemoteManagement = false;
    public Tree LogDatabases = new Tree();

    //public Tree Configuration = null;

    public int MAXTHREADPOOLSIZE = 8;

    public string CommandLine = "";
    public string PreferenceFile = "";
    public Boolean DEBUG = false;

    public int MaxFileCache = 20000;
    public int MaxSearchCache = 20000;

    public ArrayList AppList = new ArrayList();

    public Boolean MASTER = true;
    public long expiration = 0;
    public int trafficmax = 0;
    public int flowmax = 0;


    public fatumconfig()
    {
        MAXTHREADPOOLSIZE = Environment.ProcessorCount * 8;
    }

    ~fatumconfig()
    {
        FatumVersion = null;
        DBDirectory = null;
        ServerURL = null;
        ConfigDirectory = null;
        ProgramDirectory = null;
        ReportingDirectory = null;
        GraphicsDirectory = null;
        HtmlDirectory = null;
        TempDirectory = null;
        ServerHost = null;
        ServerMode = null;
        ConnectionString = null;
        ServerID = null;
        CloudServer = null;
        CloudToken = null;
        UniqueFlowsIdentifier = null;

        if (LogDatabases != null)
        {
            LogDatabases.Dispose();
            LogDatabases = null;
        }

        //if (Configuration != null)
        //{
        //    Configuration.dispose();
        //    Configuration = null;
        //}

        CommandLine = null;
        PreferenceFile = null;

        if (AppList != null)
        {
            AppList.Clear();
            AppList = null;
        }
    }

    public Tree loadPreferences()
    {
        Tree result = null;
        string ConfigDirectory = System.Reflection.Assembly.GetExecutingAssembly().Location;

        PreferenceFile = ConfigDirectory + "\\settings.xml";

        if (File.Exists(PreferenceFile))
        {
            // Okay, we might be in Development and pointing at some random directory, if so, let's pick the default one and see.
            PreferenceFile = @"C:\Program Files\Flows\settings.xml";
        }

        if (File.Exists(PreferenceFile))
        {
            result = XMLTree.ReadXML(PreferenceFile);
            TreeDataAccess.WriteXml(PreferenceFile+".bak",result,"Data");

            if (result.GetElement("ManagementDatabase")=="")
            {
                result.SetElement("ManagementDatabase", DBDirectory + "\\Flows");
            }
            if (result.GetElement("DocumentDatabaseDirectory") == "")
            {
                result.SetElement("DocumentDatabaseDirectory", DBDirectory + "\\Flows");
            }
            if (result.GetElement("ArchiveDirectory") == "")
            {
                result.SetElement("ArchiveDirectory", DBDirectory + "\\Archive");
            }
            if (result.GetElement("BackupDirectory") == "")
            {
                result.SetElement("BackupDirectory", DBDirectory + "\\Backups");
            }
            if (result.GetElement("StatisticsDirectory") == "")
            {
                result.SetElement("StatisticsDirectory", DBDirectory + "\\Statistics");
            }
            if (result.GetElement("ServerMode") == "")
            {
                result.SetElement("ServerMode", "Standalone");
            }
            if (result.GetElement("ConnectionString") == "")
            {
                result.SetElement("ConnectionString", "");
            }
            if (result.GetElement("ServerHost") == "")
            {
                result.SetElement("ServerHost", "master.phloz.com");
            }
            if (result.GetElement("UniqueID") == "")
            {
                result.SetElement("UniqueID", "default");
            }
            if (result.GetElement("InstanceName") == "")
            {
                result.SetElement("InstanceName", "default");
            }
            if (result.GetElement("CloudServer") == "")
            {
                result.SetElement("CloudServer", "cloudmaster.phloz.com");
            }
            if (result.GetElement("CloudToken") == "")
            {
                result.SetElement("CloudToken", "123456789");
            }
            if (result.GetElement("RemoteManagement") == "")
            {
                result.SetElement("RemoteManagement", "false");
            }
            if (result.GetElement("UniqueFlowsIdentifier") == "")
            {
                result.SetElement("UniqueFlowsIdentifier", System.Guid.NewGuid().ToString());
            }
            if (result.FindNode("LogDatabases") == null)
            {
                LogDatabases = new Tree();
                LogDatabases.AddElement("Name", "SQLite (Local)");
                LogDatabases.AddElement("Directory", DBDirectory + "\\Flows");
                LogDatabases.AddElement("Type", "SQLite");
                LogDatabases.AddElement("ConnectionString", "None");
            }

            TreeDataAccess.WriteXml(PreferenceFile, result, "Data");
        }
        return result;
    }

    //public string GetSetting(string setting)
    //{
    //    string result = Configuration.GetElement(setting);
    //    if (result == null) result = "";   
    //    return result;
    //}

    public void dispose()
    {

    }
}

