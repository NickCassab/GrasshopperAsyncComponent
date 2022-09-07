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
    public class Mallard_AirtableUpdateComponent : GH_AsyncComponent
    {

        public override Guid ComponentGuid { get => new Guid("2e24c463-3ca1-4429-8bf1-112262590a05"); }

        protected override System.Drawing.Bitmap Icon { get => Properties.Resources.logo32; }

        public override GH_Exposure Exposure => GH_Exposure.primary;

        public Mallard_AirtableUpdateComponent() : base("Update Airtable Record", "Airtable Update", "Updates an airtable record with a specific ID in a specific table", "Samples", "Async")
        {
            BaseWorker = new MallardAirtableUpdateWorker();
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Refresh?", "B", "Boolean button to refresh solution", GH_ParamAccess.item);
            pManager.AddTextParameter("Base ID", "ID", "ID of Airtable Base", GH_ParamAccess.item);
            pManager.AddTextParameter("App Key", "K", "App Key for Airtable Base", GH_ParamAccess.item);
            pManager.AddTextParameter("Table Name", "T", "Name of Table in Airtable Base", GH_ParamAccess.item);
            pManager.AddGenericParameter("Field Names", "FN", "Field Names of existing Airtable Record", GH_ParamAccess.list);
            pManager.AddGenericParameter("Fields", "F", "new Fields of existing Airtable Record", GH_ParamAccess.list);
            pManager.AddGenericParameter("Records", "R", "Airtable Records to Update", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Error Message", "E", "Error Message string", GH_ParamAccess.item);
            pManager.AddGenericParameter("Out Record", "O", "Out Record Result string", GH_ParamAccess.item);
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

    public class MallardAirtableUpdateWorker : WorkerInstance
    {
        //Variable List
        public string baseID = "";
        public string appKey = "";
        public string tablename = null;
        public Fields fields = new Fields();
        public bool conversion = false;
        public List<AirtableAttachment> attachmentList = new List<AirtableAttachment>();
        public AirtableRecord OutRecord = null;
        public List<String> fieldNameList = new List<string>();
        public List<Object> fieldList = new List<Object>();
        public string stringID = null;
        public AirtableRecord inputRecord = null;

        public string errorMessageString = "No response yet, refresh to try again";
        public string attachmentFieldName = "Name";
        public List<Object> records = new List<object>();
        public string offset = null;
        public IEnumerable<string> fieldsArray = null;
        public string filterByFormula = null;
        public int? maxRecords = null;
        public int? pageSize = null;
        public IEnumerable<Sort> sort = null;
        public string view = "Main View";
        public int b = 1;
        public AirtableListRecordsResponse response;
        public bool toggle = false;
        //

        public MallardAirtableUpdateWorker() : base(null) { }


        public async Task UpdateRecordsMethodAsync(AirtableBase airtableBase)
        {

        }

        public override void DoWork(Action<string, double> ReportProgress, Action Done)
        {
            // 👉 Checking for cancellation!
            if (!toggle) { return; }
            if (CancellationToken.IsCancellationRequested) { return; }

            AirtableBase airtableBase = new AirtableBase(appKey, baseID);
            var output = UpdateRecordsMethodAsync(airtableBase);
            if(output != null)
            {
                output.Wait();
            }
            Done();
        }

        public override WorkerInstance Duplicate() => new MallardAirtableUpdateWorker();

        public override void GetData(IGH_DataAccess DA, GH_ComponentParamServer Params)
        {
            if (!DA.GetData(0, ref toggle)) { return; }
            if (!DA.GetData(1, ref baseID)) { return; }
            if (!DA.GetData(2, ref appKey)) { return; }
            if (!DA.GetData(3, ref tablename)) { return; }
            if (!DA.GetDataList(4, fieldNameList)) { return; }
            if (!DA.GetDataList(5, fieldList)) { return; }
            if (!DA.GetData(6, ref inputRecord)) { return; }
        }

        public override void SetData(IGH_DataAccess DA)
        {
            // 👉 Checking for cancellation!
            if (!toggle) { return; }
            if (CancellationToken.IsCancellationRequested) { return; }
            DA.SetData(0, errorMessageString);
            DA.SetData(1, OutRecord);
            fieldList.Clear();
            fields.FieldsCollection.Clear();
            stringID = null;
            inputRecord = null;
        }
    }

}
