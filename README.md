# Webprint
本專案使用Win32_PrintJob與Win32 API抓取印表機伺服器中，所有佇列的訊息。</br>
如需要自定義印表機名稱，可在Script資料夾中建立District.json來去定義，範例如下:</br>
<div>
  <p>{"printname": "altername"}</p>
</div>
如何抓取District.json檔案:</br>
<div>
  <p>修改web.config的DistrictJsonPath標籤，更換成目前DistrictJsonPath資料夾位置</p>
</div>
</br>
如何抓取Spool檔案訊息:</br>
<div>
  <p>修改web.config的spoolFilePath標籤，更換成目前印表機伺服器的Spool資料夾位置，此標籤用來擷取印表機Job所使用的Spool檔案大小使用</p>
</div>
如需要客製化讀取Job列印檔案名稱可以修改moduel/PrintJobInfo.cs中的documentRegex:</br>
<p>document為原來檔案名稱</p>
```csharp
string documentRegex = document.Split('_')[1]+"_"+document.Split('_')[3];
```



