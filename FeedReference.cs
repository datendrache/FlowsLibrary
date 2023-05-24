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
using Proliferation.LanguageAdapters;

namespace Proliferation.Flows
{
    public class FlowReference
    {
        public BaseFlow Flow = null;
        public ArrayList Processors = new ArrayList();
        public ArrayList Rules = new ArrayList();
        public ArrayList Workspaces = new ArrayList(); 
        public ArrayList Forwarders = new ArrayList();
        public ArrayList ForwarderLinks = new ArrayList();

        private DocumentEventHandler DocumentArrived;
        private CollectionState state;

        public FlowReference(BaseFlow F, CollectionState State, DocumentEventHandler M)
        {
            Flow = F;
            string FlowID = F.UniqueID;
            DocumentArrived = M;
            state = State;

            ArrayList ProcessorLinks = ProcessLink.loadLinksByFlowID(State.managementDB, Flow.UniqueID);

            foreach (ProcessLink currentLink in ProcessorLinks)
            {
                if (currentLink.FlowID == FlowID)
                {
                    BaseProcessor currentProcessor = BaseProcessor.getProcessorByUniqueID(State.managementDB, currentLink.ProcessID);
                    if (currentProcessor.Enabled.ToLower() == "true")
                    {
                        Processors.Add(currentProcessor);
                        IntLanguage runtime = null;
                        string CompilationOutput = "";

                        switch (currentProcessor.Language)
                        {
                            case "Flowish":
                                runtime = new ContainerFlowish();
                                runtime.initialize(currentProcessor.ProcessCode, out CompilationOutput);
                                runtime.setCallback(new EventHandler(collectExternalDocument));
                                break;
                        }
                        Workspaces.Add(runtime);
                    }
                }
            }

            ProcessorLinks.Clear();
            Rules = BaseRule.loadRules(State, Flow);
            if (Rules.Count == 0)
            {
                System.Console.Out.WriteLine("ERROR:  Flow " + Flow.FlowName + " " + Flow.UniqueID + " has no associated rules.");
            }
            ForwarderLinks = ForwarderLink.loadLinksByFlowID(State.managementDB, Flow.UniqueID);
        }

        ~FlowReference()
        {
            Flow = null;
            if (Processors!=null)
            {
                Processors.Clear();
                Processors = null;
            }
            
            if (Rules!=null)
            {
                Rules.Clear();
                Rules = null;
            }
            
            if (Workspaces!=null)
            {
                Workspaces.Clear();
                Workspaces = null;
            }
            
            if (Forwarders!=null)
            {
                Forwarders.Clear();
                Forwarders = null;
            }

            if (ForwarderLinks!=null)
            {
                ForwarderLinks.Clear();
                ForwarderLinks = null;
            }
            
            DocumentArrived = null;
            state = null;
        }

        public static FlowReference findReference(string FlowID, ArrayList FlowReferences)
        {
            FlowReference result = null;
            foreach (FlowReference current in FlowReferences)
            {
                if (FlowID == current.Flow.UniqueID)
                {
                    result = current;
                    break;
                }
            }
            return result;
        }

        private void collectExternalDocument(Object o, EventArgs e)
        {
            ArrayList document = (ArrayList)o;
            BaseDocument newDocument = new BaseDocument(document, state);
            DocumentEventArgs cbe = new DocumentEventArgs();
            cbe.Document = newDocument;
            DocumentArrived.Invoke(this, cbe);
            document.Clear();
        }
    }
}
