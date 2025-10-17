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
using Microsoft.Crm.Sdk.Messages;

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
            "exchangeRate", "owningteam", "owninguser", "owningbusinessunit", "createdonbehalfby", "modifiedonbehalfby",
            // additional related name/yomi and owner/state/status fields to hide when filtering OOTB
            "createdbyname", "createdbyyominame", "createdonbehalfbyname", "createdonbehalfbyyominame",
            "modifiedbyname", "modifiedbyyominame", "modifiedonbehalfbyname", "modifiedonbehalfbyyominame",
            "owneridname", "owneridtype", "owneridyominame", "owningbusinessunitname",
            "statecodename", "statuscodename"
        };

        private bool _isCloning = false;

        public MyPluginControl()
        {
            InitializeComponent();
            LogInfo("MyPluginControl: Constructor executed.");
            dgvComparison.CellFormatting += dgvComparison_CellFormatting;
            chkHideOOTB.CheckedChanged += chkHideOOTB_CheckedChanged;
            dgvComparison.SelectionChanged += dgvComparison_SelectionChanged;
            tsbCloneSelectedColumn.Click += tsbCloneSelectedColumn_Click;
        }

        private void MyPluginControl_Load(object sender, EventArgs e)
        {
            LogInfo("MyPluginControl_Load: Starting load.");
            // Loads or creates the settings for the plugin
            try
            {
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
            catch (Exception ex)
            {
                LogError($"MyPluginControl_Load: Exception: {ex}");
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            LogInfo("MyPluginControl_Load: Finished load.");
        }

        private void LoadTables()
        {
            LogInfo("LoadTables: Starting retrieval of entities.");
            WorkAsync(new WorkAsyncInfo
            {
                Message = "Loading tables...",
                Work = (worker, args) =>
                {
                    try
                    {
                        var request = new RetrieveAllEntitiesRequest
                        {
                            EntityFilters = EntityFilters.Entity,
                            RetrieveAsIfPublished = true
                        };
                        LogInfo("LoadTables.Work: Executing RetrieveAllEntitiesRequest.");
                        var response = (RetrieveAllEntitiesResponse)Service.Execute(request);
                        args.Result = response.EntityMetadata
                            .OrderBy(e => e.DisplayName?.UserLocalizedLabel?.Label ?? e.LogicalName)
                            .ToList();
                    }
                    catch (Exception ex)
                    {
                        LogError($"LoadTables.Work: Exception retrieving entities: {ex}");
                        throw;
                    }
                },
                PostWorkCallBack = (args) =>
                {
                    if (args.Error != null)
                    {
                        LogError($"LoadTables.PostWorkCallBack: Error: {args.Error}");
                        MessageBox.Show(args.Error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    try
                    {
                        var entities = args.Result as List<EntityMetadata>;
                        LogInfo($"LoadTables.PostWorkCallBack: Retrieved {entities?.Count ?? 0} entities.");
                        // Sort alphabetically by logical name
                        entities = entities.OrderBy(e => e.LogicalName).ToList();
                        tsbTable1.ComboBox.DataSource = new BindingSource(entities, null);
                        tsbTable1.ComboBox.DisplayMember = "LogicalName";
                        tsbTable1.ComboBox.ValueMember = "LogicalName";

                        tsbTable2.ComboBox.DataSource = new BindingSource(entities.ToList(), null);
                        tsbTable2.ComboBox.DisplayMember = "LogicalName";
                        tsbTable2.ComboBox.ValueMember = "LogicalName";
                    }
                    catch (Exception ex)
                    {
                        LogError($"LoadTables.PostWorkCallBack: Exception setting UI data: {ex}");
                        MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            });
        }

        private void tsbCompare_Click(object sender, EventArgs e)
        {
            var entity1 = tsbTable1.ComboBox.SelectedItem as Microsoft.Xrm.Sdk.Metadata.EntityMetadata;
            var entity2 = tsbTable2.ComboBox.SelectedItem as Microsoft.Xrm.Sdk.Metadata.EntityMetadata;
            LogInfo($"tsbCompare_Click: Selected Table1={entity1?.LogicalName ?? "(null)"}, Table2={entity2?.LogicalName ?? "(null)"}");
            if (entity1 == null || entity2 == null)
            {
                LogWarning("tsbCompare_Click: Please select two tables to compare.");
                MessageBox.Show("Please select two tables to compare.");
                return;
            }
            LoadAndCompareTableColumns(entity1.LogicalName, entity2.LogicalName);
        }

        private void LoadAndCompareTableColumns(string logicalName1, string logicalName2)
        {
            LogInfo($"LoadAndCompareTableColumns: Comparing columns for {logicalName1} and {logicalName2}.");
            bool hideOOTB = chkHideOOTB.Checked;
            WorkAsync(new WorkAsyncInfo
            {
                Message = $"Comparing columns for {logicalName1} and {logicalName2}...",
                Work = (worker, args) =>
                {
                    try
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
                        LogInfo($"LoadAndCompareTableColumns.Work: Retrieving attributes for {logicalName1} and {logicalName2}.");
                        var response1 = (RetrieveEntityResponse)Service.Execute(request1);
                        var response2 = (RetrieveEntityResponse)Service.Execute(request2);
                        var attrs1 = response1.EntityMetadata.Attributes.ToDictionary(a => a.LogicalName);
                        var attrs2 = response2.EntityMetadata.Attributes.ToDictionary(a => a.LogicalName);
                        LogInfo($"LoadAndCompareTableColumns.Work: Retrieved {attrs1.Count} attributes from {logicalName1}, {attrs2.Count} from {logicalName2}.");
                        var allKeys = attrs1.Keys.Union(attrs2.Keys).OrderBy(k => k).ToList();
                        // Filter OOTB fields if requested
                        if (hideOOTB)
                        {
                            string pk1 = logicalName1 + "id";
                            string pk2 = logicalName2 + "id";
                            var before = allKeys.Count;
                            allKeys = allKeys.Where(k => !OOTBFields.Contains(k) && k != pk1 && k != pk2).ToList();
                            LogInfo($"LoadAndCompareTableColumns.Work: OOTB filter applied. {before} -> {allKeys.Count} fields.");
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
                    }
                    catch (Exception ex)
                    {
                        LogError($"LoadAndCompareTableColumns.Work: Exception: {ex}");
                        throw;
                    }
                },
                PostWorkCallBack = (args) =>
                {
                    if (args.Error != null)
                    {
                        LogError($"LoadAndCompareTableColumns.PostWorkCallBack: Error: {args.Error}");
                        MessageBox.Show(args.Error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    try
                    {
                        var rows = args.Result as List<dynamic>;
                        LogInfo($"LoadAndCompareTableColumns.PostWorkCallBack: Populating DataGridView with {rows?.Count ?? 0} rows.");
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
                        if (dgvComparison.Columns.Contains("OnlyIn"))
                        {
                            dgvComparison.Columns["OnlyIn"].Visible = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError($"LoadAndCompareTableColumns.PostWorkCallBack: Exception: {ex}");
                        MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            });
        }

        private void tsbClose_Click(object sender, EventArgs e)
        {
            LogInfo("tsbClose_Click: Closing tool.");
            CloseTool();
        }

        private void tsbSample_Click(object sender, EventArgs e)
        {
            LogInfo("tsbSample_Click: Executing GetAccounts.");
            // The ExecuteMethod method handles connecting to an
            // organization if XrmToolBox is not yet connected
            ExecuteMethod(GetAccounts);
        }

        private void GetAccounts()
        {
            LogInfo("GetAccounts: Starting account retrieval.");
            WorkAsync(new WorkAsyncInfo
            {
                Message = "Getting accounts",
                Work = (worker, args) =>
                {
                    try
                    {
                        args.Result = Service.RetrieveMultiple(new QueryExpression("account")
                        {
                            TopCount = 50
                        });
                        LogInfo("GetAccounts.Work: Retrieved accounts.");
                    }
                    catch (Exception ex)
                    {
                        LogError($"GetAccounts.Work: Exception: {ex}");
                        throw;
                    }
                },
                PostWorkCallBack = (args) =>
                {
                    if (args.Error != null)
                    {
                        LogError($"GetAccounts.PostWorkCallBack: Error: {args.Error}");
                        MessageBox.Show(args.Error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    var result = args.Result as EntityCollection;
                    if (result != null)
                    {
                        LogInfo($"GetAccounts.PostWorkCallBack: Found {result.Entities.Count} accounts.");
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
            LogInfo("MyPluginControl_OnCloseTool: Saving settings.");
            try
            {
                SettingsManager.Instance.Save(GetType(), mySettings);
            }
            catch (Exception ex)
            {
                LogError($"MyPluginControl_OnCloseTool: Exception saving settings: {ex}");
            }
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
            LogInfo("chkHideOOTB_CheckedChanged: Hide OOTB changed.");
            var entity1 = tsbTable1.ComboBox.SelectedItem as Microsoft.Xrm.Sdk.Metadata.EntityMetadata;
            var entity2 = tsbTable2.ComboBox.SelectedItem as Microsoft.Xrm.Sdk.Metadata.EntityMetadata;
            if (entity1 != null && entity2 != null)
            {
                LoadAndCompareTableColumns(entity1.LogicalName, entity2.LogicalName);
            }
        }

        private void dgvComparison_SelectionChanged(object sender, EventArgs e)
        {
            tsbCloneSelectedColumn.Enabled = false;
            if (dgvComparison.SelectedRows.Count == 1)
            {
                var row = dgvComparison.SelectedRows[0];
                // Only enable if the selected row is only in Table1 (no equivalent in Table2)
                var onlyIn = row.Cells["OnlyIn"].Value?.ToString();
                if (onlyIn == "Table1")
                {
                    tsbCloneSelectedColumn.Enabled = true;
                }
            }
        }

        private void tsbCloneSelectedColumn_Click(object sender, EventArgs e)
        {
            LogInfo("tsbCloneSelectedColumn_Click: Clone button clicked.");

            if (_isCloning)
            {
                LogWarning("tsbCloneSelectedColumn_Click: Clone already in progress.");
                return;
            }

            if (dgvComparison.SelectedRows.Count != 1)
            {
                LogWarning("tsbCloneSelectedColumn_Click: No single row selected.");
                return;
            }

            var row = dgvComparison.SelectedRows[0];
            var onlyIn = row.Cells["OnlyIn"].Value?.ToString();
            if (onlyIn != "Table1")
            {
                LogWarning("tsbCloneSelectedColumn_Click: Selected row is not only in Table1.");
                return; // only allow cloning from Table1 to Table2 when missing in Table2
            }

            // Get source and target logical names
            var sourceEntity = tsbTable1.ComboBox.SelectedItem as EntityMetadata;
            var targetEntity = tsbTable2.ComboBox.SelectedItem as EntityMetadata;
            if (sourceEntity == null || targetEntity == null)
            {
                LogWarning("tsbCloneSelectedColumn_Click: Source or target entity is null.");
                return;
            }

            string fieldLogicalName = row.Cells["Field"].Value?.ToString();
            if (string.IsNullOrEmpty(fieldLogicalName))
            {
                LogWarning("tsbCloneSelectedColumn_Click: Field logical name is null or empty.");
                return;
            }

            // Prevent concurrent clones
            _isCloning = true;
            tsbCloneSelectedColumn.Enabled = false;

            WorkAsync(new WorkAsyncInfo
            {
                Message = $"Cloning column {fieldLogicalName} to {targetEntity.LogicalName}...",
                Work = (worker, args) =>
                {
                    try
                    {
                        LogInfo($"[Clone] RetrieveAttributeRequest: Entity={sourceEntity.LogicalName}, Field={fieldLogicalName}");
                        var req = new RetrieveAttributeRequest
                        {
                            EntityLogicalName = sourceEntity.LogicalName,
                            LogicalName = fieldLogicalName,
                            RetrieveAsIfPublished = true
                        };
                        var resp = (RetrieveAttributeResponse)Service.Execute(req);
                        var attrMeta = resp.AttributeMetadata;

                        LogInfo($"[Clone] Retrieved AttributeMetadata: Type={attrMeta.GetType().Name}, MetadataId={attrMeta.MetadataId}, LogicalName={attrMeta.LogicalName}");

                        // Capture original required level
                        var originalRequiredLevel = attrMeta.RequiredLevel;

                        // Check if attribute already exists on target
                        bool attrExistsOnTarget = false;
                        try
                        {
                            var checkReq = new RetrieveAttributeRequest
                            {
                                EntityLogicalName = targetEntity.LogicalName,
                                LogicalName = fieldLogicalName,
                                RetrieveAsIfPublished = true
                            };
                            var checkResp = (RetrieveAttributeResponse)Service.Execute(checkReq);
                            if (checkResp?.AttributeMetadata != null)
                            {
                                attrExistsOnTarget = true;
                                LogInfo($"[Clone] Attribute already exists on target {targetEntity.LogicalName}.");
                            }
                        }
                        catch (Exception ex)
                        {
                            // If retrieval fails assume attribute does not exist; log for debugging
                            LogInfo($"[Clone] Check target attribute existence threw: {ex.Message} (treated as not existing)");
                        }

                        if (attrExistsOnTarget)
                        {
                            // Signal that we skipped creation because attribute exists
                            args.Result = "AlreadyExists";
                            return;
                        }

                        // Assign new MetadataId to avoid duplicate GUID error
                        attrMeta.MetadataId = Guid.NewGuid();
                        LogInfo($"[Clone] Assigned new MetadataId: {attrMeta.MetadataId}");

                        var createReq = new CreateAttributeRequest
                        {
                            EntityName = targetEntity.LogicalName,
                            Attribute = attrMeta
                        };

                        LogInfo($"[Clone] CreateAttributeRequest: Entity={targetEntity.LogicalName}, AttributeType={attrMeta.GetType().Name}, LogicalName={attrMeta.LogicalName}");

                        var createResp = Service.Execute(createReq);
                        LogInfo("[Clone] Attribute creation succeeded.");

                        // After creation, replicate RequiredLevel if it was set on source
                        try
                        {
                            if (originalRequiredLevel != null && originalRequiredLevel.Value != AttributeRequiredLevel.None)
                            {
                                LogInfo($"[Clone] Attempting to replicate RequiredLevel='{originalRequiredLevel.Value}' on target attribute.");
                                // Retrieve the attribute again after creation to get the correct MetadataId
                                var targetAttrResp = (RetrieveAttributeResponse)Service.Execute(new RetrieveAttributeRequest
                                {
                                    EntityLogicalName = targetEntity.LogicalName,
                                    LogicalName = attrMeta.LogicalName,
                                    RetrieveAsIfPublished = true
                                });
                                var updateAttr = targetAttrResp.AttributeMetadata;
                                updateAttr.RequiredLevel = new AttributeRequiredLevelManagedProperty(originalRequiredLevel.Value);

                                var updateReq = new UpdateAttributeRequest
                                {
                                    EntityName = targetEntity.LogicalName,
                                    Attribute = updateAttr
                                };

                                Service.Execute(updateReq);
                                LogInfo("[Clone] RequiredLevel replicated via UpdateAttributeRequest.");

                                // Publish the customization so the required level takes effect
                                try
                                {
                                    Service.Execute(new PublishAllXmlRequest());
                                    LogInfo("[Clone] Published changes after RequiredLevel update.");
                                }
                                catch (Exception pex)
                                {
                                    LogError($"[Clone] Failed to publish changes: {pex}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogError($"[Clone] Failed to replicate RequiredLevel: {ex}");
                            // Continue without failing the whole operation
                        }

                        args.Result = null;
                    }
                    catch (Exception ex)
                    {
                        // Log and rethrow to surface in PostWorkCallBack via args.Error
                        LogError($"[Clone] Exception: {ex}");
                        throw;
                    }
                },
                PostWorkCallBack = (args) =>
                {
                    try
                    {
                        if (args.Error != null)
                        {
                            LogError($"[Clone] PostWorkCallBack Error: {args.Error}");
                            MessageBox.Show(args.Error.ToString(), "Error cloning column", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else if (args.Result is string s && s == "AlreadyExists")
                        {
                            LogWarning("[Clone] Attribute already exists on target; creation skipped.");
                            ShowInfoNotification("Attribute already exists on target table. Skipped creation.", null, 0);
                        }
                        else
                        {
                            LogInfo("[Clone] Column cloned successfully.");
                            ShowInfoNotification("Column cloned successfully.", null, 0);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError($"[Clone] PostWorkCallBack: Exception: {ex}");
                        MessageBox.Show(ex.ToString(), "Error cloning column", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    // Refresh grid regardless
                    try
                    {
                        LoadAndCompareTableColumns(sourceEntity.LogicalName, targetEntity.LogicalName);
                    }
                    catch (Exception ex)
                    {
                        LogError($"[Clone] Error refreshing grid: {ex}");
                    }

                    // Re-enable cloning
                    _isCloning = false;
                    tsbCloneSelectedColumn.Enabled = true;
                }
            });
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