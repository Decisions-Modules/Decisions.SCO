using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DecisionsFramework.Design.Flow;
using DecisionsSCOrchestrator.SCOService;
using System.Collections;
using System.Data.Services.Client;
using System.Globalization;
using System.Runtime.Serialization;
using SCOModule;

namespace DecisionsSCOrchestrator
{
    [AutoRegisterMethodsOnClass(true, "SC Ochestrator", RegisterForAgents = true)]
    public class SCOrchestratorSteps
    {

        public static SCORunbook[] GetAllRunbooks()
        {
            OrchestratorContext context = GetOrchestratorContext();

            var runbooks = from runbook in context.Runbooks
                           select runbook;

            //foreach (var item in runbooks)
            //{
            //    int index = Array.IndexOf(runbooks.ToArray(), item);
            //}

            List<SCORunbook> rbList = new List<SCORunbook>();
            foreach (Runbook rb in runbooks)
            {
                SCORunbook item = new SCORunbook();
                item.Name = rb.Name;
                item.Id = rb.Id;
                rbList.Add(item);
            }

            return rbList.ToArray();
        }


        public static Guid StartRunbookWithParameters(Guid runbookId, SCORunbookInstanceParameter[] inputParams)
        {
            //Guid runbookId = new Guid(runbookId);

            Hashtable parameterValues = new Hashtable();
            //parameterValues.Add("Param1", "This is the value for Param1.");
            //parameterValues.Add("Param2", "This is the value for Param2.");


            foreach (SCORunbookInstanceParameter inParam in inputParams)
            {
                parameterValues.Add(inParam.Name, inParam.Value);
            }

            OrchestratorContext context = GetOrchestratorContext();

            // Retrieve parameters for the runbook
            var runbookParams = context.RunbookParameters.Where(runbookParam => runbookParam.RunbookId == runbookId && runbookParam.Direction == "In");

            // Configure the XML for the parameters
            StringBuilder parametersXml = new StringBuilder();
            if (runbookParams != null && runbookParams.Count() > 0)
            {
                parametersXml.Append("<Data>");
                foreach (var param in runbookParams)
                {
                    parametersXml.AppendFormat("<Parameter><ID>{0}</ID><Value>{1}</Value></Parameter>", param.Id.ToString("B"), parameterValues[param.Name]);
                }
                parametersXml.Append("</Data>");
            }

            try
            {
                // Create new job and assign runbook Id and parameters.
                Job job = new Job();
                job.RunbookId = runbookId;
                job.Parameters = parametersXml.ToString();

                // Add newly created job.
                context.AddToJobs(job);
                context.SaveChanges();

                return job.Id;
            }
            catch (DataServiceQueryException ex)
            {
                throw new ApplicationException("Error starting runbook.", ex);
            }
        }

        public static string[] GetRunbookInputParams(Guid runbookId)
        {
            OrchestratorContext context = GetOrchestratorContext();

            // Retrieve parameters 
            //var runbookParams = context.RunbookParameters.Where(runbookParam => runbookParam.RunbookId == runbookId);
            var runbookParams = context.RunbookParameters.Where(runbookParam => runbookParam.RunbookId == runbookId && runbookParam.Direction == "In");

            List<string> paramNamesList = new List<string>();

            foreach (RunbookParameter param in runbookParams)
            {
                paramNamesList.Add(param.Name);
            }

            return paramNamesList.ToArray();
        }

        public static SCOJobInstance GetJobDetails(Guid JobId)
        {
            OrchestratorContext context = GetOrchestratorContext();
            var jobId = JobId;

            var job = (from j in context.Jobs
                       where j.Id == jobId
                       select j).FirstOrDefault();

            if (job == null)
            {
                var msg = string.Format(CultureInfo.InvariantCulture, "Job Not Found: ", jobId);
                throw new Exception(msg);
            }

            //var jobOutput = new Dictionary<string, string>();
            List<SCORunbookInstanceParameter> jobOutput = new List<SCORunbookInstanceParameter>();
            //var jobInput = new Dictionary<string, string>();
            List<SCORunbookInstanceParameter> jobInput = new List<SCORunbookInstanceParameter>();

            if (job.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
            {
                // Get the return data
                var instances = context.RunbookInstances.Where(ri => ri.JobId == jobId);

                // For the non-monitor runbook job, there should be only one instance.
                foreach (var instance in instances)
                {
                    if (!instance.Status.Equals("Success", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var outParameters = context.RunbookInstanceParameters.Where(
                        rip => rip.RunbookInstanceId == instance.Id && rip.Direction.Equals("Out", StringComparison.OrdinalIgnoreCase));

                    foreach (var parameter in outParameters)
                    {
                        SCORunbookInstanceParameter newParam = new SCORunbookInstanceParameter();
                        newParam.Name = parameter.Name;
                        newParam.Value = parameter.Value;

                        //jobOutput[parameter.Name] = parameter.Value;
                        jobOutput.Add(newParam);
                    }

                    var inParameters = context.RunbookInstanceParameters.Where(
                        rip => rip.RunbookInstanceId == instance.Id && rip.Direction.Equals("In", StringComparison.OrdinalIgnoreCase));

                    foreach (var parameter in inParameters)
                    {
                        SCORunbookInstanceParameter newParam = new SCORunbookInstanceParameter();
                        newParam.Name = parameter.Name;
                        newParam.Value = parameter.Value;

                        //jobInput[parameter.Name] = parameter.Value;
                        jobInput.Add(newParam);
                    }
                }
            }
            else
            {
                // Get the return data
                var instances = context.RunbookInstances.Where(ri => ri.JobId == jobId);

                foreach (var instance in instances)
                {
                    var inParameters = context.RunbookInstanceParameters.Where(
                            rip => rip.RunbookInstanceId == instance.Id && rip.Direction.Equals("In", StringComparison.OrdinalIgnoreCase));

                    foreach (var parameter in inParameters)
                    {
                        SCORunbookInstanceParameter newParam = new SCORunbookInstanceParameter();
                        newParam.Name = parameter.Name;
                        newParam.Value = parameter.Value;

                        //jobInput[parameter.Name] = parameter.Value;
                        jobInput.Add(newParam);
                    }
                }
            }

            SCOJobInstance jInstance = new SCOJobInstance() { Job = new SCOJob(job), InputParameters = jobInput.ToArray(), OutputParameters = jobOutput.ToArray() };

            return jInstance;
        }

        public static SCOFolder[] GetAllFolders()
        {
            OrchestratorContext context = GetOrchestratorContext();

            var folders = (from f in context.Folders
                           select f);

            //Folder[] allFolders = new Folder[folders.Count()];

            //int i = 0;
            //foreach (Folder f in folders)
            //{
            //    allFolders[i] = f;
            //    i++;
            //}

            //return allFolders;

            SCOFolder[] allFolders = new SCOFolder[folders.Count()];

            int i = 0;
            foreach (Folder f in folders)
            {
                allFolders[i] = new SCOFolder(f);
                i++;
            }

            return allFolders;

        }
        public static SCOJobInstance StopRunbookJob(Guid JobId)
        {
            OrchestratorContext context = GetOrchestratorContext();

            var jobId = JobId;

            var job = (from j in context.Jobs
                       where j.Id == jobId
                       select j).FirstOrDefault();

            if (job == null)
            {
                var msg = string.Format(CultureInfo.InvariantCulture, "Job Not Found: ", jobId);
                throw new Exception(msg);
            }

            List<SCORunbookInstanceParameter> jobOutput = new List<SCORunbookInstanceParameter>();
            List<SCORunbookInstanceParameter> jobInput = new List<SCORunbookInstanceParameter>();

            if (job.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
            {
                // Get the return data
                var instances = context.RunbookInstances.Where(ri => ri.JobId == jobId);

                // For the non-monitor runbook job, there should be only one instance.
                foreach (var instance in instances)
                {
                    if (!instance.Status.Equals("Success", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var outParameters = context.RunbookInstanceParameters.Where(
                        rip => rip.RunbookInstanceId == instance.Id && rip.Direction.Equals("Out", StringComparison.OrdinalIgnoreCase));

                    foreach (var parameter in outParameters)
                    {
                        SCORunbookInstanceParameter newParam = new SCORunbookInstanceParameter();
                        newParam.Name = parameter.Name;
                        newParam.Value = parameter.Value;

                        jobOutput.Add(newParam);
                    }

                    var inParameters = context.RunbookInstanceParameters.Where(
                        rip => rip.RunbookInstanceId == instance.Id && rip.Direction.Equals("In", StringComparison.OrdinalIgnoreCase));

                    foreach (var parameter in inParameters)
                    {
                        SCORunbookInstanceParameter newParam = new SCORunbookInstanceParameter();
                        newParam.Name = parameter.Name;
                        newParam.Value = parameter.Value;

                        jobInput.Add(newParam);
                    }
                }
            }
            else
            {
                // Get the return data
                var instances = context.RunbookInstances.Where(ri => ri.JobId == jobId);

                foreach (var instance in instances)
                {
                    var inParameters = context.RunbookInstanceParameters.Where(
                            rip => rip.RunbookInstanceId == instance.Id && rip.Direction.Equals("In", StringComparison.OrdinalIgnoreCase));

                    foreach (var parameter in inParameters)
                    {
                        SCORunbookInstanceParameter newParam = new SCORunbookInstanceParameter();
                        newParam.Name = parameter.Name;
                        newParam.Value = parameter.Value;

                        jobInput.Add(newParam);
                    }
                }

                // Set <Status> to "Canceled" (this is what stops the Job)
                job.Status = "Canceled";
                context.UpdateObject(job);
                context.SaveChanges();
            }

            SCOJobInstance jInstance = new SCOJobInstance() { Job = new SCOJob(job), InputParameters = jobInput.ToArray(), OutputParameters = jobOutput.ToArray() };

            return jInstance;
        }

        public static SCOJobInstance[] GetRunbookJobInstances(String runbookPath, String runbookName)
        {
            OrchestratorContext context = GetOrchestratorContext();

            ArrayList jInstanceArrayList = new ArrayList();
            SCOJobInstance[] jInstances;

            var runbook = (from rb in context.Runbooks
                           where rb.Path == runbookPath
                           && rb.Name == runbookName
                           select rb).FirstOrDefault();

            if (runbook == null)
            {
                var msg = string.Format(CultureInfo.InvariantCulture, "Runbook Not Found:  ", runbookPath);
                throw new Exception(msg);
            }

            var jobs = (from jb in context.Jobs
                        where jb.RunbookId == runbook.Id
                        select jb);

            if (jobs == null)
            {
                if (runbook == null)
                {
                    var msg = string.Format(CultureInfo.InvariantCulture, "Jobs not found for Runbook:  ", runbookPath);
                    throw new Exception(msg);
                }
            }
            foreach (Job j in jobs)
            {
                jInstanceArrayList.Add(GetJobDetails(j.Id));
            }

            jInstances = new SCOJobInstance[jInstanceArrayList.Count];
            jInstanceArrayList.CopyTo(jInstances);
            return jInstances;
        }
        public static SCORunbook[] GetAllRunbooksInFolder(String FolderPath)
        {
            OrchestratorContext context = GetOrchestratorContext();

            var folder = (from fol in context.Folders
                          where fol.Path == FolderPath
                          select fol).FirstOrDefault();

            if (folder == null)
            {
                var msg = string.Format(CultureInfo.InvariantCulture, "Runbooks Not Found in Folder:  ", FolderPath);
                throw new Exception(msg);
            }

            Guid folderId = folder.Id;

            var runbooks = (from rb in context.Runbooks
                            where rb.FolderId == folderId
                            select rb);

            if (runbooks == null)
            {
                var msg = string.Format(CultureInfo.InvariantCulture, "Runbooks Not Found in Folder:  ", FolderPath);
                throw new Exception(msg);
            }
            SCORunbook[] runbookArray = new SCORunbook[runbooks.Count()];
            int i = 0;
            foreach (Runbook r in runbooks)
            {
                runbookArray[i] = new SCORunbook(r);
                i++;
            }

            return runbookArray;
        }

        public static SCORunbook[] GetAllRunningRunbooks()
        {
            OrchestratorContext context = GetOrchestratorContext();

            Dictionary<Guid, SCORunbook> runbookDict = new Dictionary<Guid, SCORunbook>();
            var jobs = (from jb in context.Jobs
                        where jb.Status == "Running" || jb.Status == "Pending"
                        select jb);

            foreach (Job j in jobs)
            {
                var runbook = (from rb in context.Runbooks
                               where rb.Id == j.RunbookId
                               select rb).FirstOrDefault();

                if (!runbookDict.ContainsKey(runbook.Id)) { runbookDict.Add(runbook.Id, new SCORunbook(runbook)); }
            }

            SCORunbook[] allRunbooks = new SCORunbook[runbookDict.Count];
            runbookDict.Values.CopyTo(allRunbooks, 0);
            return allRunbooks;
        }



        //public static OrchestratorContext GetOrchestratorContext(string serviceURL, string userName, string password, string domain)
        //{
        //    // Path to Orchestrator web service
        //    string serviceRoot = serviceURL;

        //    // Create Orchestrator context
        //    SCOService.OrchestratorContext context = new SCOService.OrchestratorContext(new Uri(serviceRoot));

        //    // Set credentials to default or a specific user.
        //    //context.Credentials = System.Net.CredentialCache.DefaultCredentials;
        //    context.Credentials = new System.Net.NetworkCredential(userName, password, domain);

        //    return context;
        //}

        private static OrchestratorContext GetOrchestratorContext()
        {
            SCOIntegrationSettings settings = new SCOIntegrationSettings();

            settings = SCOIntegrationSettings.GetSettings();

            // Path to Orchestrator web service
            string serviceRoot = settings.SCOServiceUrl;

            // Create Orchestrator context
            SCOService.OrchestratorContext context = new SCOService.OrchestratorContext(new Uri(serviceRoot));

            // Set credentials to a specific user.
            //context.Credentials = System.Net.CredentialCache.DefaultCredentials;
            context.Credentials = new System.Net.NetworkCredential(settings.SCOServerUsername, settings.SCOServerUserPassword, settings.SCODomain);

            return context;
        }
    }

    [DataContract]
    public class SCOJob
    {
        [DataMember]
        public Guid Id { get; set; }
        [DataMember]
        public Guid RunbookId { get; set; }
        [DataMember]
        public string CreatedBy { get; set; }
        [DataMember]
        public DateTime CreationTime { get; set; }
        [DataMember]
        public string LastModifiedBy { get; set; }
        [DataMember]
        public DateTime LastModifiedTime { get; set; }
        [DataMember]
        public string Status { get; set; }

        public SCOJob() { }

        public SCOJob(Job j)
        {
            Id = j.Id;
            RunbookId = j.RunbookId;
            CreatedBy = j.CreatedBy;
            CreationTime = j.CreationTime;
            LastModifiedBy = j.LastModifiedBy;
            LastModifiedTime = j.LastModifiedTime;
            Status = j.Status;
        }
    }

    [DataContract]
    public class SCOJobInstance
    {
        [DataMember]
        public SCOJob Job { get; set; }
        [DataMember]
        //public Dictionary<string, string> InputParameters { get; set; }
        public SCORunbookInstanceParameter[] InputParameters { get; set; }
        [DataMember]
        //public Dictionary<string, string> OutputParameters { get; set; }
        public SCORunbookInstanceParameter[] OutputParameters { get; set; }

        public SCOJobInstance() { }
    }

    [DataContract]
    public class SCORunbook
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public Guid Id { get; set; }

        public SCORunbook() { }

        public SCORunbook(Runbook rb)
        {
            Name = rb.Name;
            Id = rb.Id;
        }
    }

    [DataContract]
    public class SCORunbookInstanceParameter
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Value { get; set; }

        public SCORunbookInstanceParameter() { }
    }

    [DataContract]
    public class SCOFolder
    {
        [DataMember]
        public Guid Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string CreateBy { get; set; }
        [DataMember]
        public DateTime CreationTime { get; set; }
        [DataMember]
        public string LastModifiedBy { get; set; }
        [DataMember]
        public DateTime LastModifiedTime { get; set; }
        [DataMember]
        public string Path { get; set; }

        public SCOFolder() { }

        public SCOFolder(Folder folder)
        {
            Id = folder.Id;
            Name = folder.Name;
            CreateBy = folder.CreatedBy;
            CreationTime = folder.CreationTime;
            LastModifiedBy = folder.LastModifiedBy;
            LastModifiedTime = folder.LastModifiedTime;
            Path = folder.Path;
        }
    }
}
