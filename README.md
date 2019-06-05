# BaselTools

a collection of tools for AutoCAD and AutoCAD Architecture

this collection contains:
- OSMIN 
    import maps from the openstreetmap project into AutoCAD

for AutoCAD Architecture:
- CompletionHelper
    fills values based on data from an Excel sheet

- DataFilter
    fills values based on data from an Excel sheet
    
- ExcelReader
    fills values based on data from an Excel sheet
    
- ExcelReaderLog
    like the ExcelReader command, but with more detailed output
    
- ExcelReaderOpen
     open all conected Excel files
     
- -BSTMultiSpaceAdd
     a commandline tool to create AEC-Spaces
     
- BSTSpaceAdd
     a command to create AEC-Spaces
- -SpaceAdd
    a further commandline version of a AEC-Space creation tool

- -BSTSpaceAdd2
    yet another commandline version of a AEC-Space creation tool

- MatchPSDProperty
    to match property set values between AEC objects

- PropertyRenumber
    renumber a property by a polyline

- SelectByProperty
    select AEC objects by a property value

- XMLREPAIR
    repair a project navigator structure if drawings in the file system not appear in the project navigator

for AutoCAD
- RealXrefInsert
    insert all opbjets from an xref into the current drawing
    
- Binsert
    bind and insert drawings by select the xrefs
    
- DetachXref
    detach xrefs by selecting the xref instance
    
- BlockShaker
    poaerfull tool to replace bocks on a exact or nerby position of existing blocks
    
- LayerList
     creates a overview graphic for all existing layer inside of a drawing
     
     
to install this package copy the BaselTools.bundle folder to:
 - General Installation folder: %PROGRAMFILES%\Autodesk\ApplicationPlugins
 
or

 - All Users Profile folders: %ALLUSERSPROFILE%\Autodesk\ApplicationPlugins
 
or

 - User Profile folders: %APPDATA%\Autodesk\ApplicationPlugins
