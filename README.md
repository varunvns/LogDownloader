# LogDownloader

This tools helps us to download a number of Log Files from a remote Server on to our local machine, if we want to analyze them.

How to Configure:

This is a zip of files and not a Sitecore Package.

Steps to Configure it in a Sitecore Solution:

1. Unzip this to sitecore/admin folder.
2. Change the FileLocations.xml file and include your own Log Locations in /Resources/DataFile folder..
3. Check if there is ICSharpCode.SharpZipLib.dll - It was removed from the Bin directory, as per the Sitecore 7.0 Update-1 Release Notes of Sitecore : http://sdn.sitecore.net/Products/Sitecore%20V5/Sitecore%20CMS%207/ReleaseNotes/ChangeLog/Release%20History%20SC70.aspx
4. In case the DLL is not present, Move the DLL from the LogDownloader Folder to the bin directory
5. If you have the ICSharpCode.SharpZipLib.dll in your bin, then delete the one provided in the LogDownload folder.

Access the Sitecore MultiLog Downloader using the URL: http://oursitecoreinstance/sitecore/admin/LogDownloader/SitecoreMultiLogDownloader.aspx
