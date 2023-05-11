using System;
using System.Net;
using System.Threading;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.IO;
using FatumCore;
using System.Diagnostics;

namespace PhlozLib
{
    public class WebmodStatus : ModuleInterface
    {
        CollectionState State = null;

        public void Init(CollectionState state)
        {
            State = state;
        }

        public void SendRequest(HttpListenerContext httpContext)
        {
            SendResponse(httpContext);
        }

        public Boolean IsHandler(string uri)
        {
            Boolean result = false;

            char[] sep = { '/' };
            string[] request = uri.Split(sep);

            if (request.Length > 1)
                {
                    if (request[1] == "Status")
                    {
                        result = true;
                    }
                }

            return result;
        }

        public void SendResponse(HttpListenerContext context)
        {
            try
            {
                char[] sep = { '/' };
                string[] request = context.Request.RawUrl.Split(sep);
                byte[] msg = null;

                if (request.Length > 1)
                {
                    if (request[1] == "Status")
                    {
                        if (request.Length > 2)
                        {
                            if (request[2] == "General")
                            {
                                string channellist = getGeneralStatus(State);
                                msg = Encoding.ASCII.GetBytes(channellist);
                            }
                        }
                        else
                        {
                            string channellist = getGeneralStatus(State);
                            msg = Encoding.ASCII.GetBytes(channellist);
                        }
                    }
                }

                context.Response.ContentLength64 = msg.Length;
                using (Stream s = context.Response.OutputStream)
                    s.Write(msg, 0, msg.Length);
            }
            catch (Exception xyzzy)
            {
                byte[] msg = null;
                msg = Encoding.ASCII.GetBytes(StaticHTMLLibrary.errorMessage(1, xyzzy.Message + "\n\n" + xyzzy.StackTrace)); 
                context.Response.ContentLength64 = msg.Length;
                using (Stream s = context.Response.OutputStream)
                    s.Write(msg, 0, msg.Length);
            }
        }

        public static string getGeneralStatus(CollectionState State)
        {
            Tree status = new Tree();
            status.setElement("FlowCount", BaseFlow.countActiveFlows(State).ToString());
            status.setElement("ChannelCount", State.Channels.Count.ToString());
            status.setElement("Started", State.Started.Ticks.ToString());
            status.setElement("DocumentCount", State.DocumentCount.ToString());
            status.setElement("CPUStatus", State.DocumentCount.ToString());
            status.addNode(getMemoryStatus(State), "MemoryStatus");
            status.addNode(getDriveSpace(State), "DriveStatus");
            string generalStatus = TreeDataAccess.writeTreeToXMLString(status, "Status");
            status.dispose();
            return generalStatus;
        }

        public static string getCPUStatus(CollectionState State)
        {
            PerformanceCounter perfmon = new PerformanceCounter("Processor", "% Processor Time", "_Total", true);
            int cpuuse = Convert.ToInt32(perfmon.NextValue());
            cpuuse = Convert.ToInt32(perfmon.NextValue());
            return cpuuse.ToString();
        }

        public static Tree getMemoryStatus(CollectionState State)
        {
            PerformanceCounter memmon = new PerformanceCounter("Memory", "Available MBytes", true);
            Tree memoryStatus = new Tree();

            int memavailable = Convert.ToInt32(memmon.NextValue());
            Microsoft.VisualBasic.Devices.ComputerInfo cinfo = new Microsoft.VisualBasic.Devices.ComputerInfo();
            ulong availablememory = cinfo.AvailablePhysicalMemory / (1024 * 1024);
            ulong maxmemory = cinfo.TotalPhysicalMemory / (1024 * 1024);
            ulong usedmemory = maxmemory - availablememory;

            memoryStatus.addElement("Used", usedmemory.ToString());
            memoryStatus.addElement("Total", maxmemory.ToString());
            memoryStatus.addElement("Percent", ((int)((double)((double)usedmemory / (double)maxmemory) * 100)).ToString());

            return memoryStatus;
        }

        public static Tree getDriveSpace(CollectionState State)
        {
            Tree driveStatus = new Tree();

            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                Tree currentDrive = new Tree();
                currentDrive.setElement("ID", drive.RootDirectory.FullName.Replace(":", "").Replace("\\", "").Replace(" ", ""));
                long allspace = drive.TotalSize / (1024 * 1024);
                long freespace = drive.TotalFreeSpace / (1024 * 1024);
                long usedspace = allspace - freespace;
                int percent = (int)((double)((double)usedspace / (double)allspace) * 100);
                currentDrive.setElement("Total", allspace.ToString());
                currentDrive.setElement("Free", freespace.ToString());
                currentDrive.setElement("Used", usedspace.ToString());
                currentDrive.setElement("Percent", percent.ToString());
                driveStatus.addNode(currentDrive, drive.RootDirectory.FullName.Replace(":","").Replace("\\","").Replace(" ",""));
            }
            return driveStatus;
        }
    }
}
