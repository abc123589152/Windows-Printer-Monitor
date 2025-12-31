# 🖨️ 印表機佇列監控專案說明文件

本專案透過 **Win32_PrintJob** 與 **Win32 API**  
用來抓取 **印表機伺服器中所有印表機佇列（Queue）與列印工作（Job）資訊**，  
並提供 **自訂印表機名稱、Spool 檔案大小解析與列印文件名稱客製化** 功能。

---

## 📌 功能總覽

- 取得印表機伺服器中 **所有列印佇列資訊**
- 顯示列印工作：
  - 印表機名稱
  - 文件名稱
  - 列印狀態
  - 總頁數
  - Spool 檔案大小
  - 送印時間
- 支援 **自定義顯示的印表機名稱**
- 支援 **解析 Spool 檔案大小**
- 可客製化 **列印文件名稱解析規則**

---

## 🏷️ 自定義印表機名稱（District.json）

如需將實際印表機名稱顯示為 **自訂名稱**，  
請在 **`Scripts` 資料夾** 中建立 `District.json` 檔案。

### 🔹 範例格式

```json
{
  "printname": "altername"
}
```
---
## 🏷️ 如何抓取District.json檔案
修改 **`web.config`的DistrictJsonPath標籤** ，更換成目前`DistrictJsonPath` 資料夾位置
```xml
<add key="DistrictJsonPath" value="C:\webprint\Scripts\District.json" />
```
---
## 🏷️ 如何抓取Spool檔案大小
修改 **`web.config`的spoolFilePath標籤** ，更換成目前印表機伺服器的Spool資料夾位置，此標籤用來擷取印表機Job所使用的Spool檔案大小使用
```xml
<add key="spoolFilePath" value="C:\Windows\System32\spool\PRINTERS" />
```
---
## 🏷️ 如何客製化Printer Job檔案名稱
如需要客製化讀取Job列印檔案名稱可以修改 **`moduel/PrintJobInfo.cs`中的documentRegex** 
```csharp
string documentRegex = document.Split('_')[1]+"_"+document.Split('_')[3];
```


