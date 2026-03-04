# Dapper UoW 教學範例專案 (.NET Framework 4.8 Web API)

這是一個以 **.NET Framework 4.8 Web API** 為基礎的教學範例專案，主要探討與實作以下技術堆疊與設計模式：
*   **Dapper**: 微型 ORM，負責高效的資料存取。
*   **Unit of Work (UoW) & Repository Pattern**: 透過 `IUnitOfWorkFactory` 掌控資料庫連線與交易 (Transaction) 的生命週期，解決多表操作與並行連線的問題。
*   **Autofac**: 依賴注入 (DI) 容器，展示 `InstancePerRequest` 與 `SingleInstance` 在連線管理的應用。
*   **Oracle 參數風格模擬**: 雖然底層為求方便展示使用了 SQLite (In-Memory File `DemoDB.sqlite`)，但 SQL 撰寫上融入了 Oracle 的 `:param` 變數風格及 `RETURNING` 思維。
*   **Owin Self-Host**: 不依賴 IIS，直接透過 `Program.cs` 跑起整套 Web API 服務 (Console Host)。

---

## 專案架構概覽

```text
DapperUoW_Net48_Api
├── App_Start/          # 系統組態 (AutofacConfig, WebApiConfig) 
├── Controllers/        # Web API 入口 (OrderController) - 提供三種情境測試
├── Core/               # 核心與領域層 (Entities, Dtos, Interfaces)
├── Infrastructure/     # 基礎設施層 (DataAccess, Repositories)
│   └── Repositories/   # Repository 實作，其中包含核心的 BaseRepository 封裝
├── Services/           # 商業邏輯層 (OrderService) - 掌控 UoW 的開啟與 Commit
└── Program.cs          # Owin 伺服器啟動載入點
```

---

## 核心亮點 1：為什麼需要 `IUnitOfWorkFactory`？

在傳統 ASP.NET MVC / Web API 中，我們經常將 `IUnitOfWork` 直接註冊為單一 Request 共用 (`InstancePerRequest`)。但這會導致一個致命缺點：**同一個 Request 內，無法進行非同步並發查詢 (Task.WhenAll)**，因為 ADO.NET 的同一條 `DbConnection` 無法同時承受多個執行緒的存取。

### 解法：Factory 模式
我們將 `IUnitOfWorkFactory` 註冊為 `Singleton`，讓需要連線的地方（Repo 或 Service）隨時向工廠「索取」一條**全新且獨立**的連線 (`IUnitOfWork`)。
*   **Service 層負責大局**：如果有多張表要交易 (Insert A + Update B)，Service 會自己 Create 一個 UoW，然後**當作參數傳進各個 Repo 中**，保證他們使用同一條連線。
*   **Repo 層負責單打獨鬥**：如果 Service 只是單純查詢，不想管連線。Repo 察覺到 Service 沒傳 UoW，就會**自己向 Factory 借一條新連線**，查完後立刻關閉 (`Dispose`)。

---

## 核心亮點 2：`BaseRepository` 封裝與 Lambda (`db =>`) 教學

如果每個 Repo 都自己向 Factory 借連線，那很容易出現「借了忘記還」或是「到處都是 `try...finally`」的冗長程式碼。
為此，我們設計了 `BaseRepository`：

```csharp
protected async Task<T> ExecuteWithDbAsync<T>(IUnitOfWork externalUow, Func<IUnitOfWork, Task<T>> action)
{
    var isExternal = externalUow != null;
    var uow = externalUow ?? _factory.Create();
    try
    {
        return await action(uow); 
    }
    finally
    {
        if (!isExternal) uow.Dispose();
    }
}
```

### 團隊教學：什麼是 `db =>`？
開發者在實作具體的 `OrderRepository` 時，會頻繁看到以下語法：

```csharp
public Task<int> InsertOrderAsync(Order order, IUnitOfWork uow = null)
{
    return ExecuteWithDbAsync(uow, async db =>
    {
        string sql = "INSERT INTO Orders ...";
        return await db.Connection.ExecuteScalarAsync<int>(sql, order, db.Transaction);
    });
}
```

**白話文解析 `db =>` (Lambda 表達式)**：
這只是一種「隱藏方法名稱的超簡寫寫法」。
1. **左邊的 `db`**：這是你自己取名的變數。`ExecuteWithDbAsync` 底層幫你準備好了一個資料庫連線 (`uow`)，並把它「交給」你。你就在左邊把它命名為 `db`。
2. **右邊的 `{ ... }` 區塊**：拿到這條叫做 `db` 的連線後，你要做的事！你就專心寫 Dapper 的連線邏輯，不用再煩惱這個 `db` 什麼時候要 `Open()` 或 `Dispose()`，因為底層的 BaseRepository 都幫你把 `try...finally` 包辦了！

---

## 如何執行與測試本專案？

1. 使用 Visual Studio 2022 或是透過 MSBuild 編譯專案。
2. 直接執行 (F5) 專案（Console 應用程式模式）。
3. 程式會自動於 Console 顯示 "Web API 伺服器已成功啟動於：http://localhost:9000/"，且會在根目錄建立並初始化模擬用的 SQLite 資料庫檔案 (`DemoDB.sqlite`)。
4. 使用 Postman 或是瀏覽器對以下三個展演路由進行測試：

### 展演情境 1：大交易共生共死 
*   **路由**: `POST` `http://localhost:9000/api/order/create?customerName=TestUser`
*   **展示重點**: Service 取出一個 `uow`，開出 Transaction，傳給 Repo 執行兩次 Insert。

### 展演情境 2：單表操作，Repository 自動管理 
*   **路由**: `GET` `http://localhost:9000/api/order/1`
*   **展示重點**: Service **沒有**傳入 `uow`，Repository 會自動向工廠拿一條新連線，查完就 Dispose。

### 展演情境 3：同時多連線並發查詢 
*   **路由**: `POST` `http://localhost:9000/api/order/dashboard`
*   **展示重點**: Service 中自己 `Create()` 兩條互不干擾的連線 (`readUow1`, `readUow2`) 送給 Repo 來解決 `Task.WhenAll` 的並發衝突，最後又開了第三條連線來進行最後的寫入。

---
## ⚡ 無網路環境 (離線) 建置說明

本專案考量到「公司內部無網路」的開發環境限制，已經將所有依賴的 NuGet 套件（包含 Autofac, Dapper, SQLite 等）預先下載並放置於 `offline-packages` 資料夾中。
並且根目錄的 `nuget.config` 也已經設定強制只從該本地資料夾還原套件：

1. **完全離線**：您只需將整個專案 clone 或複製到內部無網際網路的電腦上。
2. **直接建置**：用 Visual Studio 2022 開啟專案，或是使用命令列執行 `dotnet build`，NuGet 將會自動從 `offline-packages` 夾中取得所有必要的 `.nupkg` 檔案，**不會報任何網路連線錯誤**。
3. 如果出現找不到套件，請確定您複製專案時有包含 `offline-packages` 隱藏資料夾與 `nuget.config`。

---
**本專案作為架構解說用，歡迎團隊開發者參閱 `BaseRepository.cs` 與 `OrderService.cs` 中的註解說明！**
