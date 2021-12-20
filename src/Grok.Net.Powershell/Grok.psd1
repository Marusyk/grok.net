﻿@{

# Script module or binary module file associated with this manifest
RootModule = if($PSEdition -eq 'Core')
{
    'netstandard2.0\Grok.Net.PowerShell.dll'
}
else # Desktop
{
    'net461\Grok.Net.PowerShell.dll'
}

# Version number of this module.
ModuleVersion = '1.1.0'

# ID used to uniquely identify this module
GUID = '84fc90ab-74f4-4dbf-ade8-478538217cc8'

# Author of this module
Author = 'Mohamed Eddami'

# Company or vendor of this module
CompanyName = ''

# Copyright statement for this module
Copyright = '(c) 2021'

# Description of the functionality provided by this module
Description = 'Grok is a great way to parse unstructured log data into something structured and queryable.'

# Minimum version of the Windows PowerShell engine required by this module
PowerShellVersion = '5.1'

# Name of the Windows PowerShell host required by this module
# PowerShellHostName = ''

# Minimum version of the Windows PowerShell host required by this module
# PowerShellHostVersion = ''

# Minimum version of Microsoft .NET Framework required by this module
# DotNetFrameworkVersion = ''

# Minimum version of the common language runtime (CLR) required by this module
# CLRVersion = ''

# Processor architecture (None, X86, Amd64) required by this module
# ProcessorArchitecture = ''

# Modules that must be imported into the global environment prior to importing this module
# RequiredModules = @()

# Assemblies that must be loaded prior to importing this module
# RequiredAssemblies = @()

# Script files (.ps1) that are run in the caller's environment prior to importing this module.
# ScriptsToProcess = @()

# Type files (.ps1xml) to be loaded when importing this module
# TypesToProcess = @()

# Modules to import as nested modules of the module specified in RootModule/ModuleToProcess
# NestedModules = @()

# Functions to export from this module
FunctionsToExport = @()

# Cmdlets to export from this module
CmdletsToExport = @('Get-Grok')

# Variables to export from this module
VariablesToExport = @()

# Aliases to export from this module
AliasesToExport = @()

# DSC resources to export from this module
# DscResourcesToExport = @()

# List of all modules packaged with this module
# ModuleList = @()

# List of all files packaged with this module
# FileList = @()

# Private data to pass to the module specified in RootModule/ModuleToProcess. This may also contain a PSData hashtable with additional module metadata used by PowerShell.
PrivateData = @{

    PSData = @{

        # Tags applied to this module. These help with module discovery in online galleries.
        Tags = @('Grok', 'Logs','PSEdition_Desktop', 'PSEdition_Core', 'Windows', 'Linux', 'MacOS')

        # A URL to the license for this module.
        LicenseUri = 'https://github.com/Marusyk/grok.net/blob/main/LICENSE'

        # A URL to the main website for this project.
        ProjectUri = 'https://github.com/Marusyk/grok.net'

        # A URL to an icon representing this module.
        IconUri = 'https://github.com/Marusyk/grok.net/raw/main/Grok.png'
    } # End of PSData hashtable

} # End of PrivateData hashtable

# HelpInfo URI of this module
HelpInfoURI = 'https://github.com/Marusyk/grok.net/blob/main/README.md#powershell'

# Default prefix for commands exported from this module. Override the default prefix using Import-Module -Prefix.
# DefaultCommandPrefix = ''

}