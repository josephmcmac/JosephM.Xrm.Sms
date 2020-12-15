﻿
//Note object instantiated after function definition

/// <summary>Functions and variables for common interaction with the crm form</summary>
SmsPageUtility = function () {
    var that = this;
    if (typeof (Xrm) == "undefined") {
        Xrm = window.parent.Xrm;
    }
    /********************************
    Constants
    ********************************/
    this.FormMode =
    {
        Create: 1,
        Update: 2,
        ReadOnly: 3,
        Disabled: 4,
        BulkEdit: 6,
        QuickCreate: 5
    },
    this.SaveMode =
    {
        AutoSave: 70
    },
    this.NotificationLevel =
    {
        WARNING: "WARNING",
        ERROR: "ERROR"
    },
    this.FieldRequirementLevel =
    {
        Required: 'required',
        None: 'none',
        Recommended: 'recommended'
    },
    /********************************
    General form IO and behaviour
    ********************************/
    this.GetRecordId = function () {
        return Xrm.Page.data.entity.getId();
    },
    this.GetRecordType = function () {
        return Xrm.Page.data.entity.getEntityName();
    },
    this.OpenEntityForm = function (entityType, id) {
        Xrm.Utility.openEntityForm(entityType, id);
    },
    /// <summary> Disables all the fields in a tab </summary>
    this.DisableTab = function (tabName) {
        var tab = Xrm.Page.ui.tabs.get(tabName);
        tab.sections.forEach(function (section, index) { that.DisableSection(section); });
    },
    this.SaveRecord = function (onSuccess, onError) {
        if (onSuccess == null)
            onSuccess = that.DoNothing;
        if (onError == null)
            onError = that.DoNothing;
        Xrm.Page.data.save().then(onSuccess, onError);
    }
    /// <summary> Disables all the fields in a section </summary>
    this.DisableSection = function (section) {
        var sectionObject;
        if (section instanceof Object)
            sectionObject = section;
        else
            sectionObject = that.GetSection(section);
        try {
            sectionObject.controls.forEach(function (control, index) { control.setDisabled(true); });
        } catch (err) {
        }
    },
    /// <summary>limits the option set field to those index values in the options array</summary>
     this.OnlyDisplayOptions = function (fieldName, options) {
         var allOptions = Xrm.Page.getAttribute(fieldName).getOptions();
         var control = Xrm.Page.getControl(fieldName);
         control.clearOptions();
         var optionOrder = 0;
         for (var i = 0; i < allOptions.length; i++) {
             var intValue = parseInt(allOptions[i].value);
             if (that.ArrayContains(options, intValue) || that.GetFieldValue(fieldName) == intValue) {
                 control.addOption(allOptions[i], optionOrder++);
             }
         }
     },
    /// <summary>refreshes the option set field to include all configured options</summary>
     this.DisplayAllOptions = function (fieldName) {
         var options = Xrm.Page.getAttribute(fieldName).getOptions();
         var control = Xrm.Page.getControl(fieldName);
         control.clearOptions();
         var optionOrder = 0;
         for (var i = 0; i < options.length; i++)
             control.addOption(options[i], optionOrder++);
     },
    /// <summary> Adds the given option to the end of the option set
     this.AddPicklistOption = function (fieldName, value) {
         var options = Xrm.Page.getAttribute(fieldName).getOptions();
         var control = Xrm.Page.getControl(fieldName);
         for (var i = 0; i < options.length; i++)
             if (options[i].value == value) {
                 control.addOption(options[i]);
             }
     },
    /// <summary>clears and hides the field</summary>
     this.ClearAndHide = function (fieldName) {
         that.SetFieldValue(fieldName, null);
         that.SetFieldMandatory(fieldName, false);
         that.SetFieldVisibility(fieldName, false);
     },
    /// <summary>returns the section object with the given name</summary>
     this.GetSection = function (sectionName) {
         var result = null;
         Xrm.Page.ui.tabs.forEach(function (tab, index) {
             tab.sections.forEach(function (section, index) {
                 if (section.getName() == sectionName)
                     result = section;
             });
         });
         return result;
     };
    /// <summary>hides a section and sets all fields contained in it to null</summary>
    this.ClearAndHideSection = function (sectionName) {
        var section = that.GetSection(sectionName);
        section.controls.forEach(function (control, index) {
            try {
                that.SetFieldValue(control.getAttribute().getName());
            } catch (err) {
            }
        });
        section.setVisible(false);
    },
    this.ForceSubmitField = function (fieldName, submit) {
        if (submit == false)
            Xrm.Page.getAttribute(fieldName).setSubmitMode('never');
        else
            Xrm.Page.getAttribute(fieldName).setSubmitMode('always');
    },
    /// <summary>disables a field and if it has changed tells the form to still submit it on save</summary>
    this.SetFieldDisabled = function (fieldName, isDisabled, ignoreReadOnly) {
        var attribute = Xrm.Page.getAttribute(fieldName);
        if (isDisabled == null)
            isDisabled = true;
        var isDirty = attribute.getIsDirty();
        //only change the editability of fields if the user has permissions on it
        if ((that.GetFormType() == that.FormMode.Create && attribute.getUserPrivilege().canCreate)
            || (that.GetFormType() == that.FormMode.Update && attribute.getUserPrivilege().canUpdate)
            || (ignoreReadOnly == true && that.GetFormType() == that.FormMode.ReadOnly && attribute.getUserPrivilege().canUpdate)) {
            {
                var fieldControl = Xrm.Page.getControl(fieldName);
                if (fieldControl != null)
                    fieldControl.setDisabled(isDisabled);
                try {
                    var headerField = Xrm.Page.getControl("header_process_" + fieldName);
                    if (headerField != null)
                        headerField.setDisabled(isDisabled);
                } catch (e) {

                } 
            }
        }
        if (isDirty)
            that.ForceSubmitField(fieldName);
    },
    /// <summary>returns if the user has read access for field</summary>
    this.CanReadField = function (fieldName) {
        return Xrm.Page.getAttribute(fieldName).getUserPrivilege().canRead;
    },
    /// <summary>returns if the field is currently set as mandatory</summary>
    this.GetFieldIsMandatory = function (fieldName, isMandatory) {
        return Xrm.Page.getAttribute(fieldName).getRequiredLevel() == 'required';
    },
    /// <summary>sets a field as mandatory (true) or no requirement level (false)</summary>
    this.SetFieldMandatory = function (fieldName, isMandatory) {
        if (isMandatory) {
            that.SetFieldVisibility(fieldName, true);
            Xrm.Page.getAttribute(fieldName).setRequiredLevel('required');
        } else
            Xrm.Page.getAttribute(fieldName).setRequiredLevel('none');
    },
    /// <summary>sets a fields requirement level and if not none makes the field visible - can input a this.FieldRequirementLevel</summary>
    this.SetFieldRequiredLevel = function (fieldName, level) {
        if (level == that.FieldRequirementLevel.Required || level == that.FieldRequirementLevel.Recommended)
            that.SetFieldVisibility(fieldName, true);
        Xrm.Page.getAttribute(fieldName).setRequiredLevel(level);
    },
    /// <summary>returns the data value of a field</summary>
    this.GetFieldValue = function (fieldName) {
        return Xrm.Page.getAttribute(fieldName).getValue();
    },
    /// <summary>sets the data value of a field and if changing calls the fields RunOnChange method</summary>
    this.SetFieldValue = function (fieldName, value, dontSubmit, dontOnChange) {
        var attribute = Xrm.Page.getAttribute(fieldName);
        if (attribute == null)
            return;

        var preValue = that.GetFieldValue(fieldName);
        if (dontOnChange == null)
            dontOnChange = false;
        //if the value is not actually changing don't bother
        if (preValue == value)
            return;
        //this horrible piece of code is to return if we are setting a lookup and the lookup is not actually changing we don't want to trigger unneccessary logic further down the track (e.g. a server call on change)
        if (preValue != null && value != null && Xrm.Page.getControl(fieldName).getControlType() == 'lookup' && that.GuidsEqual(preValue[0].id, value[0].id) && preValue[0].entityType == value[0].entityType)
            return;
        //set the value then run the onchange method
        attribute.setValue(value);
        if (dontSubmit != null && dontSubmit)
            that.ForceSubmitField(fieldName, false);
        else
            that.ForceSubmitField(fieldName);
        if (!dontOnChange)
            Xrm.Page.getAttribute(fieldName).fireOnChange();
    },
    /// <summary>sets the data value of a lookup field and if changing calls the fields RunOnChange method</summary>
    this.SetLookupField = function (fieldName, type, id, name) {
        if (id == null)
            that.SetFieldValue(fieldName, null);
        else {
            var lookupReference = [];
            lookupReference[0] = {};
            lookupReference[0].id = id;
            lookupReference[0].entityType = type;
            lookupReference[0].name = name;
            that.SetFieldValue(fieldName, lookupReference);
        }
    },
    /// <summary>sets the data value of a lookup field and if changing calls the fields RunOnChange method</summary>
    this.AddActivityParty = function (fieldName, type, id, name) {
        var value = that.GetFieldValue(fieldName);
        if (value == null)
            value = [];
        var indexToAdd = value.length;
        value[indexToAdd] = {};
        value[indexToAdd].id = id;
        value[indexToAdd].entityType = type;
        value[indexToAdd].name = name;
        that.SetFieldValue(fieldName, value);
    },
    /// <summary>returns the selected id value of the lookup field if it has a value else null</summary>
    this.GetLookupId = function (fieldName) {
        var result = null;
        var lookupArray = Xrm.Page.getAttribute(fieldName).getValue();
        if (lookupArray != null && lookupArray.length > 0)
            result = lookupArray[0].id;
        return result;
    },
    this.GetLookupType = function (fieldName) {
        var result = null;
        var lookupArray = Xrm.Page.getAttribute(fieldName).getValue();
        if (lookupArray != null && lookupArray.length > 0)
            result = lookupArray[0].entityType;
        return result;
    },
    /// <summary>if not currently selected removes the option for the specified value from the option set fields avilable values</summary>
    this.RemovePicklistOption = function (fieldName, option) {
        var value = that.GetFieldValue(fieldName);
        if (value != option) {
            that.DoForControls(fieldName, function(c) { c.removeOption(option)});
        }
    },
    /// <summary>sets a field to either visible or hidden</summary>
    this.SetFieldVisibility = function (fieldName, isVisible) {
        that.DoForControls(fieldName, function (c) { c.setVisible(isVisible) });
        var control = Xrm.Page.getControl(fieldName);
    },
    this.AddCustomFilter = function (fieldName, filter) {
        that.DoForControls(fieldName, function (control) { control.addPreSearch(filter); });
    },
    /// <summary>sets a section to either visible or hidden</summary>
    this.DoForControls = function (fieldName, controlFunction) {
        var attribute = Xrm.Page.getAttribute(fieldName);
        if (attribute != null) {
            attribute.controls.forEach(function (control, index) {
                controlFunction(control);
            });
        }
    },
    /// <summary>sets a section to either visible or hidden</summary>
    /// headerFieldName must be prefixed with 'header_'
    this.SetHeaderFieldVisibility = function (headerFieldName, isVisible) {
        if (Xrm.Page.getControl(headerFieldName) != null) {
            Xrm.Page.getControl(headerFieldName).setVisible(isVisible);
        }
    },
    /// <summary>sets a border around the header field</summary>
    this.AddHeaderBorder = function (headerFieldName, isVisible) {
        if (window.parent.document.getElementsByClassName(headerFieldName) != null) {
            var element = window.parent.document.getElementsByClassName(headerFieldName)[0];
            if (isVisible) {
                //element.classList.add("errorheader");
                element.style.backgroundColor = "#ffe0e6";
                element.style.borderTop = "2px solid red";
                element.style.padding = "5px 14px 10px 5px";
            } else {
                //element.classList.remove("errorheader");
                element.style.backgroundColor = null;
                element.style.borderTop = null;
                element.style.padding = null;
            }
        }



    },
    /// <summary>sets a sub grid header to the selected color</summary>
    this.SetSubGridHeaderColor = function (fieldName, isValid) {

        var titleContainer = window.parent.document.getElementById("titleContainer_" + fieldName);
        if (titleContainer != null) {
            if (!isValid) {
                //titleContainer.classList.add("errorsubgridheader");
                titleContainer.style.backgroundColor = "#ffe0e6";
                titleContainer.style.borderTop = "2px solid red";
            }else
            {
                //titleContainer.classList.remove("errorsubgridheader");
                titleContainer.style.backgroundColor = null;
                titleContainer.style.borderTop = null;
            }
        }
    },
    /// <summary>sets a section to either visible or hidden</summary>
    this.SetSectionVisibility = function (sectionName, isVisible) {
        Xrm.Page.ui.tabs.forEach(function (tab, index) {
            tab.sections.forEach(function (section, index) {
                if (section.getName() == sectionName)
                    section.setVisible(isVisible);
            });
        });
    },
    /// <summary>sets a section to either visible or hidden</summary>
    this.SetTabVisibility = function (tabName, isVisible) {
        var tab = Xrm.Page.ui.tabs.get(tabName);
        if(tab != null)
            tab.setVisible(isVisible);
    },
    /// <summary>sets a tabs visibility to be dependant on whether another tab is expanded or callapsed i.e. dependantTab is shown if tabName is expanded</summary>
    this.SetTabCascadingVisibility = function (tabName, dependantTab, additionalVisibilityFunction) {
        var visibilityFunction = function () {
            Xrm.Page.ui.tabs.get(dependantTab).setVisible(Xrm.Page.ui.tabs.get(tabName).getDisplayState() == 'expanded' && (additionalVisibilityFunction == null || additionalVisibilityFunction()));
        };
        Xrm.Page.ui.tabs.get(tabName).add_tabStateChange(visibilityFunction);
    },
    /// <summary>returns the selected name value of the lookup field if it has a value else null</summary>
    this.GetLookupName = function (fieldName) {
        var result = null;
        var lookupArray = Xrm.Page.getAttribute(fieldName).getValue();
        if (lookupArray != null && lookupArray.length > 0)
            result = lookupArray[0].name;
        return result;
    },
    /// <summary>returns the form type - can be comapred with this.FormMode</summary>
    this.GetFormType = function () {
        return Xrm.Page.ui.getFormType();
    },
    /// <summary>returns the form type - can be comapred with this.FormMode</summary>
    this.GetFieldType = function (fieldName) {
        return Xrm.Page.getAttribute(fieldName).getAttributeType();
    },
    /// <summary>pops up an alert message</summary>
    this.PopupMessage = function (message) {
        Xrm.Utility.alertDialog(message, that.DoNothing);
        console.log(message);
    },

    /// <summary>pops up an alert message with confirmation callbacks</summary>
    this.ConfirmMessage = function (message, yesCloseCallback, noCloseCallback) {
        Xrm.Utility.confirmDialog(message, yesCloseCallback, noCloseCallback);
        console.log(message);
    },

    this.GetFieldLabel = function (field) {
        var control = Xrm.Page.getControl(field);
        if(control == null)
            return "[Error in GetFieldLabel " + field + " not on form]";
        return control.getLabel();
    },

    this.GetFieldDisplay = function (field) {
        var attr = Xrm.Page.getAttribute(field);
        if (attr == null)
            return "[Error in GetFieldDisplay " + field + " not on form]";
        if (attr.getText != null)
            return attr.getText();
        return that.GetFieldValue(field);
    },

    this.DoNothing = function () {
    },

    this.BlockSave = function () {
        event.returnValue = false;
    },
    /// <summary>Sets focus to the control given</summary>
    this.SetFocus = function (fieldName) {
        var control = Xrm.Page.ui.controls.get(fieldName);
        control.setFocus();
    },
    /// <summary>NOTE THIS IS DEPENDANT ON CONFIGURATION OF THE LOOKUP (it must allow mulitple views) - Sets the deafult lookup view to a lookup with the configured input</summary>
    this.SetDefaultLookupView = function (fieldName, viewId, entityName, viewDisplayName, fetchXml, layoutXml) {
        that.DoForControls(fieldName, function (c) { c.addCustomView(viewId, entityName, viewDisplayName, fetchXml, layoutXml, true) });
        that.DoForControls(fieldName, function (c) { c.setDefaultView(viewId) });
    },
    /// <summary> Checks a given array of field names if either of them are dirty </summary>
    this.AreAnyFieldsDirty = function (fieldNames) {
        var isDirty = false;
        if (fieldNames != null) {
            for (var i = 0; i < fieldNames.length; i++) {
                if (Xrm.Page.getAttribute(fieldNames[i]).getIsDirty()) return true;
            }
        } else {
            Xrm.Page.data.entity.attributes.forEach(function (attribute, index) {
                if (attribute.getIsDirty()) isDirty = true;
            });
        }
        return isDirty;
    },
    this.SetFormReadOnly = function () {
        Xrm.Page.ui.controls.forEach(function (control, index) {
            try {
                control.setDisabled(true);
            } catch (err) {
            }
        });
    },
    this.SetTabReadOnly = function (tabName) {
        var tab = Xrm.Page.ui.tabs.get(tabName);
        if (tab != null) {
            tab.sections.forEach(function(section, index) {
                section.controls.forEach(function(control, index) {
                    try {
                        control.setDisabled(true);
                    } catch (err) {
                    }
                });
            });
        }
    },
    this.SetGridVisibility = function (gridName, isVisible) {
        //added recursive retry on this because sometimes
        //was throwing error on load i think because hadn't loaded yet
        var tryFunc = function (gridName, isVisible, tryCount) {
            if (tryCount > 9)
                return; //oh well 
            try {
                var control = Xrm.Page.getControl(gridName);
                if (control != null) {
                    control.setVisible(isVisible);
                }
            }
            catch (err) {
                setTimeout(function () { tryFunc(gridName, isVisible, tryCount + 1); }, 1000);
            }
        };
        tryFunc(gridName, isVisible, 0);
    },
    this.GetSubgridSelectedIds = function (gridName){
        var ids = new Array();
        var gridControl = Xrm.Page.getControl(gridName);
        if (gridControl != null) {
            var selectedRows = gridControl.getGrid().getSelectedRows();
            for (var i = 0; i < selectedRows.getLength(); i++) {
                ids.push(selectedRows.get(i).data.entity.getId());
            }
        }
        return ids;
    },
    this.LoadGridData = function (gridName) {
        var control = Xrm.Page.getControl(gridName);
        if(control != null) {
            control.refresh();
        }
    },
    this.AddOnChange = function (fieldName, functionOnChange) {
        Xrm.Page.getAttribute(fieldName).addOnChange(functionOnChange);
    },
    this.AddOnSave = function (functionOnSave) {
        Xrm.Page.data.entity.addOnSave(functionOnSave);
    },
    this.AddNotification = function (message, notificatioLevel, id) {
        Xrm.Page.ui.setFormNotification(message, notificatioLevel, id);
    },
    this.RemoveNotification = function (id) {
        Xrm.Page.ui.clearFormNotification(id);
    },

    this.DisplayLoading = function (message) {
        if (message == null) {
            message = "Please wait while loading";
        }
        if (Xrm != null && Xrm.Utility != null && Xrm.Utility.showProgressIndicator != null) {
            Xrm.Utility.showProgressIndicator(message);
        }
    };

    this.CloseLoading = function () {
        if (Xrm != null && Xrm.Utility != null && Xrm.Utility.closeProgressIndicator != null) {
            Xrm.Utility.closeProgressIndicator();
        }
    };

    /********************************
    Common Logic
    ********************************/
    /// <summary>javascript for running on all forms to load e.g. RunOnChange methods</summary>
    this.CommonForm = function (runOnChange, runOnSave) {
        that.LoadOnChanges(runOnChange);
        if (runOnSave == null)
            return;
        try {
            Xrm.Page.data.entity.addOnSave(runOnSave);
        } catch (err) {
        }
    },
    /// <summary>loads a function RunOnChange(fieldName) to execute for all field on the form - assumes the RunOnChange method will exist</summary>
    this.LoadOnChanges = function (runOnChange) {
        Xrm.Page.data.entity.attributes.forEach(function (attribute, index) { attribute.addOnChange(function () { runOnChange(attribute.getName()); }); });
    },
    /********************************
    Misc. General Functions
    ********************************/
    /// <summary>returns if two guid strings are logically the same (strips parentheses and case before comparison)</summary>
    this.GuidsEqual = function (guid1, guid2) {
        var result;
        if (guid1 == null && guid2 == null)
            result = true;
        else if (guid1 == null && guid2 != null)
            result = false;
        else if (guid1 != null && guid2 == null)
            result = false;
        else {
            result = guid1.replace('{', '').replace('}', '').toLowerCase() == guid2.replace('{', '').replace('}', '').toLowerCase();
        }
        return result;
    },
    /// <summary>returns if the item exists in the array</summary>
    this.ArrayContains = function (array, item) {
        var result = false;
        for (var i in array) {
            if (array[i] == item) {
                result = true;
                break;
            }
        }
        return result;
    },

    this.ReLoadPage = function () {
        Xrm.Page.data.refresh();
    },

    this.OnLoadLoadSubGrids = function () {
        var controls = Xrm.Page.ui.controls.get();
        var numberOfSubgrids = 0;
        for (var i in controls) {
            var control = controls[i];
            if (control.getControlType() == "subgrid") {
                try {
                    numberOfSubgrids++;
                    if (numberOfSubgrids > 4)
                        control.refresh();
                } catch (e) {
                    //The subgrids aren't loaded straight away so if error caught timeout for a period and retry the function
                    setTimeout(that.OnLoadLoadSubGrids, 500);
                    return;
                }
            }
        }
    },
    /// <summary>Fire onchange event for field on load of form</summary>
    this.FireOnChange = function (fieldName) {
        Xrm.Page.getAttribute(fieldName).fireOnChange();
    },

    /// <summary>sets a tab to either collapsed or expanded</summary>
    this.SetTabExpanded = function (tabName, isExpanded) {
        var tab = Xrm.Page.ui.tabs.get(tabName);
        if (tab != null) {
            if (isExpanded) {
                tab.setDisplayState("expanded");

            } else {
                tab.setDisplayState("collapsed");
            }
        }
    },

    this.RefreshWebResource = function(webresourcenames, id) {
        // When the create is just created, the id isn't being refreshed an passed to the iframe.
        // This function will refresh the iframe passing it the new id
        if (webresourcenames.length > 0) {
            for (var i = 0; i < webresourcenames.length; i++) {
                var webresourcename = webresourcenames[i];
                try {
                    var rw = Xrm.Page.getControl(webresourcename).getObject();
                    if (rw.InitParams != null) {
                        var params = rw.InitParams;
                        params = params.replace("id=,type", "id=" + id + ",type");
                        rw.InitParams = params;
                    } else {
                        var rwSrc = Xrm.Page.getControl(webresourcename).getSrc();
                        if (rwSrc.length > 5) {
                            if (rwSrc.substring(rwSrc.length - 3, rwSrc.length) == "id=") {
                                rwSrc = rwSrc + id;
                                Xrm.Page.getControl(webresourcename).setSrc(rwSrc);
                            }
                        }
                    }
                } catch (err) {

                }

            }

        }
    },
    this.GetFieldsInSection = function (sectionName) {
        var fields = new Array();
        var section = that.GetSection(sectionName);
        var controls = section.controls.get();
        controls.forEach(function(attribute, index) {
            try {
                var fieldName = attribute.getAttribute().getName();
                fields.push(fieldName);
            } catch (e) {

            }
        });
        return fields;
        }

    this.SetFieldNotification = function (fieldName, msg) {
        var control = Xrm.Page.ui.controls.get(fieldName);
        control.setNotification(msg);
    }

    this.ClearFieldNotification = function (fieldName) {
        var control = Xrm.Page.ui.controls.get(fieldName);
        control.clearNotification();
    }
    
};

smsPageUtility = new SmsPageUtility();