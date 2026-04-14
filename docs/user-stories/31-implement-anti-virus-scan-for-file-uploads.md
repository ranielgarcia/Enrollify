# ClamAV

ClamAV is an open-source antivirus engine commonly used on servers to scan files for malware, viruses, and other threats.
In the context of file uploads, you'd pipe the uploaded file through ClamAV before saving it. There's a NuGet package nClam that makes this straightforward in .NET:

```csharp
var clam = new ClamClient("localhost", 3310);
var result = await clam.SendAndScanFileAsync(file.OpenReadStream());

if (result.Result == ClamScanResults.VirusDetected)
    return (ErrorOccurred: true, ErrorMessage: "File failed virus scan");
```


Practical notes:

ClamAV runs as a separate daemon (clamd) — your app talks to it over TCP
Easy to run via Docker: docker run -d -p 3310:3310 clamav/clamav
It's free but not the most robust scanner — it's better suited for defense-in-depth than as your sole security measure
For production, paid cloud alternatives like AWS Malware Protection or Google Cloud's VirusTotal API are more reliable

For most internal/low-risk apps, magic bytes + extension validation is sufficient. ClamAV is worth adding if users are uploading files that others will later download.