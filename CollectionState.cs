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
using System.Data.SQLite;
using Proliferation.Flows.SearchCore;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using DatabaseAdapters;

namespace Proliferation.Flows
{
    public class CollectionState
    {

        public BaseLabelStats LabelStats = new BaseLabelStats(); // Unlike other stats, these ones are completely disposable.
        public BaseCategoryStats CategoryStats = new BaseCategoryStats();  // Same structure, might as well use the same object.

        public DateTime Started = DateTime.Now;

        public long DocumentCount = 0;
        public long AlarmCount = 0;
        public long EventCount = 0;

        public ArrayList Sources = new ArrayList();
        public ArrayList Forwarders = new ArrayList();
        public ArrayList DocumentColors = new ArrayList();
        public ArrayList Filters = new ArrayList();
        public ArrayList Channels = new ArrayList();
        public ArrayList ChannelFlows = new ArrayList();
        public ArrayList TaskList = new ArrayList();
        public ArrayList Instances = new ArrayList();

        public int LogSources = 0;

        public IntDatabase managementDB = null;
        public SQLiteDatabase statsDB = null;
        public string InstanceUniqueID = "";

        public Config config = null;
        public fatumconfig fatumConfig = null;

        public SearchManager searchSystem = null;
        //private static readonly object SQLiteConnection;

        public BaseFlow logFlow = null;
        public BaseFlow alarmFlow = null;

        public BaseReceiver MasterReceiver = null;

        public CollectionState(fatumconfig FC)
        {
            fatumConfig = FC;
            config = new Config();
            string PreferenceFile = fatumConfig.ConfigDirectory + "\\Settings.xml";
            if (!File.Exists(PreferenceFile))
            {
                // Okay, we might be in Development and pointing at some random directory, if so, let's pick the default one and see.
                PreferenceFile = @"C:\Program Files\Flows\settings.xml";
            }

            config.LoadConfig(fatumConfig.ConfigDirectory + "\\Settings.xml");
        }

        public void LoadState()
        {
            try
            {
                InstanceUniqueID = config.GetProperty("UniqueID");

                // Because Commands represent a moving state and we are restarting, the database should already reflect the target state
                // and the commands are unnnecessary.  I.e., we change the setting first, then inform the service that the settings must be changed
                // as its running because ram and the database are not yet synced.  If LoadState() is called, the service fully syncs to the
                // the setting changes.

                // As an FYI, you shouldn't use Commands as a means to perform non-configuration based activities, create or recycle another API
                // for that.

                BaseCommand.clearCommandsByInstance(managementDB, InstanceUniqueID);

                Instances.Clear();
                Instances = BaseInstance.getInstances(this.managementDB);

                Sources.Clear();
                Sources = BaseSource.loadSources(this.managementDB);
                foreach (BaseSource currentSource in Sources)
                {
                    if (currentSource.Enabled)
                    {
                        currentSource.Services = BaseService.loadServicesBySource(managementDB, currentSource);
                        foreach (BaseService currentService in currentSource.Services)
                        {
                            if (currentService.Enabled)
                            {
                                currentService.Flows = BaseFlow.loadFlowsByServiceEnabledOnly(this, currentService);
                                foreach (BaseFlow currentFlow in currentService.Flows)
                                {
                                    if (currentFlow.Enabled)
                                    {
                                        if (currentService.Receivers.Count == 0)
                                        {
                                            MasterReceiver.activateReceiver(this, currentFlow);
                                        }
                                        else
                                        { 
                                            foreach (ReceiverInterface receiver in currentService.Receivers)
                                            {
                                                if (receiver == null)
                                                {
                                                    MasterReceiver.activateReceiver(this, currentFlow);
                                                }
                                                else
                                                {
                                                    receiver.registerFlow(currentFlow);
                                                }
                                            }
                                        }
                                        
                                        BaseFlow.enableFlow(this, currentFlow);
                                    }
                                }
                            }
                        }
                    }
                }

                Forwarders.Clear();
                Forwarders = BaseForwarder.loadForwarders(this);

                Channels.Clear();
                Channels = BaseChannel.loadChannels(this);

                ChannelFlows.Clear();
                ChannelFlows = ChannelFlow.loadFlows(this);

                Filters.Clear();
                Filters = BaseFilter.loadFilters(this);

                DocumentColors.Clear();
                DocumentColors = DocumentColor.loadColors(this);

                TaskList.Clear();
                TaskList = BaseTask.loadTasksForInstance(this.managementDB, InstanceUniqueID);

                string alarmflowid = config.GetProperty("AlarmFlow");
                string logflowid = config.GetProperty("LogFlow");

                alarmFlow = BaseFlow.locateCachedFlowByUniqueID(alarmflowid, this);
                logFlow = BaseFlow.locateCachedFlowByUniqueID(logflowid, this);

                if (searchSystem != null)
                {
                    searchSystem.closeDatabases();
                    searchSystem = null;
                }
                searchSystem = new SearchCore.SearchManager(fatumConfig, this);

                DateTime lastweek = DateTime.Now;
                lastweek = lastweek.AddHours(7 * (-24));

                
                ArrayList notLoaded = searchSystem.loadDatabases(lastweek);
                archiveOldData(notLoaded);
            }
            catch (Exception xyzzy)
            {
                int xyz = 0;
            }
        }

        public void eventListProcessor()
        {
            ArrayList agenda = new ArrayList();

            foreach (BaseTask current in TaskList)
            {
                if (current.EndOfMonth)
                {
                    if (DateTime.Now.Day == 1)
                    {
                        agenda.Add(current);
                        break;
                    }
                }

                if (current.Sunday && (DateTime.Now.DayOfWeek == DayOfWeek.Sunday))
                {
                    agenda.Add(current);
                    break;
                }

                if (current.Monday && (DateTime.Now.DayOfWeek == DayOfWeek.Monday))
                {
                    agenda.Add(current);
                    break;
                }

                if (current.Tuesday && (DateTime.Now.DayOfWeek == DayOfWeek.Tuesday))
                {
                    agenda.Add(current);
                    break;
                }

                if (current.Wednesday && (DateTime.Now.DayOfWeek == DayOfWeek.Wednesday))
                {
                    agenda.Add(current);
                    break;
                }

                if (current.Thursday && (DateTime.Now.DayOfWeek == DayOfWeek.Thursday))
                {
                    agenda.Add(current);
                    break;
                }

                if (current.Friday && (DateTime.Now.DayOfWeek == DayOfWeek.Friday))
                {
                    agenda.Add(current);
                    break;
                }

                if (current.Saturday && (DateTime.Now.DayOfWeek == DayOfWeek.Saturday))
                {
                    agenda.Add(current);
                    break;
                }
            }
 
            ArrayList agenda2 = new ArrayList();
            foreach (BaseTask current in agenda)
            {
                if (current.Occurence.Hour >= DateTime.Now.Hour)
                {
                    if (current.Occurence.Hour == DateTime.Now.Hour)
                    {
                        if (current.Occurence.Minute >= DateTime.Now.Minute)
                        {
                            agenda2.Add(current);
                        }
                    }
                    else
                    {
                        agenda2.Add(current);
                    }
                }
            }
            agenda.Clear();
            TaskList.Clear();
            TaskList = agenda2;
        }

        public static void getState(CollectionState State, fatumconfig fatumConfig)
        {
            string eventdatabasedir = State.config.GetProperty("DocumentDatabaseDirectory");

            if (!Directory.Exists(eventdatabasedir))
            {
                Directory.CreateDirectory(eventdatabasedir);
            }

            State.managementDB = ToolLibrary.getManagementDatabaseConnection(fatumConfig, State.config);

            string dbdirectory = State.config.GetProperty("StatisticsDirectory");
            if (File.Exists(dbdirectory + "\\statistics.s3db"))
            {
                State.statsDB = new SQLiteDatabase(dbdirectory + "\\statistics.s3db");
            }
            else
            {
                if (!Directory.Exists(dbdirectory))
                {
                    Directory.CreateDirectory(dbdirectory);
                }

                SQLiteConnection.CreateFile(dbdirectory + "\\statistics.s3db");
                State.statsDB = new SQLiteDatabase(dbdirectory + "\\statistics.s3db");
                BaseFlowStatus.defaultSQL(State.statsDB, DatabaseSoftware.SQLite);
            }

            //  Download all observed log sources

            State.LoadState();
        }

        public void archiveOldData(ArrayList oldDatabases)
        {
            Thread archiveThread = new Thread(new ParameterizedThreadStart(archiveDataThread));
            archiveThread.Start(oldDatabases);
        }

        public void archiveDataThread(Object o)
        {
            ArrayList directorylist = (ArrayList)o;

            try
            {
                string destination = config.GetProperty("ArchiveDirectory");
                destination += "\\";
                if (!Directory.Exists(destination))
                {
                    Directory.CreateDirectory(destination);
                }

                foreach (string directory in directorylist)
                {
                    try
                    {
                        DirectoryInfo di = new DirectoryInfo(directory);
                        string shortname = di.Name;
                        string filename = destination + shortname + ".zip";

                        if (!File.Exists(filename))   // If the file already exists, we assume it was a restored archive and won't overwrite it.
                        {
                            FileStream fsOut = File.Create(filename);
                            ZipOutputStream zipStream = new ZipOutputStream(fsOut);

                            zipStream.SetLevel(3); //0-9, 9 being the highest level of compression

                            zipStream.Password = null;  // optional. Null is the same as not setting. Required if using AES.

                            int folderOffset = directory.Length + (directory.EndsWith("\\") ? 0 : 1);

                            CompressFolder(directory, zipStream, folderOffset);

                            zipStream.IsStreamOwner = true; // Makes the Close also Close the underlying stream
                            zipStream.Close();
                        }

                        Directory.Delete(directory, true);
                    }
                    catch (Exception xyz)
                    {
                        System.Console.Out.WriteLine("Error archiving streamed data directory: " + xyz.Message + "; " + xyz.StackTrace);
                    }
                }
                directorylist.Clear();
            }
            catch (Exception xyz)
            {
                if (directorylist!=null)
                {
                    directorylist.Clear();
                }
            }
        }

        // Compresses the files in the nominated folder, and creates a zip file on disk named as outPathname.
        //
        public void CreateSample(string outPathname, string password, string folderName)
        {

            FileStream fsOut = File.Create(outPathname);
            ZipOutputStream zipStream = new ZipOutputStream(fsOut);

            zipStream.SetLevel(3); //0-9, 9 being the highest level of compression

            zipStream.Password = password;  // optional. Null is the same as not setting. Required if using AES.

            // This setting will strip the leading part of the folder path in the entries, to
            // make the entries relative to the starting folder.
            // To include the full path for each entry up to the drive root, assign folderOffset = 0.
            int folderOffset = folderName.Length + (folderName.EndsWith("\\") ? 0 : 1);

            CompressFolder(folderName, zipStream, folderOffset);

            zipStream.IsStreamOwner = true; // Makes the Close also Close the underlying stream
            zipStream.Close();
        }

        // Recurses down the folder structure
        //
        private void CompressFolder(string path, ZipOutputStream zipStream, int folderOffset)
        {

            string[] files = Directory.GetFiles(path);

            foreach (string filename in files)
            {

                FileInfo fi = new FileInfo(filename);

                string entryName = filename.Substring(folderOffset); // Makes the name in zip based on the folder
                entryName = ZipEntry.CleanName(entryName); // Removes drive from name and fixes slash direction
                ZipEntry newEntry = new ZipEntry(entryName);
                newEntry.DateTime = fi.LastWriteTime; // Note the zip format stores 2 second granularity

                // Specifying the AESKeySize triggers AES encryption. Allowable values are 0 (off), 128 or 256.
                // A password on the ZipOutputStream is required if using AES.
                //   newEntry.AESKeySize = 256;

                // To permit the zip to be unpacked by built-in extractor in WinXP and Server2003, WinZip 8, Java, and other older code,
                // you need to do one of the following: Specify UseZip64.Off, or set the Size.
                // If the file may be bigger than 4GB, or you do not need WinXP built-in compatibility, you do not need either,
                // but the zip will be in Zip64 format which not all utilities can understand.
                //   zipStream.UseZip64 = UseZip64.Off;
                newEntry.Size = fi.Length;

                zipStream.PutNextEntry(newEntry);

                // Zip the file in buffered chunks
                // the "using" will close the stream even if an exception occurs
                byte[] buffer = new byte[4096];
                using (FileStream streamReader = File.OpenRead(filename))
                {
                    StreamUtils.Copy(streamReader, zipStream, buffer);
                }
                zipStream.CloseEntry();
            }
            string[] folders = Directory.GetDirectories(path);
            foreach (string folder in folders)
            {
                CompressFolder(folder, zipStream, folderOffset);
            }
        }

        public void updateState()
        {
            ArrayList commands = BaseCommand.loadPendingCommands(managementDB, InstanceUniqueID);
            foreach (BaseCommand command in commands)
            {
                command.performCommand(this);
            }
            commands.Clear();
        }     
    }
}
