using McTools.Xrm.Connection;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using XrmToolBox.Extensibility;

namespace SFS.Xtbplugin.TableComparer
{
    public partial class MyPluginControl : PluginControlBase
    {
        private Settings mySettings;

        // List of common OOTB fields
        private static readonly HashSet<string> OOTBFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "createdon", "createdby", "modifiedon", "modifiedby", "ownerid", "owningbusinessunit", "owninguser", "owningteam",
            "statecode", "statuscode", "versionnumber", "importsequencenumber", "overriddencreatedon", "transactioncurrencyid",
            "organizationid", "timezoneruleversionnumber", "utcconversiontimezonecode", "processid", "stageid", "entityimage",
            "exchangeRate", "owningteam", "owninguser", "owningbusinessunit", "createdonbehalfby", "modifiedonbehalfby"
        };

        public MyPluginControl()
        {
            InitializeComponent();
            dgvComparison.CellFormatting += dgvComparison_CellFormatting;
            chkHideOOTB.CheckedChanged += chkHideOOTB_CheckedChanged;
        }

        private void MyPluginControl_Load(object sender, EventArgs e)
        {
            // Loads or creates the settings for the plugin
            if (!SettingsManager.Instance.TryLoad(GetType(), out mySettings))
            {
                mySettings = new Settings();
                LogWarning("Settings not found => a new settings file has been created!");
            }
            else
            {
                LogInfo("Settings found and loaded");
            }

            // Load tables into ComboBoxes
            LoadTables();
        }

        private void LoadTables()
        {
            WorkAsync(new WorkAsyncInfo
            {
                Message = "Loading tables...",
                Work = (worker, args) =>
                {
                    var request = new RetrieveAllEntitiesRequest
                    {
                        EntityFilters = EntityFilters.Entity,
                        RetrieveAsIfPublished = true
                    };
                    var response = (RetrieveAllEntitiesResponse)Service.Execute(request);
                    args.Result = response.EntityMetadata
                        .OrderBy(e => e.DisplayName?.UserLocalizedLabel?.Label ?? e.LogicalName)
                        .ToList();
                },
                PostWorkCallBack = (args) =>
                {
                    if (args.Error != null)
                    {
                        MessageBox.Show(args.Error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    var entities = args.Result as List<EntityMetadata>;
                    // Sort alphabetically by logical name
                    entities = entities.OrderBy(e => e.LogicalName).ToList();
                    tsbTable1.ComboBox.DataSource = new BindingSource(entities, null);
                    tsbTable1.ComboBox.DisplayMember = "LogicalName";
                    tsbTable1.ComboBox.ValueMember = "LogicalName";

                    tsbTable2.ComboBox.DataSource = new BindingSource(entities.ToList(), null);
                    tsbTable2.ComboBox.DisplayMember = "LogicalName";
                    tsbTable2.ComboBox.ValueMember = "LogicalName";
                }
            });
        }

        private void tsbCompare_Click(object sender, EventArgs e)
        {
            var entity1 = tsbTable1.ComboBox.SelectedItem as Microsoft.Xrm.Sdk.Metadata.EntityMetadata;
            var entity2 = tsbTable2.ComboBox.SelectedItem as Microsoft.Xrm.Sdk.Metadata.EntityMetadata;
            if (entity1 == null || entity2 == null)
            {
                MessageBox.Show("Please select two tables to compare.");
                return;
            }
            LoadAndCompareTableColumns(entity1.LogicalName, entity2.LogicalName);
        }

        private void LoadAndCompareTableColumns(string logicalName1, string logicalName2)
        {
            bool hideOOTB = chkHideOOTB.Checked;
            WorkAsync(new WorkAsyncInfo
            {
                Message = $"Comparing columns for {logicalName1} and {logicalName2}...",
                Work = (worker, args) =>
                {
                    var request1 = new RetrieveEntityRequest
                    {
                        LogicalName = logicalName1,
                        EntityFilters = EntityFilters.Attributes
                    };
                    var request2 = new RetrieveEntityRequest
                    {
                        LogicalName = logicalName2,
                        EntityFilters = EntityFilters.Attributes
                    };
                    var response1 = (RetrieveEntityResponse)Service.Execute(request1);
                    var response2 = (RetrieveEntityResponse)Service.Execute(request2);
                    var attrs1 = response1.EntityMetadata.Attributes.ToDictionary(a => a.LogicalName);
                    var attrs2 = response2.EntityMetadata.Attributes.ToDictionary(a => a.LogicalName);
                    var allKeys = attrs1.Keys.Union(attrs2.Keys).OrderBy(k => k).ToList();
                    // Filter OOTB fields if requested
                    if (hideOOTB)
                    {
                        string pk1 = logicalName1 + "id";
                        string pk2 = logicalName2 + "id";
                        allKeys = allKeys.Where(k => !OOTBFields.Contains(k) && k != pk1 && k != pk2).ToList();
                    }
                    var rows = new List<dynamic>();
                    foreach (var key in allKeys)
                    {
                        var a1 = attrs1.ContainsKey(key) ? attrs1[key] : null;
                        var a2 = attrs2.ContainsKey(key) ? attrs2[key] : null;
                        // Get size for string, memo, decimal, double, integer, money, etc.
                        string size1 = GetAttributeSize(a1);
                        string size2 = GetAttributeSize(a2);
                        rows.Add(new
                        {
                            Field = key,
                            Table1_Display_Name = a1?.DisplayName?.UserLocalizedLabel?.Label ?? "",
                            Table1_DataType = a1?.AttributeTypeName?.Value ?? "",
                            Table1_Size = size1,
                            Table1_Required = a1?.RequiredLevel?.Value.ToString() ?? "",
                            Separator = "", // Empty separator column
                            Table2_Display_Name = a2?.DisplayName?.UserLocalizedLabel?.Label ?? "",
                            Table2_DataType = a2?.AttributeTypeName?.Value ?? "",
                            Table2_Size = size2,
                            Table2_Required = a2?.RequiredLevel?.Value.ToString() ?? "",
                            OnlyIn = a1 == null ? "Table2" : a2 == null ? "Table1" : (a1.AttributeTypeName?.Value != a2.AttributeTypeName?.Value || a1.RequiredLevel?.Value != a2.RequiredLevel?.Value ? "Diff" : "Both")
                        });
                    }
                    args.Result = rows;
                },
                PostWorkCallBack = (args) =>
                {
                    if (args.Error != null)
                    {
                        MessageBox.Show(args.Error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    var rows = args.Result as List<dynamic>;
                    dgvComparison.DataSource = rows;
                    // Set separator column width
                    if (dgvComparison.Columns.Contains("Separator"))
                    {
                        dgvComparison.Columns["Separator"].Width = 10;
                        dgvComparison.Columns["Separator"].Resizable = DataGridViewTriState.False;
                        dgvComparison.Columns["Separator"].DefaultCellStyle.BackColor = Color.White;
                    }
                    // Highlight differences
                    foreach (DataGridViewRow row in dgvComparison.Rows)
                    {
                        var onlyIn = row.Cells["OnlyIn"].Value?.ToString();
                        if (onlyIn == "Table1")
                            row.DefaultCellStyle.BackColor = Color.LightSalmon;
                        else if (onlyIn == "Table2")
                            row.DefaultCellStyle.BackColor = Color.LightSkyBlue;
                        else if (onlyIn == "Diff")
                            row.DefaultCellStyle.BackColor = Color.Khaki;
                    }
                    dgvComparison.Columns["OnlyIn"].Visible = false;
                }
            });
        }

        private void tsbClose_Click(object sender, EventArgs e)
        {
            CloseTool();
        }

        private void tsbSample_Click(object sender, EventArgs e)
        {
            // The ExecuteMethod method handles connecting to an
            // organization if XrmToolBox is not yet connected
            ExecuteMethod(GetAccounts);
        }

        private void GetAccounts()
        {
            WorkAsync(new WorkAsyncInfo
            {
                Message = "Getting accounts",
                Work = (worker, args) =>
                {
                    args.Result = Service.RetrieveMultiple(new QueryExpression("account")
                    {
                        TopCount = 50
                    });
                },
                PostWorkCallBack = (args) =>
                {
                    if (args.Error != null)
                    {
                        MessageBox.Show(args.Error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    var result = args.Result as EntityCollection;
                    if (result != null)
                    {
                        MessageBox.Show($"Found {result.Entities.Count} accounts");
                    }
                }
            });
        }

        private void dgvComparison_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvComparison.Columns[e.ColumnIndex].Name == "Field")
            {
                e.CellStyle.Font = new Font(dgvComparison.DefaultCellStyle.Font, FontStyle.Bold);
            }
        }

        /// <summary>
        /// This event occurs when the plugin is closed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MyPluginControl_OnCloseTool(object sender, EventArgs e)
        {
            // Before leaving, save the settings
            SettingsManager.Instance.Save(GetType(), mySettings);
        }

        /// <summary>
        /// This event occurs when the connection has been updated in XrmToolBox
        /// </summary>
        public override void UpdateConnection(IOrganizationService newService, ConnectionDetail detail, string actionName, object parameter)
        {
            base.UpdateConnection(newService, detail, actionName, parameter);

            if (mySettings != null && detail != null)
            {
                mySettings.LastUsedOrganizationWebappUrl = detail.WebApplicationUrl;
                LogInfo("Connection has changed to: {0}", detail.WebApplicationUrl);
            }
        }

        private void chkHideOOTB_CheckedChanged(object sender, EventArgs e)
        {
            var entity1 = tsbTable1.ComboBox.SelectedItem as Microsoft.Xrm.Sdk.Metadata.EntityMetadata;
            var entity2 = tsbTable2.ComboBox.SelectedItem as Microsoft.Xrm.Sdk.Metadata.EntityMetadata;
            if (entity1 != null && entity2 != null)
            {
                LoadAndCompareTableColumns(entity1.LogicalName, entity2.LogicalName);
            }
        }

        // Helper to get attribute size as string
        private string GetAttributeSize(AttributeMetadata attr)
        {
            if (attr == null) return "";
            switch (attr.AttributeType)
            {
                case AttributeTypeCode.String:
                    var stringMeta = attr as StringAttributeMetadata;
                    return stringMeta?.MaxLength?.ToString() ?? "";
                case AttributeTypeCode.Memo:
                    var memoMeta = attr as MemoAttributeMetadata;
                    return memoMeta?.MaxLength?.ToString() ?? "";
                case AttributeTypeCode.Decimal:
                    var decMeta = attr as DecimalAttributeMetadata;
                    return decMeta?.MaxValue?.ToString() ?? "";
                case AttributeTypeCode.Double:
                    var dblMeta = attr as DoubleAttributeMetadata;
                    return dblMeta?.MaxValue?.ToString() ?? "";
                case AttributeTypeCode.Integer:
                    var intMeta = attr as IntegerAttributeMetadata;
                    return intMeta?.MaxValue?.ToString() ?? "";
                case AttributeTypeCode.Money:
                    var moneyMeta = attr as MoneyAttributeMetadata;
                    return moneyMeta?.MaxValue?.ToString() ?? "";
                default:
                    return "";
            }
        }
    }
}