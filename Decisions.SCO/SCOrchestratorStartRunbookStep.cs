using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DecisionsFramework.Design.ConfigurationStorage.Attributes;
using DecisionsFramework.Design.Flow;
using DecisionsFramework.Design.Flow.Interface;
using DecisionsFramework.Design.Flow.Mapping;
using DecisionsFramework.Design.Properties;

namespace DecisionsSCOrchestrator
{
    [AutoRegisterStep("Start Runbook", "SC Ochestrator")]
    [Writable]
    class SCOrchestratorStartRunbookStep : ISyncStep, IDataConsumer, IDataProducer, INotifyPropertyChanged
    {
        private SCORunbook[] AvailableRunbooks;

        [WritableValue]
        private string selectedRunbook;

        public ResultData Run(StepStartData data)
        {
            List<SCORunbookInstanceParameter> rbInputParams = new List<SCORunbookInstanceParameter>();

            foreach (DataDescription inData in InputData)
            {
                rbInputParams.Add(new SCORunbookInstanceParameter { Name = inData.Name, Value = data.Data[inData.Name] as string });
            }
            
            Guid JobId = SCOrchestratorSteps.StartRunbookWithParameters(Guid.Parse(selectedRunbook), rbInputParams.ToArray());

            Dictionary<string, object> resultData = new Dictionary<string, object>();
            resultData.Add("Job Id", JobId);

            return new ResultData("Done", resultData);
        }

        public OutcomeScenarioData[] OutcomeScenarios
        {
            get
            {
                List<DataDescription> outputData = new List<DataDescription>();

                outputData.Add(new DataDescription(typeof(Guid), "Job Id"));

                return new OutcomeScenarioData[] { new OutcomeScenarioData("Done", outputData.ToArray()), };
            }
        }

        private void InitializeStep()
        {
           AvailableRunbooks = SCOrchestratorSteps.GetAllRunbooks();
        }

        public DataDescription[] InputData
        {
            get
            {
                List<DataDescription> dd = new List<DataDescription>();

                if (!string.IsNullOrEmpty(selectedRunbook))
                {
                    Guid selectedRunbookId = Guid.Empty;

                    foreach (SCORunbook rb in AvailableRunbooks)
                    {
                        if (selectedRunbook == rb.Name)
                        {
                            selectedRunbookId = rb.Id;
                            break;
                        }
                      
                    }

                    if (selectedRunbookId != Guid.Empty)
                    {
                        string[] rbInputParams = SCOrchestratorSteps.GetRunbookInputParams(selectedRunbookId);

                        foreach (string param in rbInputParams)
                        {
                            dd.Add(new DataDescription(typeof(string), param));
                        }
                    }
                    else
                    {
                        throw new Exception("Unable to fetch inputs for selected Runbook");
                    }
                }

                return dd.ToArray();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        [SelectStringEditor("AvailableRunbookNames", SelectStringEditorType.DropdownList)]
        public string SelectedRunbook
        {
            get { return selectedRunbook; }
            set
            {
                selectedRunbook = value;
                this.OnPropertyChanged("SelectedRunbook");
                this.OnPropertyChanged("InputData");
            }
        }

        [PropertyHidden(true)]
        private string[] AvailableRunbookNames
        {
            get
            {
                InitializeStep();
                List<string> runbookNames = new List<string>();
                foreach (SCORunbook rb in AvailableRunbooks)
                {
                    runbookNames.Add(rb.Name);
                }
                return runbookNames.ToArray();
            }

            set { }
        }

    }
}
