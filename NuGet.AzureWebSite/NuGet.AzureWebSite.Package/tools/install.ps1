param($installPath, $toolsPath, $package, $project)
 
if ($host.Version.Major -eq 1 -and $host.Version.Minor -lt 1) 
{ 
    "NOTICE: This package only works with NuGet 2.0 or above. Please update your NuGet install at http://nuget.codeplex.com. Sorry, but you're now in a weird state. Please 'uninstall-package NuGet.AzureWebSite' now."
}
else
{
    $project.Object.References.Remove("System.Data.Services"); 
    $project.Object.References.Remove("System.Data.Services.Client"); 
    $project.Object.References.Add("Microsoft.Data.Services"); 
    $project.Object.References.Add("Microsoft.Data.Services.Client"); 
    $project.Object.References.Add("System.ServiceModel"); 
    $project.Object.References.Add("System.ServiceModel.Activation"); 
    $project.Object.References.Add("System.ServiceModel.Web"); 
	
	foreach ($reference in $project.Object.References)
	{
		if($reference.Name -eq "Microsoft.Data.Services")
		{
			$reference.CopyLocal = $true;
		}
		if($reference.Name -eq "Microsoft.Data.Services.Client")
		{
			$reference.CopyLocal = $true;
		}
	}
}
