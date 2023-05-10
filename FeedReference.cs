//   Phloz
//   Copyright (C) 2003-2019 Eric Knight

using System.Collections;
using FatumCore;
using PhlozLib;
using PhlozLanguages;
using System;

namespace PhlozLib
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
                            case "PhlozBasic100":
                                runtime = new ContainerPhlozBasic100();
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
