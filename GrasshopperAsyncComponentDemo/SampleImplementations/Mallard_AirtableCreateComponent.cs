using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GrasshopperAsyncComponent;
using System.Windows.Forms;
using AirtableApiClient;
using Newtonsoft.Json;
using System;

namespace GrasshopperAsyncComponentDemo.SampleImplementations
{
    public class Mallard_AirtableCreateComponent : GH_AsyncComponent
    {

        public override Guid ComponentGuid { get => new Guid("ef15ac21-0771-4a3b-af82-08072fc67ec0"); }

        protected override System.Drawing.Bitmap Icon { get => Properties.Resources.logo32; }

        public override GH_Exposure Exposure => GH_Exposure.primary;

        public Mallard_AirtableCreateComponent() : base("Create Airtable Records", "Airtable Create", "Creates a list of Airtable Records.", "Samples", "Async")
        {
            BaseWorker = new MallardAirtableCreateWorker();
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Base ID", "ID", "ID of Airtable Base", GH_ParamAccess.item);
            pManager.AddTextParameter("App Key", "K", "App Key for Airtable Base", GH_ParamAccess.item);
            pManager.AddTextParameter("Table Name", "T", "Name of table in Airtable Base", GH_ParamAccess.item);
            pManager.AddGenericParameter("Field Names", "FN", "Field Names of new Airtable Records", GH_ParamAccess.list);
            pManager.AddGenericParameter("Fields", "F", "Fields of new Airtable Records", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Error Message", "E", "Error Message string", GH_ParamAccess.item);
            pManager.AddGenericParameter("Out Records", "O", "Out Record Result string", GH_ParamAccess.item);
        }

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            Menu_AppendItem(menu, "Cancel", (s, e) =>
            {
                RequestCancellation();
            });
        }
    }

    public class MallardAirtableCreateWorker : WorkerInstance
    {
        //test
        public string baseID = "";
        public string appKey = "";
        public string tablename = "";
        public string stringID = "";
        public string errorMessageString = "Set Refresh Input to 'True'";
        public string attachmentFieldName = "Name";
        public List<Object> records = new List<object>();
        public string offset = "0";
        public List<int> indexList = new List<int>();

        public bool conversion = false;
        public List<AirtableAttachment> attachmentList = new List<AirtableAttachment>();
        public AirtableRecord OutRecord = null;
        public List<Object> fieldList = new List<Object>();
        public List<String> fieldNameList = new List<string>();


        public string filterByFormula = null;
        public int? maxRecords = null;
        public int? pageSize = null;
        public IEnumerable<Sort> sort = null;
        public string view = "";
        public int b = 1;
        public AirtableListRecordsResponse response;
        public int count = 0;
        public bool data = false;

        int TheNthPrime { get; set; } = 100;
        long ThePrime { get; set; } = -1;

        public MallardAirtableCreateWorker() : base(null) { }

        public async Task CreateRecordsMethodAsync(AirtableBase airtableBase)
        {

            if (CancellationToken.IsCancellationRequested) { return; }

            int i = 0;
            Fields[] fields = new Fields[fieldNameList.Count];
            foreach (var index in indexList)
            {
                if (fieldList.ElementAt(index) is Grasshopper.Kernel.Types.GH_String)
                {
                    fields[index] = new Fields();
                    fields[index].AddField(fieldNameList[i], fieldList.ElementAt(index).ToString());

                }
                else if (fieldList.ElementAt(index) is GH_ObjectWrapper)
                {
                    GH_ObjectWrapper wrapper = (GH_ObjectWrapper)fieldList.ElementAt(index);
                    if (wrapper.Value is Newtonsoft.Json.Linq.JArray)
                    {
                        var attList = JsonConvert.DeserializeObject<List<AirtableAttachment>>(wrapper.Value.ToString());
                        fields[i] = new Fields();
                        fields[i].AddField(fieldNameList[i], fieldList.ElementAt(index).ToString());
                    }
                    else
                    {
                        AirtableRecord record = (AirtableRecord)wrapper.Value;
                        string recID = record.Id;
                        string[] recIDs = new string[1];
                        recIDs[0] = recID;
                        fields[i] = new Fields();
                        fields[i].AddField(fieldNameList[i], fieldList.ElementAt(index).ToString());
                    }
                }
                i++;
            }



            Task<AirtableCreateUpdateReplaceMultipleRecordsResponse> task = airtableBase.CreateMultipleRecords(tablename, fields, true);

            AirtableCreateUpdateReplaceMultipleRecordsResponse response = await task;

            task.Wait();
            errorMessageString = task.Status.ToString();

            if (response.Success)
            {
                if (CancellationToken.IsCancellationRequested) { return; }
                errorMessageString = "Success!";//change Error Message to success here
                records.AddRange(response.Records.ToList());
            }
            else if (response.AirtableApiError is AirtableApiException)
            {
                if (CancellationToken.IsCancellationRequested) { return; }
                errorMessageString = response.AirtableApiError.ErrorMessage;
            }
            else
            {
                if (CancellationToken.IsCancellationRequested) { return; }
                errorMessageString = "Unknown error";
            }

        }

        public override void DoWork(Action<string, double> ReportProgress, Action Done)
        {
            // 👉 Checking for cancellation!
            if (CancellationToken.IsCancellationRequested) { return; }
            AirtableBase airtableBase = new AirtableBase(appKey, baseID);
            CreateRecordsMethodAsync(airtableBase).Wait();

            Done();
        }

        public override WorkerInstance Duplicate() => new MallardAirtableCreateWorker();

        public override void GetData(IGH_DataAccess DA, GH_ComponentParamServer Params)
        {
            DA.GetData(0, ref baseID);
            DA.GetData(1, ref appKey);
            DA.GetData(2, ref tablename);
            DA.GetDataList(3, indexList);
            DA.GetDataList(4, fieldNameList);
            DA.GetDataList(5, fieldList);
        }

        public override void SetData(IGH_DataAccess DA)
        {
            // 👉 Checking for cancellation!
            if (CancellationToken.IsCancellationRequested) { return; }
            DA.SetData(0, errorMessageString);
            DA.SetDataList(1, records);
        }
    }

}
