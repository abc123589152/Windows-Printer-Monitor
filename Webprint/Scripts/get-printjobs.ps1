$printJobs = Get-WmiObject -Class Win32_PrintJob | Where-Object { $_.StatusMask -ne 128 }

$result = @()
foreach ($job in $printJobs) {
    $jobInfo = @{
        PrinterName = $job.Name
        Document = $job.Document
        JobStatus = $job.JobStatus
        PagesPrinted = $job.PagesPrinted
        TotalPages = $job.TotalPages
        Size = $job.Size
        JobId = $job.JobId
        Owner = $job.Owner
        StatusMask = $job.StatusMask
        TimeSubmitted = $job.TimeSubmitted
    }
    $result += $jobInfo
}

$result | ConvertTo-Json -Depth 3