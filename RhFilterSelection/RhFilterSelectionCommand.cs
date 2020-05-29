using System;
using System.Linq;
using System.Collections.Generic;
using Rhino.Commands;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.DocObjects;

namespace mnml
{
    public class RhFilterSelectionCommand : Command
    {
        public override string EnglishName
        {
            get { return "FilterSelection"; }
        }

        protected override Result RunCommand(Rhino.RhinoDoc doc, RunMode mode)
        {
            
            GetObject go = new GetObject();

            // List of Object Types to filter
            List<ObjectType> enumList = new List<ObjectType> {
                ObjectType.Point,
                ObjectType.Curve,
                ObjectType.Extrusion,
                ObjectType.Surface,
                ObjectType.Brep,
                ObjectType.Mesh,
                ObjectType.InstanceReference,
                ObjectType.Light,
                ObjectType.ClipPlane,
                ObjectType.Hatch,
                ObjectType.Annotation,
                ObjectType.ClipPlane,
            };

            string[] enumNames = Enum.GetNames(typeof(ObjectType));
            var enumValues = Enum.GetValues(typeof(ObjectType)).OfType<ObjectType>().ToList();
            List<OptionToggle> options = new List<OptionToggle>();
            List<ObjectType> types = new List<ObjectType>();

            for (int i = 0; i < enumList.Count; i++)
            {
                var value = (ObjectType)enumList[i];
                var index = enumValues.IndexOf(value);

                if (index > -1) {
                    var toggle = new OptionToggle(false, "Off", "On");
                    options.Add(toggle);
                    types.Add(value);
                    go.AddOptionToggle(enumNames[index], ref toggle);
                }
            }

            go.SetCommandPrompt("Select filter geometry types");

            go.GroupSelect = true;
            go.SubObjectSelect = false;
            go.EnableClearObjectsOnEntry(false);
            go.EnableUnselectObjectsOnExit(false);
            go.DeselectAllBeforePostSelect = false;

            var selectedObjectTypes = new List<ObjectType>();

            while(true)
            {
                GetResult res = go.GetMultiple(1, 0);


                // If the action is change in filter, update the filter list.
                if (res == GetResult.Option)
                {
                    selectedObjectTypes.Clear();
                    go.EnablePreSelect(false, true);
                    ObjectType objectTypes = 0;
                    for (var i = 0; i < options.Count; i++)
                    {
                        var type = types[i];
                        if (options[i].CurrentValue)
                        {
                            selectedObjectTypes.Add(type);
                            if (objectTypes == ObjectType.None) { objectTypes = type; } else { objectTypes |= type; }
                        }
                    }
                    go.GeometryFilter = objectTypes;

                    continue;
                }

                else if (res != GetResult.Object)
                    return Result.Cancel;

                if (go.ObjectsWerePreselected)
                {
                    go.EnablePreSelect(false, true);
                    continue;
                }

                break;
            }

            // Select objects that meets the filter criteria
            for (int i = 0; i < go.ObjectCount; i++)
            {
                RhinoObject rhinoObject = go.Object(i).Object();
                if (null != rhinoObject)
                {
                    if (selectedObjectTypes.Contains(rhinoObject.ObjectType))
                    {
                        rhinoObject.Select(true);
                    } else
                    {
                        rhinoObject.Select(false);
                    }
                }
            }
            doc.Views.Redraw();
            return Result.Success;
        }
    }
}
