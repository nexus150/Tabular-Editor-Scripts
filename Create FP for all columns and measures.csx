//Created by Daniel Otykier (@DOtykier)
//Modified by Ricardo Rincón (@nexus150) in order to create the field parameter with all the columns / measures of the model 
//(except for those explicitly excluded in the Objects variable) and add the table name as the final field in the field parameter
//Original SCript By Daniel: https://github.com/TabularEditor/TabularEditor3/issues/541#issuecomment-1129228481 

// Before running the script, select the measures or columns that you
// would like to use as field parameters (hold down CTRL to select multiple
// objects). Also, you may change the name of the field parameter table
// below. NOTE: If used against Power BI Desktop, you must enable unsupported
// features under File > Preferences (TE2) or Tools > Preferences (TE3).
// run this script as many times as you want field parameters, setting the desired name 
// and changing the objects variable depending on whether you want columns or measures
// normally 2 parameters for axes / rows / columns, and 2 for measures, will suffice.
var name = "Axis 1";
//var name = "Axis 2";

// For Measures FP use 
//var objects = Model.AllMeasures;
//var name = "Value 1";
//var name = "Value 2";

// Construct the DAX for the calculated based on all columns(or measures) of the model except except those whose name begins 
//with the name of one of the field parameters we intend to create (and any other table that you want exclude). 
var objects = Model.AllColumns.Where(c => !c.Name.StartsWith("Axis") && !c.Name.StartsWith("Value"));
//For Measures use: var objects = Model.AllMeasures;
var dax = "{\n    " + string.Join(",\n    ", objects.Select((c,i) => string.Format("(\"{0}\", NAMEOF('{1}'[{0}]), {2}, \"{1}\")", c.Name, c.Table.Name, i))) + "\n}";

// Add the calculated table to the model:
var table = Model.AddCalculatedTable(name, dax);

// In TE2 columns are not created automatically from a DAX expression, so 
// we will have to add them manually:
var te2 = table.Columns.Count == 0;
var nameColumn = te2 ? table.AddCalculatedTableColumn(name, "[Value1]") : table.Columns["Value1"] as CalculatedTableColumn;
var fieldColumn = te2 ? table.AddCalculatedTableColumn(name + " Fields", "[Value2]") : table.Columns["Value2"] as CalculatedTableColumn;
var orderColumn = te2 ? table.AddCalculatedTableColumn(name + " Order", "[Value3]") : table.Columns["Value3"] as CalculatedTableColumn;

if(!te2) {
    // Rename the columns that were added automatically in TE3:
    nameColumn.IsNameInferred = false;
    nameColumn.Name = name;
    fieldColumn.IsNameInferred = false;
    fieldColumn.Name = name + " Fields";
    orderColumn.IsNameInferred = false;
    orderColumn.Name = name + " Order";
}
// Set remaining properties for field parameters to work
// See: https://twitter.com/markbdi/status/1526558841172893696
nameColumn.SortByColumn = orderColumn;
nameColumn.GroupByColumns.Add(fieldColumn);
fieldColumn.SortByColumn = orderColumn;
fieldColumn.SetExtendedProperty("ParameterMetadata", "{\"version\":3,\"kind\":2}", ExtendedPropertyType.Json);
fieldColumn.IsHidden = true;
orderColumn.IsHidden = true;
