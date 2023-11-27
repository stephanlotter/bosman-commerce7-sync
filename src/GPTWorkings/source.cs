namespace BosmanCommerce7.Module.ApplicationServices.AppDataServices {
  public class AppDataFileManager : IAppDataFileManager {
    private readonly ILogger<AppDataFileManager> _logger;
    private readonly ApplicationOptions _applicationOptions;
    public string RootFolder => _applicationOptions.AppDataFolder;
    public AppDataFileManager(ILogger<AppDataFileManager> logger, ApplicationOptions applicationOptions) {
      _logger = logger;
      _applicationOptions = applicationOptions;
    }
    public Result<T?> LoadJson<T>(string subfolderName, string filename) {
      try {
        return LoadText(subfolderName, filename)
          .Map(json => JsonFunctions.Deserialize<T>(json));
      }
      catch (Exception ex) {
        _logger.LogError(ex, "While loading JSON");
        return Result.Failure<T?>(ex.Message);
      }
    }
    public Result<string> LoadText(string subfolderName, string filename) {
      try {
        var file = BuildFilename(subfolderName, filename);
        if (!File.Exists(file)) {
          return Result.Failure<string>($"File '{file}' does not exist.");
        }
        return Result.Success(File.ReadAllText(file));
      }
      catch (Exception ex) {
        _logger.LogError(ex, "While loading text");
        return Result.Failure<string>(ex.Message);
      }
    }
    public Result RemoveFile(string subfolderName, string filename) {
      try {
        var file = BuildFilename(subfolderName, filename);
        File.Delete(file);
        return Result.Success();
      }
      catch (Exception ex) {
        _logger.LogError(ex, "While loading text");
        return Result.Failure(ex.Message);
      }
    }
    public Result<string> StoreJson(string subfolderName, string filename, object? data) {
      var json = JsonFunctions.Serialize(data);
      return StoreText(subfolderName, filename, json);
    }
    public Result<string> StoreText(string subfolderName, string filename, string? data) {
      try {
        var file = BuildFilename(subfolderName, filename);
        File.WriteAllText(file, data ?? "");
        return Result.Success(file);
      }
      catch (Exception ex) {
        _logger.LogError(ex, "While storing text");
        return Result.Failure<string>(ex.Message);
      }
    }
    private string BuildFilename(string subfolderName, string filename) {
      var path = _applicationOptions.InAppDataFolder(subfolderName);
      var file = Path.Combine(path, filename);
      return file;
    }
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.AppDataServices {
  public interface IAppDataFileManager {
    Result<T?> LoadJson<T>(string subfolderName, string filename);
    Result<string> LoadText(string subfolderName, string filename);
    Result RemoveFile(string subfolderName, string filename);
    Result<string> StoreJson(string subfolderName, string filename, object? data);
    Result<string> StoreText(string subfolderName, string filename, string data);
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.DataAccess {
  public abstract class SqlServerRepositoryBase {
    protected readonly ILogger _logger;
    public SqlServerRepositoryBase(ILogger logger) {
      _logger = logger;
    }
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.DataAccess.LocalDatabaseDataAccess {
  public interface ILocalObjectSpaceProvider {
    void WrapInObjectSpaceTransaction(Action<IObjectSpace> action);
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.DataAccess.LocalDatabaseDataAccess {
  public interface IValueStoreRepository {
    Result DeleteKey(string keyName);
    Result<string?> GetValue(string keyName, string? defaultValue = null);
    Result<DateTime?> GetDateTimeValue(string keyName, DateTime? defaultValue = null);
    Result<int?> GetIntValue(string keyName, int? defaultValue = null);
    Result<bool> KeyExists(string keyName);
    Result SetValue(string keyName, string? keyValue);
    Result SetDateTimeValue(string keyName, DateTime? keyValue);
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.DataAccess.LocalDatabaseDataAccess {
  public interface IWarehouseRepository {
    Result<string?> FindWarehouseCode(FindWarehouseCodeDescriptor findWarehouseCodeDescriptor);
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.DataAccess.LocalDatabaseDataAccess {
  public abstract class LocalDatabaseRepositoryBase : SqlServerRepositoryBase {
    protected ILocalDatabaseConnectionStringProvider ConnectionStringProvider { get; }
    public LocalDatabaseRepositoryBase(ILogger logger, ILocalDatabaseConnectionStringProvider connectionStringProvider) : base(logger) {
      ConnectionStringProvider = connectionStringProvider;
    }
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.DataAccess.LocalDatabaseDataAccess {
  public class LocalObjectSpaceProvider : ILocalObjectSpaceProvider {
    private readonly ILocalDatabaseConnectionStringProvider _localDatabaseConnectionStringProvider;
    public LocalObjectSpaceProvider(ILocalDatabaseConnectionStringProvider localDatabaseConnectionStringProvider) {
      _localDatabaseConnectionStringProvider = localDatabaseConnectionStringProvider;
    }
    public void WrapInObjectSpaceTransaction(Action<IObjectSpace> action) {
      //XafTypesInfo.Instance.RegisterEntity(typeof(OnlineSalesOrder));
      osp.TypesInfo.RegisterEntity(typeof(OnlineSalesOrder));
      try {
        action(objectSpace);
        objectSpace.CommitChanges();
      }
      catch {
        objectSpace.Rollback();
        throw;
      }
    }
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.DataAccess.LocalDatabaseDataAccess {
  public class ValueStoreRepository : LocalDatabaseRepositoryBase, IValueStoreRepository {
    public ValueStoreRepository(ILogger<ValueStoreRepository> logger, ILocalDatabaseConnectionStringProvider connectionStringProvider) : base(logger, connectionStringProvider) {
    }
    public Result DeleteKey(string keyName) {
      const string sql = @"
if exists(select 1 from ValueStore where lower(KeyName) = lower(@keyName))
    delete from ValueStore where lower(KeyName) = lower(@keyName);
";
      try {
        ConnectionStringProvider.LocalDatabase.WrapInTransaction((connection, transaction) => {
          connection.Execute(sql, new { keyName }, transaction: transaction);
        });
        return Result.Success();
      }
      catch (Exception ex) {
        _logger.LogError(ex, "While DeleteKey");
        return Result.Failure(ex.Message);
      }
    }
    public Result<DateTime?> GetDateTimeValue(string keyName, DateTime? defaultValue = null) {
      return GetValue(keyName, $"{defaultValue:yyyy-MM-dd HH:mm:ss}")
        .Bind(v => Result.Success<DateTime?>(DateTime.TryParse(v, out DateTime r) ? r : null));
    }
    public Result<int?> GetIntValue(string keyName, int? defaultValue = null) {
      return GetValue(keyName, $"{defaultValue}")
        .Bind(v => Result.Success<int?>(int.TryParse(v, out int r) ? r : null));
    }
    public Result<string?> GetValue(string keyName, string? defaultValue = null) {
      const string sql = @"
if exists(select 1 from ValueStore where lower(KeyName) = lower(@keyName))
  select KeyValue from ValueStore where lower(KeyName) = lower(@keyName)
else
  select @defaultValue;
";
      try {
        var data = ConnectionStringProvider.LocalDatabase.WrapInOpenConnection(connection => {
          return connection.QueryFirst<string?>(sql, new { keyName, defaultValue });
        });
        return Result.Success(data);
      }
      catch (Exception ex) {
        _logger.LogError(ex, "While GetValue");
        return Result.Failure<string?>(ex.Message);
      }
    }
    public Result<bool> KeyExists(string keyName) {
      const string sql = @"
if exists(select 1 from ValueStore where lower(KeyName) = lower(@keyName))
    select convert(Bit, 1);
else
    select convert(Bit, 0);
";
      try {
        var data = ConnectionStringProvider.LocalDatabase.WrapInOpenConnection(connection => {
          return connection.QueryFirst<bool>(sql, new { keyName });
        });
        return Result.Success(data);
      }
      catch (Exception ex) {
        _logger.LogError(ex, "While KeyExists");
        return Result.Failure<bool>(ex.Message);
      }
    }
    public Result SetDateTimeValue(string keyName, DateTime? keyValue) {
      return SetValue(keyName, $"{keyValue:yyyy-MM-dd HH:mm:ss.fffffff}");
    }
    public Result SetValue(string keyName, string? keyValue) {
      const string sql = @"
if exists(select 1 from ValueStore where lower(KeyName) = lower(@keyName))
    update ValueStore set KeyValue=@keyValue where lower(KeyName) = lower(@keyName);
else 
    insert into ValueStore (KeyName, KeyValue, OptimisticLockField) values (lower(@keyName), @keyValue, 0);
";
      try {
        ConnectionStringProvider.LocalDatabase.WrapInTransaction((connection, transaction) => {
          connection.Execute(sql, new { keyName, keyValue }, transaction: transaction);
        });
        return Result.Success();
      }
      catch (Exception ex) {
        _logger.LogError(ex, "While SetValue");
        return Result.Failure(ex.Message);
      }
    }
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.DataAccess.LocalDatabaseDataAccess {
  public class WarehouseRepository : LocalDatabaseRepositoryBase, IWarehouseRepository {
    private readonly IValueStoreRepository _valueStoreRepository;
    public WarehouseRepository(ILogger<WarehouseRepository> logger, ILocalDatabaseConnectionStringProvider connectionStringProvider, IValueStoreRepository valueStoreRepository) : base(logger, connectionStringProvider) {
      _valueStoreRepository = valueStoreRepository;
    }
    public Result<string?> FindWarehouseCode(FindWarehouseCodeDescriptor findWarehouseCodeDescriptor) {
      return _valueStoreRepository.GetValue("default-ship-from-warehouse-code");
      //if (!findWarehouseCodeDescriptor.IsStoreOrder) {
      //  return _valueStoreRepository.GetValue("default-ship-from-warehouse-code");
      //}
      //if (string.IsNullOrWhiteSpace(findWarehouseCodeDescriptor.PostalCode)) {
      //  return Result.Failure<string?>("Postal code not provided");
      //}
      //var mapping = findWarehouseCodeDescriptor.ObjectSpace.FindObject<WarehousePostalCodeMapping>("PostalCode".IsEqualToOperator(findWarehouseCodeDescriptor.PostalCode.Trim()));
      //return mapping != null ? Result.Success(mapping.WarehouseCode) : Result.Failure<string?>($"No postal code/warehouse code mapping found for {findWarehouseCodeDescriptor.PostalCode.Trim()}.");
    }
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.EvolutionSdk {
  public class EvolutionCustomerRepository : EvolutionRepositoryBase, IEvolutionCustomerRepository {
    public Result<Customer> Get(CustomerDescriptor customerDescriptor) {
      if (string.IsNullOrWhiteSpace(customerDescriptor.EmailAddress)) {
        return Result.Failure<Customer>($"Customer account lookup: Email address may not be empty.");
      }
      int? id = GetId("SELECT DCLink FROM Client WHERE ucARwcEmail = @EmailAddress", new { customerDescriptor.EmailAddress });
      if (id == null) {
        return Result.Failure<Customer>($"Customer with email address {customerDescriptor.EmailAddress} not found");
      }
      return new Customer(id.Value);
    }
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.EvolutionSdk {
  public class EvolutionDeliveryMethodRepository : EvolutionRepositoryBase, IEvolutionDeliveryMethodRepository {
    public Result<DeliveryMethod> Get(string? code) {
      if (string.IsNullOrWhiteSpace(code)) {
        return Result.Failure<DeliveryMethod>($"Delivery method lookup: code may not be empty.");
      }
      int? id = GetId("select Counter from DelTbl where Method = @code", new { code });
      if (id == null) {
        return Result.Failure<DeliveryMethod>($"Delivery Method code {code} not found");
      }
      return new DeliveryMethod(id.Value);
    }
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.EvolutionSdk {
  public class EvolutionGeneralLedgerAccountRepository : EvolutionRepositoryBase, IEvolutionGeneralLedgerAccountRepository {
    public Result<GLAccount> Get(string? code) {
      if (string.IsNullOrWhiteSpace(code)) {
        return Result.Failure<GLAccount>($"General ledger account lookup: code may not be empty.");
      }
      int? id = GetId("select AccountLink from Accounts where lower(Master_Sub_Account)=lower(@code)", new { code });
      if (id == null) {
        return Result.Failure<GLAccount>($"General ledger account with code {code} not found");
      }
      return new GLAccount(id.Value);
    }
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.EvolutionSdk {
  public class EvolutionInventoryItemRepository : EvolutionRepositoryBase, IEvolutionInventoryItemRepository {
    public Result<InventoryItem> Get(string? code) {
      if (string.IsNullOrWhiteSpace(code)) {
        return Result.Failure<InventoryItem>($"Inventory lookup: code may not be empty.");
      }
      int? id = GetId("select StockLink from StkItem where lower(cSimpleCode)=lower(@code)", new { code });
      if (id == null) {
        return Result.Failure<InventoryItem>($"Inventory item with code {code} not found");
      }
      return new InventoryItem(id.Value);
    }
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.EvolutionSdk
{
    public class EvolutionPriceListRepository : EvolutionRepositoryBase, IEvolutionPriceListRepository {
    private readonly ILogger<EvolutionPriceListRepository> _logger;
    public EvolutionPriceListRepository(ILogger<EvolutionPriceListRepository> logger) {
      _logger = logger;
    }
    public Result<EvolutionPriceListPrice> Get(int inventoryItemId, int? priceListId) {
      const string sql = @"
if not exists(select 1 from _etblPriceListPrices where iPriceListNameID = @priceListId) 
  select -1;
else
  select fInclPrice PriceValue
	  from _etblPriceListPrices
	  where iPriceListNameID = @priceListId
		  and iStockID = @inventoryItemId
		  and iWarehouseID = 0;
";
      var useDefaultPriceList = !priceListId.HasValue || priceListId.Value <= 0;
      static int? DefaultPriceListId() => PriceList.GetDefault()?.ID;
      priceListId = useDefaultPriceList ? DefaultPriceListId() : priceListId;
      if (!priceListId.HasValue) {
        return Result.Failure<EvolutionPriceListPrice>("No price list id provided and there is no default price list defined in Evolution.");
      }
      var priceValue = GetDouble(sql, new { priceListId, inventoryItemId });
      if (!priceValue.HasValue && !useDefaultPriceList) {
        _logger.LogWarning("Could not find price list ID:{priceListId} in database. Trying default price list.", priceListId);
        priceValue = GetDouble(sql, new { priceListId = DefaultPriceListId(), inventoryItemId });
      }
      return priceValue.HasValue
        ? Result.Success(new EvolutionPriceListPrice(priceListId.Value, priceValue!.Value))
        : Result.Failure<EvolutionPriceListPrice>($"Could not find price list ID:{priceListId} in database.");
    }
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.EvolutionSdk {
  public class EvolutionProjectRepository : EvolutionRepositoryBase, IEvolutionProjectRepository {
    public Result<Project> Get(string? code) {
      if (string.IsNullOrWhiteSpace(code)) {
        return Result.Failure<Project>($"Project lookup: code may not be empty.");
      }
      int? id = GetId("select p.ProjectLink from Project p where lower(p.ProjectCode)=lower(@code)", new { code });
      if (id == null) {
        return Result.Failure<Project>($"Project with code {code} not found");
      }
      return new Project(id.Value);
    }
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.EvolutionSdk {
  public abstract class EvolutionRepositoryBase {
    protected DbConnection Connection => DatabaseContext.DBConnection;
    protected DbTransaction Transaction => DatabaseContext.DBTransaction;
    protected int? GetId(string sql, object? param = null) {
      return Connection.QueryFirstOrDefault<int?>(sql, param, transaction: Transaction);
    }
    protected double? GetDouble(string sql, object? param = null) {
      return Connection.QueryFirstOrDefault<double?>(sql, param, transaction: Transaction);
    }
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.EvolutionSdk {
  public class EvolutionSalesRepresentativeRepository : EvolutionRepositoryBase, IEvolutionSalesRepresentativeRepository {
    public Result<SalesRepresentative> Get(string? code) {
      if (string.IsNullOrWhiteSpace(code)) {
        return Result.Failure<SalesRepresentative>($"Sales representative lookup: code may not be empty.");
      }
      int? id = GetId("select idSalesRep from SalesRep where Code = @code", new { code });
      if (id == null) {
        return Result.Failure<SalesRepresentative>($"Sales Representative with code {code} not found");
      }
      return new SalesRepresentative(id.Value);
    }
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.EvolutionSdk {
  public class EvolutionSdk : IEvolutionSdk {
    private readonly IEvolutionCompanyDescriptor _companyDescriptor;
    public DatabaseConnection Connection => new(DatabaseContext.DBConnection, DatabaseContext.DBTransaction);
    public EvolutionSdk(IEvolutionCompanyDescriptor companyDescriptor) {
      _companyDescriptor = companyDescriptor;
    }
    public Result WrapInSdkTransaction(Func<IDatabaseConnection, Result> func) {
      try {
        BeginTransaction();
        var result = func(new DatabaseConnection(DatabaseContext.DBConnection, DatabaseContext.DBTransaction));
        result
          .Tap(() => { CommitTransaction(); })
          .TapError(() => { RollbackTransaction(); });
        return result;
      }
      catch {
        DatabaseContext.RollbackTran();
        throw;
      }
    }
    public Result<T> WrapInSdkTransaction<T>(Func<IDatabaseConnection, Result<T>> func) {
      try {
        BeginTransaction();
        var result = func(new DatabaseConnection(DatabaseContext.DBConnection, DatabaseContext.DBTransaction));
        result
          .Tap(() => { CommitTransaction(); })
          .TapError(() => { RollbackTransaction(); });
        return result;
      }
      catch {
        RollbackTransaction();
        throw;
      }
    }
    public void BeginTransaction() {
      BeginTransaction(_companyDescriptor.EvolutionCompanyDatabaseConnectionString,
        _companyDescriptor.EvolutionCommonDatabaseConnectionString,
        _companyDescriptor.BranchCode,
        string.IsNullOrWhiteSpace(_companyDescriptor.UserName) ? null : _companyDescriptor.UserName);
    }
    public void CommitTransaction() {
      if (!DatabaseContext.IsTransactionPending) { return; }
      DatabaseContext.CommitTran();
    }
    public void RollbackTransaction() {
      if (!DatabaseContext.IsTransactionPending) { return; }
      DatabaseContext.RollbackTran();
    }
    private static void BeginTransaction(string companyConnectionString, string commonDatabaseConnectionString, string branchCode, string? userName) {
      if (DatabaseContext.IsConnectionOpen) { return; }
      var retryCount = 0;
      while (true) {
        try {
          DatabaseContext.CreateCommonDBConnection(commonDatabaseConnectionString);
          DatabaseContext.SetLicense("DE09110064", "2428759");
          DatabaseContext.CreateConnection(companyConnectionString);
          DatabaseContext.BeginTran();
          if (!string.IsNullOrWhiteSpace(branchCode)) {
            var branch = new Branch(branchCode);
            DatabaseContext.SetBranchContext(branch.ID);
          }
          if (!string.IsNullOrWhiteSpace(userName)) {
            DatabaseContext.CurrentAgent = new Agent(userName);
          }
          return;
        }
        catch (Exception ex) when (Regex.IsMatch(ex.Message, ".*registration.*invalid.*")) {
          throw new Exception("Evolution SDK registration is invalid. Check that Evolution is registered and that the version of the SDK is the same as that of Evolution.");
        }
        catch (FormatException ex) when (Regex.IsMatch(ex.Message, ".*Input string was not in a correct format.*")) {
          if (retryCount > 3) {
            throw;
          }
          Thread.Sleep(2000);
          retryCount++;
        }
      }
    }
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.EvolutionSdk {
  public class EvolutionWarehouseRepository : EvolutionRepositoryBase, IEvolutionWarehouseRepository {
    public Result<Warehouse> Get(string? code) {
      if (string.IsNullOrWhiteSpace(code)) {
        return Result.Failure<Warehouse>($"Warehouse lookup: code may not be empty.");
      }
      int? id = GetId("select WhseLink from WhseMst where lower(Code)=lower(@code)", new { code });
      if (id == null) {
        return Result.Failure<Warehouse>($"Warehouse with code {code} not found");
      }
      return new Warehouse(id.Value);
    }
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.EvolutionSdk {
  public interface IEvolutionCustomerRepository {
    Result<Customer> Get(CustomerDescriptor customerDescriptor);
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.EvolutionSdk {
  public interface IEvolutionDeliveryMethodRepository {
    Result<DeliveryMethod> Get(string? code);
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.EvolutionSdk {
  public interface IEvolutionGeneralLedgerAccountRepository {
    Result<GLAccount> Get(string? code);
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.EvolutionSdk {
  public interface IEvolutionInventoryItemRepository {
    Result<InventoryItem> Get(string? code);
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.EvolutionSdk
{
    public interface IEvolutionPriceListRepository {
    Result<EvolutionPriceListPrice> Get(int inventoryItemId, int? priceListId);
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.EvolutionSdk {
  public interface IEvolutionProjectRepository {
    Result<Project> Get(string? code);
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.EvolutionSdk {
  public interface IEvolutionSalesRepresentativeRepository {
    Result<SalesRepresentative> Get(string? code);
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.EvolutionSdk {
  public interface IEvolutionSdk {
    DatabaseConnection? Connection { get; }
    void BeginTransaction();
    void CommitTransaction();
    void RollbackTransaction();
    Result<T> WrapInSdkTransaction<T>(Func<IDatabaseConnection, Result<T>> func);
    Result WrapInSdkTransaction(Func<IDatabaseConnection, Result> func);
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.EvolutionSdk {
  public interface IEvolutionWarehouseRepository {
    Result<Warehouse> Get(string? code);
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.EvolutionSdk {
  public interface IPostToEvolutionSalesOrderService {
    Result<OnlineSalesOrder> Post(PostToEvolutionSalesOrderContext context, OnlineSalesOrder onlineSalesOrder);
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.EvolutionSdk {
  public record PostToEvolutionSalesOrderContext {
    public IObjectSpace ObjectSpace { get; init; } = default!;
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.EvolutionSdk {
  public class PostToEvolutionSalesOrderService : IPostToEvolutionSalesOrderService {
    private readonly ILogger<PostToEvolutionSalesOrderService> _logger;
    private readonly IEvolutionSdk _evolutionSdk;
    private readonly ISalesOrdersPostValueStoreService _salesOrdersPostValueStoreService;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IEvolutionCustomerRepository _evolutionCustomerRepository;
    private readonly IEvolutionProjectRepository _evolutionProjectRepository;
    private readonly IEvolutionDeliveryMethodRepository _evolutionDeliveryMethodRepository;
    private readonly IEvolutionSalesRepresentativeRepository _evolutionSalesRepresentativeRepository;
    private readonly IEvolutionInventoryItemRepository _evolutionInventoryItemRepository;
    private readonly IEvolutionWarehouseRepository _evolutionWarehouseRepository;
    private readonly IEvolutionPriceListRepository _evolutionPriceListRepository;
    private readonly IEvolutionGeneralLedgerAccountRepository _evolutionGeneralLedgerAccountRepository;
    public PostToEvolutionSalesOrderService(ILogger<PostToEvolutionSalesOrderService> logger,
      IEvolutionSdk evolutionSdk,
      ISalesOrdersPostValueStoreService salesOrdersPostValueStoreService,
      IWarehouseRepository warehouseRepository,
      IEvolutionCustomerRepository evolutionCustomerRepository,
      IEvolutionProjectRepository evolutionProjectRepository,
      IEvolutionDeliveryMethodRepository evolutionDeliveryMethodRepository,
      IEvolutionSalesRepresentativeRepository evolutionSalesRepresentativeRepository,
      IEvolutionInventoryItemRepository evolutionInventoryItemRepository,
      IEvolutionWarehouseRepository evolutionWarehouseRepository,
      IEvolutionPriceListRepository evolutionPriceListRepository,
      IEvolutionGeneralLedgerAccountRepository evolutionGeneralLedgerAccountRepository) {
      _logger = logger;
      _evolutionSdk = evolutionSdk;
      _salesOrdersPostValueStoreService = salesOrdersPostValueStoreService;
      _warehouseRepository = warehouseRepository;
      _evolutionCustomerRepository = evolutionCustomerRepository;
      _evolutionProjectRepository = evolutionProjectRepository;
      _evolutionDeliveryMethodRepository = evolutionDeliveryMethodRepository;
      _evolutionSalesRepresentativeRepository = evolutionSalesRepresentativeRepository;
      _evolutionInventoryItemRepository = evolutionInventoryItemRepository;
      _evolutionWarehouseRepository = evolutionWarehouseRepository;
      _evolutionPriceListRepository = evolutionPriceListRepository;
      _evolutionGeneralLedgerAccountRepository = evolutionGeneralLedgerAccountRepository;
    }
    public Result<OnlineSalesOrder> Post(PostToEvolutionSalesOrderContext context, OnlineSalesOrder onlineSalesOrder) {
      return _evolutionSdk.WrapInSdkTransaction(connection => {
        try {
          return CreateSalesOrderHeader(context, onlineSalesOrder)
          .Bind(salesOrder => AddSalesOrderLines(context, salesOrder, onlineSalesOrder))
          .Bind(salesOrder => {
            salesOrder.Save();
            onlineSalesOrder.EvolutionSalesOrderNumber = salesOrder.OrderNo;
            onlineSalesOrder.PostingStatus = SalesOrderPostingStatus.Posted;
            onlineSalesOrder.DatePosted = DateTime.Now;
            return Result.Success(onlineSalesOrder);
          });
        }
        catch (Exception ex) {
          _logger.LogError(ex, "Error posting sales order Online Order Number {OrderNumber}", onlineSalesOrder.OrderNumber);
          return Result.Failure<OnlineSalesOrder>(ex.Message);
        }
      });
    }
    private Result<SalesOrder> CreateSalesOrderHeader(PostToEvolutionSalesOrderContext context, OnlineSalesOrder onlineSalesOrder) {
      return NewSalesOrder()
        .Bind(salesOrder => {
          var parameters = new CustomerDescriptor { EmailAddress = onlineSalesOrder.EmailAddress };
          return RepositoryGet(salesOrder, parameters, p => _evolutionCustomerRepository.Get((CustomerDescriptor)p), customer => salesOrder.Customer = customer);
        })
        .Bind(salesOrder => RepositoryGetFromCode(salesOrder, onlineSalesOrder.ProjectCode, _evolutionProjectRepository.Get, project => salesOrder.Project = project))
        .Bind(salesOrder => _salesOrdersPostValueStoreService
          .GetDefaultSalesRepresentativeCode()
          .Bind(code => RepositoryGetFromCode(salesOrder, code, _evolutionSalesRepresentativeRepository.Get, representative => salesOrder.Representative = representative))
        )
        .Bind(salesOrder => _salesOrdersPostValueStoreService
          .GetDefaultDeliveryMethodCode()
          .Bind(code => RepositoryGetFromCode(salesOrder, code, _evolutionDeliveryMethodRepository.Get, deliveryMethod => salesOrder.DeliveryMethod = deliveryMethod))
        )
        .Bind(salesOrder => {
          var deliveryAddress = onlineSalesOrder.ShipToAddress();
          salesOrder.DeliverTo = deliveryAddress;
          return Result.Success(salesOrder);
        })
        .Bind(salesOrder => {
          salesOrder.DiscountPercent = onlineSalesOrder.IsStoreOrder ? salesOrder.Customer.AutomaticDiscount : 0d;
          return Result.Success(salesOrder);
        });
      Result<SalesOrder> NewSalesOrder() {
        return Result.Success(new SalesOrder {
          ExternalOrderNo = $"{onlineSalesOrder.OrderNumber}",
          OrderDate = onlineSalesOrder.OrderDate,
          TaxMode = TaxMode.Inclusive
        });
      }
    }
    private Result<SalesOrder> AddSalesOrderLines(PostToEvolutionSalesOrderContext context, SalesOrder salesOrder, OnlineSalesOrder onlineSalesOrder) {
      foreach (var onlineSalesOrderLine in onlineSalesOrder.SalesOrderLines.OrderBy(a => a.LineType)) {
        var result = onlineSalesOrderLine.LineType switch {
          SalesOrderLineType.Inventory => AddSalesOrderInventoryLine(context, salesOrder, onlineSalesOrder, onlineSalesOrderLine),
          SalesOrderLineType.GeneralLedger => AddSalesOrderGeneralLedgerLine(context, salesOrder, onlineSalesOrder, onlineSalesOrderLine),
          _ => throw new NotImplementedException()
        };
        if (result.IsFailure) { return result; }
      }
      return Result.Success(salesOrder);
    }
    private Result<SalesOrder> AddSalesOrderInventoryLine(PostToEvolutionSalesOrderContext context, SalesOrder salesOrder, OnlineSalesOrder onlineSalesOrder, OnlineSalesOrderLine onlineSalesOrderLine) {
      try {
        if (string.IsNullOrWhiteSpace(onlineSalesOrderLine.Sku)) {
          return Result.Failure<SalesOrder>($"Sku on line with Oid {onlineSalesOrderLine.Oid} is blank");
        }
        return NewSalesOrderLine(salesOrder)
          .Bind(salesOrderLine => RepositoryGetFromCode(salesOrderLine, onlineSalesOrderLine.Sku, _evolutionInventoryItemRepository.Get, inventoryItem => salesOrderLine.InventoryItem = inventoryItem))
          .Bind(salesOrderLine => {
            return _warehouseRepository.FindWarehouseCode(new FindWarehouseCodeDescriptor {
              IsStoreOrder = onlineSalesOrder.IsStoreOrder,
              PostalCode = onlineSalesOrder.ShipToAddressPostalCode,
              ObjectSpace = context.ObjectSpace
            })
            .Bind(warehouseCode => RepositoryGetFromCode(salesOrderLine, warehouseCode, _evolutionWarehouseRepository.Get, warehouse => salesOrderLine.Warehouse = warehouse));
          })
          .Bind(salesOrderLine => {
            int? customerPriceListId = salesOrder.Customer.DefaultPriceListID > 0 ? salesOrder.Customer.DefaultPriceListID : null;
            return _evolutionPriceListRepository
              .Get(salesOrderLine.InventoryItem.ID, customerPriceListId)
              .Bind(evolutionPrice => {
                salesOrderLine.PriceListNameID = evolutionPrice.UsedPriceListId;
                salesOrderLine.UnitSellingPrice = evolutionPrice.UnitPriceInVat;
                return Result.Success(salesOrderLine);
              });
          })
          .Bind(salesOrderLine => {
            salesOrderLine.Quantity = onlineSalesOrderLine.Quantity;
            salesOrderLine.Reserved = Math.Min(salesOrderLine.WarehouseContext.QtyFree, onlineSalesOrderLine.Quantity);
            if (!string.IsNullOrWhiteSpace(onlineSalesOrderLine.LineNotes)) {
              salesOrderLine.Note = onlineSalesOrderLine.LineNotes;
            }
            return Result.Success(salesOrderLine);
          })
          .Map(_ => salesOrder);
      }
      catch (Exception ex) {
        var message = $"Error adding inventory line with code '{onlineSalesOrderLine.Sku}' (Oid:{onlineSalesOrderLine.Oid}): {ex.Message}";
        _logger.LogError("{error}", message);
        _logger.LogError(ex, "Error adding inventory line");
        return Result.Failure<SalesOrder>(message);
      }
    }
    private Result<SalesOrder> AddSalesOrderGeneralLedgerLine(PostToEvolutionSalesOrderContext context, SalesOrder salesOrder, OnlineSalesOrder onlineSalesOrder, OnlineSalesOrderLine onlineSalesOrderLine) {
      try {
        return NewSalesOrderLine(salesOrder)
          .Bind(salesOrderLine => RepositoryGetFromCode(salesOrderLine, onlineSalesOrderLine.Sku, _evolutionGeneralLedgerAccountRepository.Get, account => salesOrderLine.Account = account))
          .Bind(salesOrderLine => {
            salesOrderLine.Quantity = onlineSalesOrderLine.Quantity;
            salesOrderLine.UnitSellingPrice = (double)onlineSalesOrderLine.UnitPriceInVat;
            if (!string.IsNullOrWhiteSpace(onlineSalesOrderLine.LineDescription)) {
              salesOrderLine.Description = onlineSalesOrderLine.LineDescription;
            }
            if (!string.IsNullOrWhiteSpace(onlineSalesOrderLine.LineNotes)) {
              salesOrderLine.Note = onlineSalesOrderLine.LineNotes;
            }
            return Result.Success(salesOrderLine);
          })
          .Map(_ => salesOrder);
      }
      catch (Exception ex) {
        var message = $"Error adding general ledger line with code '{onlineSalesOrderLine.Sku}' (Oid:{onlineSalesOrderLine.Oid}): {ex.Message}";
        _logger.LogError("{error}", message);
        _logger.LogError(ex, "Error adding general ledger line");
        return Result.Failure<SalesOrder>(message);
      }
    }
    private Result<OrderDetail> NewSalesOrderLine(SalesOrder salesOrder) {
      return Result.Success(new OrderDetail {
        TaxMode = salesOrder.TaxMode
      })
      .Bind(salesOrderLine => {
        salesOrder.Detail.Add(salesOrderLine);
        return Result.Success(salesOrderLine);
      });
    }
    private Result<SalesOrder> RepositoryGet<T>(SalesOrder salesOrder, object parameters, Func<object, Result<T>> get, Action<T> onSuccess) {
      return get(parameters).Bind(result => {
        onSuccess(result);
        return Result.Success(salesOrder);
      });
    }
    private Result<SalesOrder> RepositoryGetFromCode<T>(SalesOrder salesOrder, string? parameters, Func<string, Result<T>> get, Action<T> onSuccess) {
      return parameters != null ? RepositoryGet(salesOrder, parameters, p => get((string)p), onSuccess) : Result.Failure<SalesOrder>("Parameters may not be null");
    }
    private Result<OrderDetail> RepositoryGetFromCode<T>(OrderDetail salesOrderLine, string? parameters, Func<string, Result<T>> get, Action<T> onSuccess) {
      return parameters != null ? get(parameters).Bind(result => {
        onSuccess(result);
        return Result.Success(salesOrderLine);
      }) : Result.Failure<OrderDetail>("Parameters may not be null");
    }
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices {
  public abstract class SyncQueueServiceBase {
    protected ILogger Logger { get; }
    private readonly string _typeName = "";
    public SyncQueueServiceBase(ILogger logger) {
      Logger = logger;
      _typeName = GetType().Name;
    }
    public async Task Execute() {
      await Task.Run(() => {
        try {
          Logger.LogDebug("START: Execute {service}", _typeName);
          ProcessQueue();
        }
        catch (Exception ex) {
          Logger.LogError(ex, "While processing queue: {type}", _typeName);
        }
        finally {
          Logger.LogDebug("END: Execute {service}", _typeName);
        }
      });
    }
    protected abstract void ProcessQueue();
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices {
  public abstract class SyncServiceBase {
    protected ILogger Logger { get; }
    public SyncServiceBase(ILogger logger) {
      Logger = logger;
    }
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersPostServices {
  public interface ISalesOrdersPostQueueService : ISyncQueueService {
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersPostServices {
  public interface ISalesOrdersPostService {
    Result<SalesOrdersPostResult> Execute(SalesOrdersPostContext context);
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersPostServices {
  public interface ISalesOrdersPostValueStoreService {
    Result<string?> GetDefaultSalesRepresentativeCode();
    Result<string?> GetDefaultDeliveryMethodCode();
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersPostServices {
  public class SalesOrdersPostQueueService : SyncQueueServiceBase, ISalesOrdersPostQueueService {
    private readonly ISalesOrdersPostService _salesOrdersPostService;
    public SalesOrdersPostQueueService(ILogger<SalesOrdersPostQueueService> logger, ISalesOrdersPostService salesOrdersPostService) : base(logger) {
      _salesOrdersPostService = salesOrdersPostService;
    }
    protected override void ProcessQueue() {
      var context = new SalesOrdersPostContext();
      var result = _salesOrdersPostService.Execute(context);
    }
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersPostServices {
  public class SalesOrdersPostService : SyncServiceBase, ISalesOrdersPostService {
    private readonly ILocalObjectSpaceProvider _localObjectSpaceProvider;
    private readonly IPostToEvolutionSalesOrderService _postToEvolutionSalesOrderService;
    public SalesOrdersPostService(ILogger<SalesOrdersPostService> logger,
      ILocalObjectSpaceProvider localObjectSpaceProvider,
      IPostToEvolutionSalesOrderService postToEvolutionSalesOrderService) : base(logger) {
      _localObjectSpaceProvider = localObjectSpaceProvider;
      _postToEvolutionSalesOrderService = postToEvolutionSalesOrderService;
    }
    public Result<SalesOrdersPostResult> Execute(SalesOrdersPostContext context) {
        var errorCount = 0;
      _localObjectSpaceProvider.WrapInObjectSpaceTransaction(objectSpace => {
        CriteriaOperator? criteria = context.Criteria;
        if (criteria is null) {
          var criteriaPostingStatus = "PostingStatus".InCriteriaOperator(SalesOrderPostingStatus.New, SalesOrderPostingStatus.Retrying);
          var criteriaRetryAfter = CriteriaOperator.Or("RetryAfter".IsNullOperator(), "RetryAfter".PropertyLessThan(DateTime.Now));
          criteria = CriteriaOperator.And(criteriaPostingStatus, criteriaRetryAfter);
        }
        var olineSalesOrders = objectSpace.GetObjects<OnlineSalesOrder>(criteria).ToList();
        if (!olineSalesOrders.Any()) {
          Logger.LogDebug("No sales orders to post");
          return;
        }
        var postToEvolutionSalesOrderContext = new PostToEvolutionSalesOrderContext {
          ObjectSpace = objectSpace
        };
        foreach (var onlineSalesOrder in olineSalesOrders) {
          try {
            Logger.LogInformation("Start posting online sales order. Order Number {OrderNumber}", onlineSalesOrder.OrderNumber);
            if (string.IsNullOrWhiteSpace(onlineSalesOrder.CustomerOnlineId)) {
              Logger.LogError("Customer Online Id is empty. Order Number {OrderNumber}", onlineSalesOrder.OrderNumber);
              onlineSalesOrder.PostLog("Customer Online Id is empty.");
              onlineSalesOrder.PostingStatus = SalesOrderPostingStatus.Failed;
              continue;
            }
            if (string.IsNullOrWhiteSpace(onlineSalesOrder.EmailAddress)) {
              Logger.LogError("Email Address is empty. Order Number {OrderNumber}", onlineSalesOrder.OrderNumber);
              onlineSalesOrder.PostLog("Email Address is empty.");
              onlineSalesOrder.PostingStatus = SalesOrderPostingStatus.Failed;
              continue;
            }
            _postToEvolutionSalesOrderService
              .Post(postToEvolutionSalesOrderContext, onlineSalesOrder)
              .OnFailureCompensate(err => {
                errorCount++;
                Logger.LogError("Error posting sales order Online Order Number {OrderNumber}", onlineSalesOrder.OrderNumber);
                Logger.LogError("{error}", err);
                onlineSalesOrder.PostLog(err);
                if (onlineSalesOrder.RetryCount < 6) {
                  onlineSalesOrder.PostingStatus = SalesOrderPostingStatus.Retrying;
                  onlineSalesOrder.RetryCount++;
                  onlineSalesOrder.RetryAfter = DateTime.Now.AddMinutes(onlineSalesOrder.RetryCount switch {
                    1 => 10,
                    2 => 10,
                    3 => 15,
                    4 => 30,
                    5 => 60,
                    _ => 60
                  });
                }
                else {
                  onlineSalesOrder.PostingStatus = SalesOrderPostingStatus.Failed;
                }
                return Result.Failure<OnlineSalesOrder>(err);
              });
          }
          catch (Exception ex) {
            Logger.LogError(ex, "Error posting online sales order number {OrderNumber}", onlineSalesOrder.OrderNumber);
            onlineSalesOrder.PostLog(ex.Message, ex);
          }
          finally {
            onlineSalesOrder.Save();
            Logger.LogInformation("End posting online sales order. Order Number {OrderNumber}", onlineSalesOrder.OrderNumber);
          }
        }
      });
      return errorCount == 0 ? Result.Success(BuildResult()) : Result.Failure<SalesOrdersPostResult>($"Completed with {errorCount} errors.");
      SalesOrdersPostResult BuildResult() {
        return new SalesOrdersPostResult { };
      }
    }
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersPostServices {
  public class SalesOrdersPostValueStoreService : ISalesOrdersPostValueStoreService {
    private readonly IValueStoreRepository _valueStoreRepository;
    public SalesOrdersPostValueStoreService(IValueStoreRepository valueStoreRepository) {
      _valueStoreRepository = valueStoreRepository;
    }
    public Result<string?> GetDefaultSalesRepresentativeCode() {
      return _valueStoreRepository.GetValue("default-sales-rep-code");
    }
    public Result<string?> GetDefaultDeliveryMethodCode() {
      return _valueStoreRepository.GetValue("default-delivery-method-description");
    }
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersPostServices.Models {
  public record SalesOrdersPostContext(CriteriaOperator? Criteria = null);
}
namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersPostServices.Models {
  public record SalesOrdersPostResult {
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersSyncServices {
  public interface ISalesOrdersSyncQueueService : ISyncQueueService {
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersSyncServices {
  public interface ISalesOrdersSyncService {
    Result<SalesOrdersSyncResult> Execute(SalesOrdersSyncContext context);
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersSyncServices {
  public interface ISalesOrdersSyncValueStoreService {
    Result<DateTime?> GetSalesOrdersSyncLastSynced();
    Result UpdateSalesOrdersSyncLastSynced(DateTime? dateTime);
    Result<int?> GetFetchNumberOfDaysBack();
    Result<string?> GetShippingGeneralLedgerAccountCode();
    Result<string?> GetShippingTaxType();
    Result<string?> GetChannelProjectCode(string? channel);
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersSyncServices {
  public record SalesOrdersSyncParameters {
    public double FetchNumberOfDaysBack { get; init; }
    public string? ShippingGeneralLedgerAccountCode { get; init; }
    public string? ShippingTaxType { get; init; }
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersSyncServices {
  public class SalesOrdersSyncQueueService : SyncQueueServiceBase, ISalesOrdersSyncQueueService {
    private readonly ISalesOrdersSyncService _salesOrdersSyncService;
    public SalesOrdersSyncQueueService(ILogger<SalesOrdersSyncQueueService> logger, ISalesOrdersSyncService salesOrdersSyncService) : base(logger) {
      _salesOrdersSyncService = salesOrdersSyncService;
    }
    protected override void ProcessQueue() {
      var context = new SalesOrdersSyncContext();
      _salesOrdersSyncService.Execute(context);
    }
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersSyncServices {
  public class SalesOrdersSyncService : SyncServiceBase, ISalesOrdersSyncService {
    private readonly SalesOrdersSyncJobOptions _salesOrdersSyncJobOptions;
    private readonly ISalesOrdersSyncValueStoreService _salesOrdersSyncValueStoreService;
    private readonly ISalesOrdersApiClient _apiClient;
    private readonly ILocalObjectSpaceProvider _localObjectSpaceProvider;
    private readonly IAppDataFileManager _appDataFileManager;
    private readonly List<string> _processedOrders = new();
    public SalesOrdersSyncService(ILogger<SalesOrdersSyncService> logger,
      SalesOrdersSyncJobOptions salesOrdersSyncJobOptions,
      ISalesOrdersSyncValueStoreService salesOrdersSyncValueStoreService,
      ISalesOrdersApiClient apiClient,
      ILocalObjectSpaceProvider localObjectSpaceProvider,
      IAppDataFileManager appDataFileManager)
      : base(logger) {
      _salesOrdersSyncJobOptions = salesOrdersSyncJobOptions;
      _salesOrdersSyncValueStoreService = salesOrdersSyncValueStoreService;
      _apiClient = apiClient;
      _localObjectSpaceProvider = localObjectSpaceProvider;
      _appDataFileManager = appDataFileManager;
    }
    public Result<SalesOrdersSyncResult> Execute(SalesOrdersSyncContext context) {
      var errorCount = 0;
      var parametersResult = LoadParameters();
      if (parametersResult.IsFailure) { return Result.Failure<SalesOrdersSyncResult>(parametersResult.Error); }
      var parameters = parametersResult.Value;
      try {
        var lastSynced = _salesOrdersSyncValueStoreService.GetSalesOrdersSyncLastSynced();
        if (lastSynced.IsFailure) {
          Logger.LogError("Unable to execute SalesOrdersSyncService: {error}", lastSynced.Error);
          return Result.Failure<SalesOrdersSyncResult>(lastSynced.Error);
        }
        var orderSubmittedDate = (lastSynced.Value ?? DateTime.Now).AddDays(-parameters.FetchNumberOfDaysBack).Date;
        Logger.LogInformation("Fetching sales orders since: {orderSubmittedDate}", $"{orderSubmittedDate:yyyy-MM-dd HH:mm:ss}");
        var salesOrdersResult = _apiClient.GetSalesOrders(orderSubmittedDate);
        if (salesOrdersResult.IsFailure) {
          Logger.LogError("Unable to execute SalesOrdersSyncService: {error}", salesOrdersResult.Error);
          return Result.Failure<SalesOrdersSyncResult>(salesOrdersResult.Error);
        }
        var response = salesOrdersResult.Value;
        var lastOrderDate = DateTime.MinValue;
        if (response.SalesOrders == null || response.SalesOrders!.Length == 0) {
          Logger.LogInformation("No sales orders found since: {orderSubmittedDate}", $"{orderSubmittedDate:yyyy-MM-dd HH:mm:ss}");
          return Result.Success(BuildResult("No new orders"));
        }
        Result<string?> MapChannelToProjectCode(string? channel) {
          return _salesOrdersSyncValueStoreService
            .GetChannelProjectCode(channel)
            .OnFailureCompensate(error => Result.Failure<string?>($"Unable to map channel to project code: {error}"));
        }
        bool ContainsKey(dynamic o, string name) {
          var o1 = o as Newtonsoft.Json.Linq.JObject;
          return o1?.ContainsKey(name) ?? false;
        }
        var channelsToProcess = _salesOrdersSyncJobOptions.ChannelsToProcess?.Select(c => c.ToLower()).ToList();
        _localObjectSpaceProvider.WrapInObjectSpaceTransaction(objectSpace => {
          foreach (dynamic salesOrder in response.SalesOrders!) {
            Logger.LogInformation("Sales order received: {orderNumber}", $"{salesOrder.orderNumber}");
            var channel = $"{salesOrder.channel}";
            var processChannel = channelsToProcess?.Contains(channel.ToLower()) ?? false;
            if (!processChannel) {
              Logger.LogWarning("Sales order channel not configured to process: {orderNumber} [channel:{channel}]", $"{salesOrder.orderNumber}", $"{channel}");
              continue;
            }
            var orderDate = (DateTime)salesOrder.orderSubmittedDate;
            lastOrderDate = lastOrderDate > orderDate ? lastOrderDate : orderDate;
            if (_processedOrders.Contains($"{salesOrder.id}")) {
              Logger.LogWarning("Sales order already processed: {orderNumber} [id:{id}]", $"{salesOrder.orderNumber}", $"{salesOrder.id}");
              continue;
            }
            _processedOrders.Add($"{salesOrder.id}");
            var localSalesOrder = NewOrder(salesOrder.id, salesOrder.orderNumber);
            if (localSalesOrder.OnlineId == $"{salesOrder.id}") {
              Logger.LogWarning("Sales order already exists: {orderNumber} [id:{id}]", $"{salesOrder.orderNumber}", $"{salesOrder.id}");
              continue;
            }
            localSalesOrder.OrderNumber = salesOrder.orderNumber;
            localSalesOrder.OrderJson = JsonConvert.SerializeObject(salesOrder, Formatting.Indented);
            _appDataFileManager.StoreText("json-sales-orders", $"{localSalesOrder.OrderNumber}.json", localSalesOrder.OrderJson);
            //Logger.LogInformation("{json}", localSalesOrder.OrderJson);
            var channelProjectCode = MapChannelToProjectCode(channel);
            if (channelProjectCode.IsFailure) { throw new Exception(channelProjectCode.Error); }
            localSalesOrder.CustomerOnlineId = salesOrder.customerId;
            if (ContainsKey(salesOrder.customer, "emails")) {
              localSalesOrder.EmailAddress = salesOrder.customer.emails.Count > 0 ? salesOrder.customer.emails[0]?.email : null;
            }
            localSalesOrder.OnlineId = salesOrder.id;
            localSalesOrder.Channel = channel;
            localSalesOrder.OrderDate = orderDate;
            localSalesOrder.ProjectCode = channelProjectCode.Value;
            if (salesOrder.shipTo != null) {
              localSalesOrder.ShipToName = BuildShipToName($"{salesOrder.shipTo.firstName}", $"{salesOrder.shipTo.lastName}");
              localSalesOrder.ShipToPhoneNumber = salesOrder.shipTo.phone;
              localSalesOrder.ShipToAddress1 = salesOrder.shipTo.address;
              localSalesOrder.ShipToAddress2 = salesOrder.shipTo.address2;
              localSalesOrder.ShipToAddressCity = salesOrder.shipTo.city;
              localSalesOrder.ShipToAddressProvince = salesOrder.shipTo.stateCode;
              localSalesOrder.ShipToAddressPostalCode = salesOrder.shipTo.zipCode;
              localSalesOrder.ShipToAddressCountryCode = salesOrder.shipTo.countryCode;
            }
            foreach (var item in salesOrder.items) {
              var localSalesOrderLine = NewLine(localSalesOrder);
              localSalesOrderLine.OnlineId = item.id;
              localSalesOrderLine.LineType = SalesOrderLineType.Inventory;
              localSalesOrderLine.Sku = NormalizeItemCode($"{item.sku}");
              localSalesOrderLine.LineDescription = BuildLineDescription($"{item.productTitle}", $"{item.productVariantTitle}");
              localSalesOrderLine.Quantity = item.quantity;
              localSalesOrderLine.TaxAmount = ConvertCurrencyValue(item.tax);
              localSalesOrderLine.OnlineTaxType = item.taxType;
              localSalesOrderLine.UnitPriceInVat = ConvertCurrencyValue(item.price);
              if (!string.IsNullOrWhiteSpace($"{item.notes}")) { localSalesOrderLine.LineNotes = $"{item.notes}"; }
              localSalesOrderLine.Save();
              localSalesOrder.OrderValueInVat += localSalesOrderLine.LineValueInVat;
            }
            if (salesOrder.shipping != null) {
              foreach (var shipping in salesOrder.shipping) {
                var price = ConvertCurrencyValue(shipping.price);
                if (price == 0) { continue; }
                var localSalesOrderLine = NewLine(localSalesOrder);
                localSalesOrderLine.OnlineId = shipping.id;
                localSalesOrderLine.LineType = SalesOrderLineType.GeneralLedger;
                localSalesOrderLine.Sku = NormalizeItemCode(parameters.ShippingGeneralLedgerAccountCode);
                localSalesOrderLine.LineDescription = shipping.title;
                localSalesOrderLine.Quantity = 1;
                localSalesOrderLine.TaxAmount = ConvertCurrencyValue(shipping.tax);
                localSalesOrderLine.OnlineTaxType = parameters.ShippingTaxType;
                localSalesOrderLine.UnitPriceInVat = ConvertCurrencyValue(shipping.price);
                localSalesOrder.Save();
                localSalesOrder.OrderValueInVat += localSalesOrderLine.LineValueInVat;
              }
            }
            localSalesOrder.Save();
          }
          OnlineSalesOrder NewOrder(dynamic id, dynamic orderNumber) {
            var criteria = CriteriaOperator.Or("OnlineId".IsEqualToOperator($"{id}"), "OrderNumber".IsEqualToOperator($"{orderNumber}"));
            var orders = objectSpace.GetObjects<OnlineSalesOrder>(criteria);
            if (orders.Count > 1) {
              Logger.LogWarning("Multiple sales orders found:[Order number:{orderNumber}][id:{id}]", $"{orderNumber}", $"{id}");
            }
            var order = orders.FirstOrDefault();
            return order ?? objectSpace.CreateObject<OnlineSalesOrder>();
          }
          OnlineSalesOrderLine NewLine(OnlineSalesOrder localSalesOrder) {
            var localSalesOrderLine = objectSpace.CreateObject<OnlineSalesOrderLine>();
            localSalesOrder.SalesOrderLines.Add(localSalesOrderLine);
            return localSalesOrderLine;
          }
        });
        _salesOrdersSyncValueStoreService.UpdateSalesOrdersSyncLastSynced(lastOrderDate.Date);
        return errorCount == 0 ? Result.Success(BuildResult()) : Result.Failure<SalesOrdersSyncResult>($"Completed with {errorCount} errors.");
      }
      catch (Exception ex) {
        Logger.LogError("Unable to execute SalesOrdersSyncService: {error}", ex);
        return Result.Failure<SalesOrdersSyncResult>(ex.Message);
      }
      SalesOrdersSyncResult BuildResult(string? message = null) {
        return new SalesOrdersSyncResult { Message = message };
      }
    }
    private Result<SalesOrdersSyncParameters> LoadParameters() {
      Result<SalesOrdersSyncParameters> GetResult<T>(SalesOrdersSyncParameters p, Func<Result<T?>> getValue, Func<SalesOrdersSyncParameters, T?, Result<SalesOrdersSyncParameters>> onFound) {
        return getValue().Bind(value => onFound(p, value));
      }
      Result<SalesOrdersSyncParameters> ParameterNotDefined(string keyName) {
        var error = $"{keyName} is not defined in ValueStore table";
        Logger.LogError("{error}", error);
        return Result.Failure<SalesOrdersSyncParameters>(error);
      }
      var parameters = new SalesOrdersSyncParameters { };
      return GetResult(parameters,
      _salesOrdersSyncValueStoreService.GetShippingGeneralLedgerAccountCode, (p, v) => {
        if (string.IsNullOrWhiteSpace(v)) { return ParameterNotDefined("sales-orders-sync-shipping-general-ledger-account-code"); }
        return p with { ShippingGeneralLedgerAccountCode = v };
      })
      .Bind(pa => GetResult(pa, _salesOrdersSyncValueStoreService.GetShippingTaxType, (p, v) => {
        if (string.IsNullOrWhiteSpace(v)) { return ParameterNotDefined("sales-orders-sync-shipping-tax-type"); }
        return p with { ShippingTaxType = v };
      }))
      .Bind(pa => GetResult(pa, _salesOrdersSyncValueStoreService.GetFetchNumberOfDaysBack, (p, v) => p with { FetchNumberOfDaysBack = v ?? 3 }))
      ;
    }
    private static string BuildShipToName(string? firstName, string? lastName) {
      return $"{(firstName ?? "").Trim()} {(lastName ?? "").Trim()}".Trim();
    }
    private static string BuildLineDescription(string? productTitle, string? productVariantTitle) {
      return $"{(productTitle ?? "").Trim()} {(productVariantTitle ?? "").Trim()}".Trim();
    }
    private static double ConvertCurrencyValue(dynamic amount) {
      if (double.TryParse($"{amount}", out var result)) {
        return result / 100d;
      }
      return 0d;
    }
    private static string NormalizeItemCode(string? code) => (code ?? "").Trim().ToUpper();
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersSyncServices {
  public class SalesOrdersSyncValueStoreService : ISalesOrdersSyncValueStoreService {
    private readonly IValueStoreRepository _valueStoreRepository;
    public SalesOrdersSyncValueStoreService(IValueStoreRepository valueStoreRepository) {
      _valueStoreRepository = valueStoreRepository;
    }
    private Result<DateTime?> GetSalesOrdersSyncStartDate() => _valueStoreRepository.GetDateTimeValue("sales-orders-sync-start-date", new DateTime(2023, 1, 1));
    public Result UpdateSalesOrdersSyncLastSynced(DateTime? dateTime) => _valueStoreRepository.SetDateTimeValue("sales-orders-sync-last-synced", dateTime);
    public Result<DateTime?> GetSalesOrdersSyncLastSynced() {
      return GetSalesOrdersSyncStartDate().Bind(startDate => {
        return _valueStoreRepository.GetDateTimeValue("sales-orders-sync-last-synced", startDate);
      });
    }
    public Result<int?> GetFetchNumberOfDaysBack() {
      return _valueStoreRepository.GetIntValue("sales-orders-sync-fetch-number-of-days-back", 3);
    }
    public Result<string?> GetShippingGeneralLedgerAccountCode() {
      return _valueStoreRepository.GetValue("sales-orders-sync-shipping-general-ledger-account-code");
    }
    public Result<string?> GetShippingTaxType() {
      return _valueStoreRepository.GetValue("sales-orders-sync-shipping-tax-type");
    }
    public Result<string?> GetChannelProjectCode(string? channel) {
      if (string.IsNullOrWhiteSpace(channel)) { return Result.Failure<string?>("Channel provided is empty."); }
      var keyName = $"sales-orders-sync-channel-{channel.Trim().ToLower()}-project-code";
      return _valueStoreRepository
        .GetValue(keyName)
        .Bind(p => string.IsNullOrWhiteSpace(p)
            ? Result.Failure<string?>($"No project code mapping found for channel: '{channel}'. Please add a record to ValueStore for key: {keyName}")
            : Result.Success<string?>(p));
    }
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersSyncServices.Models {
  public record SalesOrdersSyncContext { }
}
namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersSyncServices.Models {
  public record SalesOrdersSyncResponse {
    public dynamic[]? SalesOrders { get; init; }
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersSyncServices.Models {
  public record SalesOrdersSyncResult {
    public string? Message { get; init; }
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.RestApiClients {
  public abstract class ApiClientBase {
    private readonly IApiClientService _apiClientService;
    protected ILogger Logger { get; }
    public ApiClientBase(ILogger logger, IApiClientService apiClientService) {
      Logger = logger;
      _apiClientService = apiClientService;
    }
    protected Result<T> SendRequest<T>(ApiRequestBase apiRequest, Func<dynamic, (Result<T> result, ApiRequestPaginationStatus paginationStatus)> onSuccess, Action<RestRequest>? configRestRequest = null) {
      while (true) {
        if (apiRequest.IsPagedResponse) {
          apiRequest = apiRequest with { CurrentPage = apiRequest.CurrentPage + 1 };
        }
        var response = _apiClientService.Execute(apiRequest, configRestRequest: configRestRequest);
        if (response is ApiResponseBase.Failure f) {
          Logger.LogError("{error}", f.FullErrorMessage);
          return Result.Failure<T>(f.FullErrorMessage);
        }
        var successResponse = response as ApiResponseBase.Success;
        if (apiRequest.DeserializeBody && IsResponseBodyEmpty(successResponse!)) { return Result.Failure<T>("Response body empty."); }
        dynamic data = new { };
        if (apiRequest.DeserializeBody) {
          var dataResult = Deserialize<dynamic>(successResponse!);
          if (dataResult.IsFailure) { return Result.Failure<T>(dataResult.Error); }
          data = dataResult.Value;
        }
        (Result<T> result, ApiRequestPaginationStatus paginationStatus) r = onSuccess(data);
        if (r.result.IsFailure || r.paginationStatus == ApiRequestPaginationStatus.Completed) { return r.result; }
      }
    }
    protected bool IsResponseBodyEmpty(ApiResponseBase responseBase) {
      return string.IsNullOrWhiteSpace(responseBase.ResonseBody);
    }
    protected Result<T> Deserialize<T>(Result<ApiResponseBase.Success> response) {
      try {
        var value = response.Value;
        var data = JsonFunctions.Deserialize<T>(value.ResonseBody ?? "");
        if (data == null) {
          Logger.LogError("Response body not valid JSON.\r\n{body}", value?.ResonseBody);
          return Result.Failure<T>("Response body not valid JSON.");
        }
        return Result.Success(data);
      }
      catch (Exception ex) {
        Logger.LogError(ex, "While deserializing response");
        return Result.Failure<T>(ex.Message);
      }
    }
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.RestApiClients {
  public class ApiClientService : IApiClientService {
    private readonly ILogger<ApiClientService> _logger;
    private readonly IRestClientFactory _restClient;
    public ApiClientService(ILogger<ApiClientService> logger, IRestClientFactory commerce7RestClient) {
      _logger = logger;
      _restClient = commerce7RestClient;
    }
    public ApiResponseBase Execute(ApiRequestBase apiRequest, Action<RestClientOptions>? configOptions = null, Action<RestRequest>? configRestRequest = null) {
      var requestResource = apiRequest.GetResource();
      var json = JsonFunctions.Serialize(apiRequest.Data);
      var useJson = apiRequest.Data != null && !string.IsNullOrWhiteSpace(json) && json != "null";
      _logger.LogInformation("Send Commerce7 request {method}:{resource}{json}", apiRequest.Method, requestResource, $":\r\n{json}" ?? "");
      try {
        var result = _restClient.NewRestClient(configOptions)
          .Bind(client => {
            var method = apiRequest.Method;
            var request = new RestRequest(requestResource, method);
            if (useJson) { request.AddJsonBody(json!); }
            configRestRequest?.Invoke(request);
            var response = client.Execute(request);
            _logger.LogDebug("{0} Commerce7 response status code {1}", requestResource, response.StatusCode);
            if (!response.IsSuccessful) {
              return Result.Success((ApiResponseBase)new ApiResponseBase.Failure {
                Uri = response.ResponseUri?.AbsoluteUri ?? "",
                ErrorMessage = response.ErrorMessage,
                ErrorException = response.ErrorException,
                ResonseBody = response.Content,
                StatusCode = $"{response.StatusCode}",
                StatusDescription = response.StatusDescription
              });
            }
            var responseIsBinaryData = response.ContentType?.Equals("image/jpeg", StringComparison.InvariantCultureIgnoreCase) ?? false;
            return Result.Success((ApiResponseBase)new ApiResponseBase.Success {
              ResonseBody = responseIsBinaryData ? null : response.Content,
              ResponseRawBytes = responseIsBinaryData ? response.RawBytes : null
            });
          });
        return result.IsSuccess ? result.Value : ReturnFailure(result);
      }
      catch (Exception ex) {
        _logger.LogError(ex, "While sending HTTP request: {resource}", requestResource);
        return new ApiResponseBase.Failure { ErrorException = ex };
      }
      ApiResponseBase ReturnFailure(Result<ApiResponseBase> result) {
        _logger.LogError("While sending HTTP request: {resource}", requestResource);
        _logger.LogError("{error}", result.Error);
        return new ApiResponseBase.Failure { ErrorMessage = result.Error, StatusCode = "Error" };
      }
    }
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.RestApiClients {
  public enum ApiRequestPaginationStatus {
    MorePages,
    Completed
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.RestApiClients {
  public interface IApiClientService {
    ApiResponseBase Execute(ApiRequestBase apiRequest, Action<RestClientOptions>? configOptions = null, Action<RestRequest>? configRestRequest = null);
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.RestApiClients {
  public interface IRestClientFactory {
    Result<RestClient> NewRestClient(Action<RestClientOptions>? configOptions = null);
    Result<RestClient> SetAuthorization(RestClient client, string token);
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.RestApiClients {
  public interface ISalesOrdersApiClient {
    Result<SalesOrdersSyncResponse> GetSalesOrders(DateTime orderSubmittedDate);
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.RestApiClients {
  public class RestClientFactory : IRestClientFactory {
    private readonly ApiOptions _apiOptions;
    public RestClientFactory(ApiOptions apiOptions) {
      _apiOptions = apiOptions;
    }
    public Result<RestClient> NewRestClient(Action<RestClientOptions>? configOptions = null) {
      if (_apiOptions == null) { return Result.Failure<RestClient>("ApiOptions cannot be empty. Please define a ApiOptions section in the appsettings.json file under ApplicationOptions"); }
      if (string.IsNullOrWhiteSpace(_apiOptions.Endpoint)) { return Result.Failure<RestClient>("Endpoint cannot be empty. Please define the Endpoint in the appsettings.json file under ApplicationOptions.ApiOptions.Endpoint"); }
      if (string.IsNullOrWhiteSpace(_apiOptions.TenantId)) { return Result.Failure<RestClient>("TenantId cannot be empty. Please define the TenantId in the appsettings.json file under ApplicationOptions.ApiOptions.TenantId"); }
      if (string.IsNullOrWhiteSpace(_apiOptions.AppId)) { return Result.Failure<RestClient>("AppId cannot be empty. Please define the AppId in the appsettings.json file under ApplicationOptions.ApiOptions.AppId"); }
      if (string.IsNullOrWhiteSpace(_apiOptions.AppSecretKey)) { return Result.Failure<RestClient>("AppSecretKey cannot be empty. Please define the AppSecretKey in the appsettings.json file under ApplicationOptions.ApiOptions.AppSecretKey"); }
      var options = new RestClientOptions(_apiOptions.Endpoint!) {
        Authenticator = new HttpBasicAuthenticator(_apiOptions.AppId!, _apiOptions.AppSecretKey!)
      };
      configOptions?.Invoke(options);
      var client = new RestClient(options) { };
      client.AddDefaultHeader("tenant", $"{_apiOptions.TenantId.Trim()}");
      return Result.Success(client);
    }
    public Result<RestClient> SetAuthorization(RestClient client, string token) {
      if (string.IsNullOrWhiteSpace(token)) { return Result.Failure<RestClient>("ApiToken cannot be empty. First do an AUTH call to get a token."); }
      client.AddDefaultHeader("Authorization", $"{token.Trim()}");
      return Result.Success(client);
    }
  }
}
namespace BosmanCommerce7.Module.ApplicationServices.RestApiClients {
  public class SalesOrdersApiClient : ApiClientBase, ISalesOrdersApiClient {
    public SalesOrdersApiClient(ILogger<SalesOrdersApiClient> logger, IApiClientService apiClientService) : base(logger, apiClientService) {
    }
    public Result<SalesOrdersSyncResponse> GetSalesOrders(DateTime orderSubmittedDate) {
      SalesOrdersSyncResponse apiResponse = new();
      var list = new List<dynamic>();
      return SendRequest(new SalesOrdersSyncApiRequest(orderSubmittedDate), data => {
        if (data == null) {
          return (Result.Failure<SalesOrdersSyncResponse>("Response body not valid JSON."), ApiRequestPaginationStatus.Completed);
        }
        var totalRecords = (int)data!.total;
        foreach (var order in data!.orders) { list.Add(order); }
        apiResponse = apiResponse with { SalesOrders = list.ToArray() };
        return (Result.Success(apiResponse), list.Count < totalRecords ? ApiRequestPaginationStatus.MorePages : ApiRequestPaginationStatus.Completed);
      });
    }
  }
}
namespace BosmanCommerce7.Module.BusinessObjects {
  [DefaultClassOptions]
  [NavigationItem(true)]
  [DefaultProperty(nameof(OrderNumber))]
  public class OnlineSalesOrder : XPObject {
    private string? _customerId;
    private string? _emailAddress;
    private string? _onlineId;
    private string? _channel;
    private DateTime _orderDate;
    private int? _orderNumber;
    private string? _evolutionSalesOrderNumber;
    private double _orderValueInVat;
    private string? _projectCode;
    private string? _shipToName;
    private string? _shipToPhoneNumber;
    private string? _shipToAddress1;
    private string? _shipToAddress2;
    private string? _shipToAddressCity;
    private string? _shipToAddressProvince;
    private string? _shipToAddressPostalCode;
    private string? _shipToAddressCountryCode;
    private string? _lastErrorMessage;
    private string? _orderJson;
    private SalesOrderPostingStatus _postingStatus;
    private int _retryCount;
    private DateTime? _retryAfter;
    private DateTime? _datePosted;
    [Size(40)]
    public string? CustomerOnlineId {
      get => _customerId;
      set => SetPropertyValue(nameof(CustomerOnlineId), ref _customerId, value);
    }
    public string? EmailAddress {
      get => _emailAddress;
      set => SetPropertyValue(nameof(EmailAddress), ref _emailAddress, value);
    }
    [Size(40)]
    [ModelDefault("AllowEdit", "false")]
    [Indexed(Unique = true)]
    public string? OnlineId {
      get => _onlineId;
      set => SetPropertyValue(nameof(OnlineId), ref _onlineId, value);
    }
    [ModelDefault("AllowEdit", "false")]
    public string? Channel {
      get => _channel;
      set => SetPropertyValue(nameof(Channel), ref _channel, value);
    }
    [ModelDefault("AllowEdit", "false")]
    public string? EvolutionSalesOrderNumber {
      get => _evolutionSalesOrderNumber;
      set => SetPropertyValue(nameof(EvolutionSalesOrderNumber), ref _evolutionSalesOrderNumber, value);
    }
    [ModelDefault("DisplayFormat", "{0:yyyy/MM/dd HH:mm:ss}")]
    [ModelDefault("EditMask", "yyyy/MM/dd HH:mm:ss")]
    [ModelDefault("AllowEdit", "false")]
    public DateTime OrderDate {
      get => _orderDate;
      set => SetPropertyValue(nameof(OrderDate), ref _orderDate, value);
    }
    [ModelDefault("AllowEdit", "false")]
    [Indexed(Unique = true)]
    public int? OrderNumber {
      get => _orderNumber;
      set => SetPropertyValue(nameof(OrderNumber), ref _orderNumber, value);
    }
    public string? ShipToName {
      get => _shipToName;
      set => SetPropertyValue(nameof(ShipToName), ref _shipToName, value);
    }
    public string? ShipToPhoneNumber {
      get => _shipToPhoneNumber;
      set => SetPropertyValue(nameof(ShipToPhoneNumber), ref _shipToPhoneNumber, value);
    }
    [Size(200)]
    public string? ShipToAddress1 {
      get => _shipToAddress1;
      set => SetPropertyValue(nameof(ShipToAddress1), ref _shipToAddress1, value);
    }
    [Size(200)]
    public string? ShipToAddress2 {
      get => _shipToAddress2;
      set => SetPropertyValue(nameof(ShipToAddress2), ref _shipToAddress2, value);
    }
    public string? ShipToAddressCity {
      get => _shipToAddressCity;
      set => SetPropertyValue(nameof(ShipToAddressCity), ref _shipToAddressCity, value);
    }
    public string? ShipToAddressProvince {
      get => _shipToAddressProvince;
      set => SetPropertyValue(nameof(ShipToAddressProvince), ref _shipToAddressProvince, value);
    }
    public string? ShipToAddressPostalCode {
      get => _shipToAddressPostalCode;
      set => SetPropertyValue(nameof(ShipToAddressPostalCode), ref _shipToAddressPostalCode, value);
    }
    public string? ShipToAddressCountryCode {
      get => _shipToAddressCountryCode;
      set => SetPropertyValue(nameof(ShipToAddressCountryCode), ref _shipToAddressCountryCode, value);
    }
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [ModelDefault("AllowEdit", "false")]
    public double OrderValueInVat {
      get => _orderValueInVat;
      set => SetPropertyValue(nameof(OrderValueInVat), ref _orderValueInVat, value);
    }
    [ModelDefault("AllowEdit", "false")]
    public string? ProjectCode {
      get => _projectCode;
      set => SetPropertyValue(nameof(ProjectCode), ref _projectCode, value);
    }
    [ModelDefault("AllowEdit", "false")]
    public SalesOrderPostingStatus PostingStatus {
      get => _postingStatus;
      set => SetPropertyValue(nameof(PostingStatus), ref _postingStatus, value);
    }
    [ModelDefault("AllowEdit", "false")]
    public int RetryCount {
      get => _retryCount;
      set => SetPropertyValue(nameof(RetryCount), ref _retryCount, value);
    }
    [ModelDefault("DisplayFormat", "{0:yyyy/MM/dd HH:mm:ss}")]
    [ModelDefault("EditMask", "yyyy/MM/dd HH:mm:ss")]
    [ModelDefault("AllowEdit", "false")]
    public DateTime? RetryAfter {
      get => _retryAfter;
      set => SetPropertyValue(nameof(RetryAfter), ref _retryAfter, value);
    }
    [ModelDefault("DisplayFormat", "{0:yyyy/MM/dd HH:mm:ss}")]
    [ModelDefault("EditMask", "yyyy/MM/dd HH:mm:ss")]
    [ModelDefault("AllowEdit", "false")]
    public DateTime? DatePosted {
      get => _datePosted;
      set => SetPropertyValue(nameof(DatePosted), ref _datePosted, value);
    }
    [ModelDefault("AllowEdit", "false")]
    public string? LastErrorMessage {
      get => _lastErrorMessage;
      set => SetPropertyValue(nameof(LastErrorMessage), ref _lastErrorMessage, value);
    }
    [Size(-1)]
    [ModelDefault("AllowEdit", "false")]
    public string? OrderJson {
      get => _orderJson;
      set => SetPropertyValue(nameof(OrderJson), ref _orderJson, value);
    }
    [Browsable(false)]
    public bool IsStoreOrder => Channel?.Equals("web", StringComparison.InvariantCultureIgnoreCase) ?? false;
    [Association("OnlineSalesOrder-OnlineSalesOrderLine")]
    [Aggregated]
    [ModelDefault("AllowEdit", "false")]
    public XPCollection<OnlineSalesOrderLine> SalesOrderLines => GetCollection<OnlineSalesOrderLine>(nameof(SalesOrderLines));
    [Association("OnlineSalesOrder-OnlineSalesOrderProcessingLog")]
    [Aggregated]
    [ModelDefault("AllowEdit", "false")]
    public XPCollection<OnlineSalesOrderProcessingLog> SalesOrderProcessingLogs => GetCollection<OnlineSalesOrderProcessingLog>(nameof(SalesOrderProcessingLogs));
    public OnlineSalesOrder(Session session) : base(session) { }
    public override void AfterConstruction() {
      base.AfterConstruction();
      PostingStatus = SalesOrderPostingStatus.New;
      RetryAfter = DateTime.Now;
    }
    public void PostLog(string shortDescription, Exception ex) {
      PostLog(shortDescription, ExceptionFunctions.GetMessages(ex));
    }
    public void PostLog(string shortDescription, string? details = null) {
      if (shortDescription.Length > 100) {
        details = shortDescription + "\r\n" + (details ?? "");
        shortDescription = shortDescription[..100];
      }
      LastErrorMessage = shortDescription;
      var log = new OnlineSalesOrderProcessingLog(Session) {
        ShortDescription = shortDescription,
        Details = details
      };
      log.Save();
      SalesOrderProcessingLogs.Add(log);
    }
    public void ResetPostingStatus() {
      RetryAfter = DateTime.Now;
      RetryCount = 0;
      PostingStatus = SalesOrderPostingStatus.New;
      LastErrorMessage = null;
    }
    public Address ShipToAddress() {
      var list = new List<string>();
      string Normalize(string? value) {
        value = value?.Trim() ?? "";
        var x = Math.Min(value.Length, 40);
        return value[..x];
      }
      void Add(string? value) {
        if (string.IsNullOrWhiteSpace(value)) { return; }
        if (!value.Contains(',')) {
          list.Add(Normalize(value));
          return;
        }
        value.Split(',').ToList().ForEach(x => list.Add(Normalize(x)));
      }
      if (IsStoreOrder) { Add(ShipToName); }
      Add(ShipToAddress1);
      Add(ShipToAddress2);
      Add(ShipToAddressCity);
      if (IsStoreOrder) { Add(ShipToPhoneNumber); }
      Add(ShipToAddressProvince);
      Add(ShipToAddressCountryCode);
      list = list.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
      string GetLine(int i) => list!.Count > i ? list![i] : "";
      return new Address(
          GetLine(0),
          GetLine(1),
          GetLine(2),
          GetLine(3),
          GetLine(4),
          Normalize(ShipToAddressPostalCode));
    }
  }
}
namespace BosmanCommerce7.Module.BusinessObjects {
  [DefaultClassOptions]
  [NavigationItem(false)]
  public class OnlineSalesOrderLine : XPObject {
    private OnlineSalesOrder? _onlineSalesOrder;
    private string? _onlineId;
    private SalesOrderLineType _lineType;
    private string? _sku;
    private string? _lineDescription;
    private double _quantity;
    private double _taxAmount;
    private string? _onlineTaxType;
    private double _unitPriceInVat;
    private string? _lineNotes;
    [Association("OnlineSalesOrder-OnlineSalesOrderLine")]
    public OnlineSalesOrder? OnlineSalesOrder {
      get => _onlineSalesOrder;
      set => SetPropertyValue(nameof(OnlineSalesOrder), ref _onlineSalesOrder, value);
    }
    [Size(40)]
    [ModelDefault("AllowEdit", "false")]
    public string? OnlineId {
      get => _onlineId;
      set => SetPropertyValue(nameof(OnlineId), ref _onlineId, value);
    }
    [ModelDefault("AllowEdit", "false")]
    public SalesOrderLineType LineType {
      get => _lineType;
      set => SetPropertyValue(nameof(LineType), ref _lineType, value);
    }
    [Size(20)]
    [ModelDefault("AllowEdit", "false")]
    public string? Sku {
      get => _sku;
      set => SetPropertyValue(nameof(Sku), ref _sku, value);
    }
    [ModelDefault("AllowEdit", "false")]
    public string? LineDescription {
      get => _lineDescription;
      set => SetPropertyValue(nameof(LineDescription), ref _lineDescription, value);
    }
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [ModelDefault("AllowEdit", "false")]
    public double Quantity {
      get => _quantity;
      set => SetPropertyValue(nameof(Quantity), ref _quantity, value);
    }
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [ModelDefault("AllowEdit", "false")]
    public double TaxAmount {
      get => _taxAmount;
      set => SetPropertyValue(nameof(TaxAmount), ref _taxAmount, value);
    }
    [ModelDefault("AllowEdit", "false")]
    [Size(20)]
    public string? OnlineTaxType {
      get => _onlineTaxType;
      set => SetPropertyValue(nameof(OnlineTaxType), ref _onlineTaxType, value);
    }
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [ModelDefault("AllowEdit", "false")]
    // price
    public double UnitPriceInVat {
      get => _unitPriceInVat;
      set => SetPropertyValue(nameof(UnitPriceInVat), ref _unitPriceInVat, value);
    }
    [ModelDefault("DisplayFormat", "{0:n2}")]
    public double LineValueInVat => Quantity * UnitPriceInVat;
    [Size(-1)]
    [ModelDefault("AllowEdit", "false")]
    public string? LineNotes {
      get => _lineNotes;
      set => SetPropertyValue(nameof(LineNotes), ref _lineNotes, value);
    }
    public OnlineSalesOrderLine(Session session) : base(session) { }
  }
}
namespace BosmanCommerce7.Module.BusinessObjects {
  [DefaultClassOptions]
  [NavigationItem(true)]
  public class OnlineSalesOrderProcessingLog : XPObject {
    private OnlineSalesOrder? _onlineSalesOrder;
    private DateTime _entryDate;
    private string? _shortDescription;
    private string? _details;
    [Association("OnlineSalesOrder-OnlineSalesOrderProcessingLog")]
    public OnlineSalesOrder? OnlineSalesOrder {
      get => _onlineSalesOrder;
      set => SetPropertyValue(nameof(OnlineSalesOrder), ref _onlineSalesOrder, value);
    }
    [ModelDefault("DisplayFormat", "{0:yyyy/MM/dd HH:mm:ss}")]
    [ModelDefault("EditMask", "yyyy/MM/dd HH:mm:ss")]
    [ModelDefault("AllowEdit", "false")]
    public DateTime EntryDate {
      get => _entryDate;
      set => SetPropertyValue(nameof(EntryDate), ref _entryDate, value);
    }
    [ModelDefault("AllowEdit", "false")]
    public string? ShortDescription {
      get => _shortDescription;
      set => SetPropertyValue(nameof(ShortDescription), ref _shortDescription, value);
    }
    [Size(-1)]
    [ModelDefault("AllowEdit", "false")]
    public string? Details {
      get => _details;
      set => SetPropertyValue(nameof(Details), ref _details, value);
    }
    public OnlineSalesOrderProcessingLog(Session session) : base(session) { }
    public override void AfterConstruction() {
      base.AfterConstruction();
      EntryDate = DateTime.Now;
    }
  }
}
namespace BosmanCommerce7.Module.BusinessObjects {
  [DefaultClassOptions]
  [NavigationItem("System")]
  public class TaxMapping : XPObject {
    private string? _onlineTaxType;
    private string? _evolutionTaxType;
    [RuleRequiredField]
    [RuleUniqueValue]
    public string? OnlineTaxType {
      get => _onlineTaxType;
      set => SetPropertyValue(nameof(OnlineTaxType), ref _onlineTaxType, value);
    }
    [RuleRequiredField]
    public string? EvolutionTaxType {
      get => _evolutionTaxType;
      set => SetPropertyValue(nameof(EvolutionTaxType), ref _evolutionTaxType, value);
    }
    public TaxMapping(Session session) : base(session) { }
  }
}
namespace BosmanCommerce7.Module.BusinessObjects {
  [DefaultClassOptions]
  [NavigationItem("System")]
  public class ValueStore : XPObject {
    private string? _keyName;
    private string? _keyValue;
    [ModelDefault("AllowEdit", "false")]
    public string? KeyName {
      get => _keyName;
      set => SetPropertyValue(nameof(KeyName), ref _keyName, value);
    }
    [Size(-1)]
    public string? KeyValue {
      get => _keyValue;
      set => SetPropertyValue(nameof(KeyValue), ref _keyValue, value);
    }
    public ValueStore(Session session) : base(session) { }
  }
}
namespace BosmanCommerce7.Module.BusinessObjects {
  [DefaultClassOptions]
  [NavigationItem("System")]
  public class WarehousePostalCodeMapping : XPObject {
    private string? _postalCode;
    private string? _warehouseCode;
    [Size(4)]
    [Indexed]
    [RuleUniqueValue]
    [RuleRequiredField]
    public string? PostalCode {
      get => _postalCode;
      set => SetPropertyValue(nameof(PostalCode), ref _postalCode, value);
    }
    [Size(10)]
    [RuleRequiredField]
    public string? WarehouseCode {
      get => _warehouseCode;
      set => SetPropertyValue(nameof(WarehouseCode), ref _warehouseCode, value);
    }
    public WarehousePostalCodeMapping(Session session) : base(session) { }
  }
}
namespace BosmanCommerce7.Module.Controllers {
  public abstract class ActionControllerBase : ViewController {
    protected IObjectSpace? PopupObjectSpace { get; private set; }
    protected virtual string? ActionId => null;
    protected int CurrentObjectOid {
      get {
        switch (View.CurrentObject) {
          case XPObject o:
            return o.Oid;
          default:
            throw new ArgumentOutOfRangeException();
        }
      }
    }
    protected IContainer Components { get; }
    protected ViewType PopupViewType;
    protected ActionControllerBase() {
      Components = new Container();
      PopupViewType = ViewType.DetailView;
    }
    protected override void OnAfterConstruction() {
      base.OnAfterConstruction();
      CreateActions();
    }
    protected abstract void CreateActions();
    protected override void Dispose(bool disposing) {
      if (disposing) {
        PopupObjectSpace?.Dispose();
        Components?.Dispose();
      }
      base.Dispose(disposing);
    }
    protected SimpleAction NewToolsAction(
    string caption,
    SimpleActionExecuteEventHandler handler,
    string targetObjectsCriteria = "",
    SelectionDependencyType selectionDependencyType = SelectionDependencyType.RequireSingleObject,
    bool confirmationPrompt = true) {
      var action = NewAction(
        caption,
        handler,
        targetObjectsCriteria,
        selectionDependencyType,
        confirmationPrompt);
      action.Category = "Tools";
      return action;
    }
    protected SimpleAction NewAction(
      string caption,
      SimpleActionExecuteEventHandler handler,
      string targetObjectsCriteria = "",
      SelectionDependencyType selectionDependencyType = SelectionDependencyType.RequireSingleObject,
      bool confirmationPrompt = true,
      string tooltip = "",
      string? actionId = null) {
      var action = new SimpleAction(Components) {
        Caption = caption,
        ConfirmationMessage = confirmationPrompt ? "Are you sure?" : string.Empty,
        Id = actionId ?? ActionId ?? $"[{caption}]{GetType()}",
        SelectionDependencyType = selectionDependencyType,
        TargetObjectsCriteriaMode = TargetObjectsCriteriaMode.TrueForAll,
        TargetObjectsCriteria = string.IsNullOrWhiteSpace(targetObjectsCriteria) ? null : targetObjectsCriteria,
        ToolTip = tooltip
      };
      action.Execute += handler;
      OnAfterActionCreated(action);
      Actions.Add(action);
      return action;
    }
    protected PopupWindowShowAction NewPopupWindowShowAction(
      string acceptButtonCaption,
      string caption,
      string actionId,
      string targetObjectsCriteria = "",
      SelectionDependencyType selectionDependencyType = SelectionDependencyType.RequireSingleObject) {
      var action = new PopupWindowShowAction(Components) {
        AcceptButtonCaption = acceptButtonCaption,
        CancelButtonCaption = null,
        Caption = caption,
        ConfirmationMessage = null,
        Id = actionId,
        SelectionDependencyType = selectionDependencyType,
        ToolTip = null,
        TargetObjectsCriteriaMode = TargetObjectsCriteriaMode.TrueForAll,
        TargetObjectsCriteria = targetObjectsCriteria
      };
      action.CustomizePopupWindowParams += Pop_CustomizePopupWindowParams;
      action.Execute += Popup_Execute;
      OnAfterActionCreated(action);
      Actions.Add(action);
      return action;
    }
    protected virtual void OnAfterActionCreated(ActionBase viewAction) {
    }
    private void Pop_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e) {
      View.CommitChangesIfEditState();
      AssignPopupView(sender, e, CreatePopupView(e));
      OnCustomizePopupWindowParams(sender, e);
    }
    protected virtual View CreatePopupView(CustomizePopupWindowParamsEventArgs e) {
      PopupObjectSpace = e.Application.CreateObjectSpace(GetPopupParameterObjectType(e.Action));
      OnObjectSpaceCreated(e.Application, PopupObjectSpace);
      if (PopupObjectSpace is NonPersistentObjectSpace compositeObjectSpace) {
        compositeObjectSpace.AutoDisposeAdditionalObjectSpaces = true;
        compositeObjectSpace.AutoRefreshAdditionalObjectSpaces = true;
      }
      if (PopupViewType == ViewType.ListView) {
        var listView = CreatePopupListView(e.Application, e.Action, PopupObjectSpace);
        return listView;
      }
      var detailView = CreatePopupDetailView(e.Application, e.Action, PopupObjectSpace);
      detailView.ViewEditMode = ViewEditMode.Edit;
      return detailView;
    }
    protected virtual void OnObjectSpaceCreated(XafApplication application, IObjectSpace PopupObjectSpace) {
    }
    protected virtual void AssignPopupView(object sender, CustomizePopupWindowParamsEventArgs e, View view) {
      e.View = view;
    }
    protected virtual void OnCustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e) {
    }
    protected DetailView CreatePopupDetailView(XafApplication application, ActionBase action, IObjectSpace PopupObjectSpace) {
      return application.CreateDetailView(PopupObjectSpace, GetPopupDetailViewModel(action), true);
    }
    protected ListView CreatePopupListView(XafApplication application, ActionBase action, IObjectSpace PopupObjectSpace) {
      var viewId = GetListViewId();
      var collectionSource = Application.CreateCollectionSource(PopupObjectSpace, GetListViewCollectionSourceType(), viewId);
      OnAfterCollectionSourceCreated(collectionSource);
      var criteria = GetListViewCollectionSourceCriteria();
      if (!string.IsNullOrWhiteSpace(criteria)) { collectionSource.SetCriteria(viewId, criteria); }
      var view = application.CreateListView(viewId, collectionSource, true);
      return view;
    }
    protected virtual void OnAfterCollectionSourceCreated(CollectionSourceBase collectionSource) {
    }
    protected virtual string? GetListViewCollectionSourceCriteria() {
      return null;
    }
    protected virtual string GetListViewId() {
      throw new NotImplementedException();
    }
    protected virtual Type GetListViewCollectionSourceType() {
      throw new NotImplementedException();
    }
    protected abstract object GetPopupDetailViewModel(ActionBase action);
    protected abstract Type GetPopupParameterObjectType(ActionBase action);
    private void Popup_Execute(object sender, PopupWindowShowActionExecuteEventArgs e) => ExecutePopup(sender, e);
    protected abstract void ExecutePopup(object sender, PopupWindowShowActionExecuteEventArgs args);
    protected void ShowErrorMessage(string error) => Application.ShowErrorMessage(error);
    protected void ShowSuccessMessage(string message = "Success") => Application.ShowSuccessMessage(message);
    protected object GetParentObject() => ((NestedFrame)Frame).ViewItem.View.CurrentObject;
    protected View GetParentView() => ((NestedFrame)Frame).ViewItem.View;
    protected T? ResolveService<T>() => (T?)Application.ServiceProvider.GetService(typeof(T));
  }
}
namespace BosmanCommerce7.Module.Controllers {
  public class FetchNowActionController : ActionControllerBase {
    private readonly IServiceProvider? _serviceProvider;
    public FetchNowActionController() {
      TargetObjectType = typeof(OnlineSalesOrder);
      TargetViewType = ViewType.ListView;
      TargetViewNesting = Nesting.Root;
    }
    [ActivatorUtilitiesConstructor]
    public FetchNowActionController(IServiceProvider serviceProvider) : this() {
      _serviceProvider = serviceProvider;
    }
    protected override void CreateActions() {
      var action = NewAction("Fetch now", (s, e) => { Execute(); }, selectionDependencyType: SelectionDependencyType.Independent);
    }
    private void Execute() {
      var service = _serviceProvider!.GetService<ISalesOrdersSyncService>();
      var context = new SalesOrdersSyncContext();
      service!
        .Execute(context)
        .Tap(() => { ShowSuccessMessage(); })
        .TapError(ShowErrorMessage);
      View.RefreshView();
    }
    protected override void ExecutePopup(object sender, PopupWindowShowActionExecuteEventArgs args) {
      throw new NotImplementedException();
    }
    protected override object GetPopupDetailViewModel(ActionBase action) {
      throw new NotImplementedException();
    }
    protected override Type GetPopupParameterObjectType(ActionBase action) {
      throw new NotImplementedException();
    }
  }
}
namespace BosmanCommerce7.Module.Controllers {
  public class PostNowActionController : ActionControllerBase {
    private readonly IServiceProvider? _serviceProvider;
    public PostNowActionController() {
      TargetObjectType = typeof(OnlineSalesOrder);
      TargetViewType = ViewType.ListView;
      TargetViewNesting = Nesting.Root;
    }
    [ActivatorUtilitiesConstructor]
    public PostNowActionController(IServiceProvider serviceProvider) : this() {
      _serviceProvider = serviceProvider;
    }
    protected override void CreateActions() {
      var criteria = "PostingStatus".InCriteriaOperator(SalesOrderPostingStatus.New, SalesOrderPostingStatus.Retrying, SalesOrderPostingStatus.Failed).ToCriteriaString();
      var action = NewAction("Post now", (s, e) => { Execute(); },
        selectionDependencyType: SelectionDependencyType.Independent,
        targetObjectsCriteria: criteria);
    }
    private void Execute() {
      var userHasSelectedSalesOrders = View.SelectedObjects.Count > 0;
      var selectedSalesOrders = userHasSelectedSalesOrders ? View.SelectedObjects.Cast<OnlineSalesOrder>().ToList() : new List<OnlineSalesOrder>();
      var criteria = userHasSelectedSalesOrders
        ? "Oid".MapArrayToInOperator(selectedSalesOrders.Select(a => a.Oid).ToArray())
        : null;
      var service = _serviceProvider!.GetService<ISalesOrdersPostService>();
      var context = new SalesOrdersPostContext(criteria);
      service!
        .Execute(context)
        .Tap(() => { ShowSuccessMessage(); })
        .TapError(ShowErrorMessage);
      View.RefreshView();
    }
    protected override void ExecutePopup(object sender, PopupWindowShowActionExecuteEventArgs args) {
      throw new NotImplementedException();
    }
    protected override object GetPopupDetailViewModel(ActionBase action) {
      throw new NotImplementedException();
    }
    protected override Type GetPopupParameterObjectType(ActionBase action) {
      throw new NotImplementedException();
    }
  }
}
namespace BosmanCommerce7.Module.Extensions {
  public static class CriteriaOperatorFunctions {
    public static CriteriaOperator InCriteriaOperator(this string propertyName, params object[] values) => new InOperator(propertyName, values);
    public static CriteriaOperator IsEqualOidOperator(this int oid) => "Oid".IsEqualToOperator(oid);
    public static CriteriaOperator IsEqualOidOperator(this Guid oid) => "Oid".IsEqualToOperator(oid);
    public static CriteriaOperator IsEqualToOperator(this string propertyName, object value) => new BinaryOperator(propertyName, value);
    public static CriteriaOperator IsGreaterThanOperator(this string propertyName, object value) => new BinaryOperator(propertyName, value, BinaryOperatorType.Greater);
    public static CriteriaOperator IsGreaterOrEqualOperator(this string propertyName, object value) => new BinaryOperator(propertyName, value, BinaryOperatorType.GreaterOrEqual);
    public static CriteriaOperator IsNotEqualToOperator(this string propertyName, object value) => new BinaryOperator(propertyName, value).Not();
    public static CriteriaOperator IsNotNullOperator(this string propertyName) => new NullOperator(propertyName).Not();
    public static CriteriaOperator IsNullOperator(this string propertyName) => new NullOperator(propertyName);
    public static CriteriaOperator? MapArrayToInOperator(this string propertyName, object[] values) {
      if (values == null || values.Length == 0) {
        return null;
      }
      return new InOperator(propertyName, values);
    }
    public static CriteriaOperator? MapArrayToInOperator<T>(this string propertyName, T[] values) {
      if (values == null || values.Length == 0) {
        return null;
      }
      return new InOperator(propertyName, values);
    }
    public static CriteriaOperator? MapArrayToInOperator(this string propertyName, int[] values) {
      if (values == null || values.Length == 0) {
        return null;
      }
      return new InOperator(propertyName, values);
    }
    public static ConstantValue MapToConstantValue(this string value) => new(value);
    public static CriteriaOperator? MapToInOperator(this string propertyName, params object[] values) {
      if (values == null || values.Length == 0) {
        return null;
      }
      return new InOperator(propertyName, (IEnumerable)values.Select(o => new OperandValue(o)));
    }
    public static OperandProperty MapToPropertyOperand(this string propertyName) => new(propertyName);
    public static CriteriaOperator NotInCriteriaOperator(this string propertyName, params object[] values) => new InOperator(propertyName, values).Not();
    public static CriteriaOperator PropertyDateTypeHasValue(this string propertyName) => new BinaryOperator(propertyName, new DateTime(2000, 1, 1), BinaryOperatorType.Greater);
    public static CriteriaOperator PropertyFalse(this string propertyName) => propertyName.IsEqualToOperator(false);
    public static CriteriaOperator PropertyLessThan(this string propertyName, object value) => new BinaryOperator(propertyName, value, BinaryOperatorType.Less);
    public static CriteriaOperator PropertyNotEqualTo(this string propertyName, object value) => new BinaryOperator(propertyName, value).Not();
    public static CriteriaOperator PropertyNotStartWith(this string propertyName, string startValue) => propertyName.PropertyStartsWith(startValue).Not();
    public static CriteriaOperator PropertyStartsWith(this string propertyName, string startValue) =>
      propertyName
        .MapToPropertyOperand()
        .StartsWith(startValue.MapToConstantValue());
    public static CriteriaOperator PropertyTrue(this string propertyName) => propertyName.IsEqualToOperator(true);
    public static CriteriaOperator StartsWith(this CriteriaOperator thisCriteriaOperator, CriteriaOperator startsWithCriteriaOperator) =>
      new FunctionOperator(FunctionOperatorType.StartsWith,
        thisCriteriaOperator,
        startsWithCriteriaOperator);
    public static CriteriaOperator StringCriteria(this string propertyName, string value) => CriteriaOperator.Parse($"lower({propertyName}) = lower('{value}')");
    public static string EnumOperand<T>(T value) {
      return $"##Enum#{typeof(T).FullName},{value}#";
    }
    public static string ToCriteriaString(this CriteriaOperator criteriaOperator) => criteriaOperator.LegacyToString();
  }
}
namespace BosmanCommerce7.Module.Extensions {
  public static class ExceptionFunctions {
    public static string GetMessages(Exception? ex) {
      var sb = new StringBuilder();
      var e = ex;
      while (e != null) {
        sb.AppendLine(e.Message);
        e = e.InnerException;
      }
      return sb.ToString();
    }
  }
}
namespace BosmanCommerce7.Module.Extensions {
  public static class FindObjectFunctions {
    public static T FindByOid<T>(this Session session, Guid oid) => session.FromCriteria<T>(oid.IsEqualOidOperator());
    public static T FindByOid<T>(this Session session, int oid) => session.FromCriteria<T>(oid.IsEqualOidOperator());
    public static T FindByOid<T>(this IObjectSpace objectSpace, Guid oid) => objectSpace.FromCriteria<T>(oid.IsEqualOidOperator());
    public static T FindByOid<T>(this IObjectSpace objectSpace, int oid) => objectSpace.FromCriteria<T>(oid.IsEqualOidOperator());
    public static object FindByOid(this IObjectSpace objectSpace, Type type, int oid) => objectSpace.FromCriteria(type, oid.IsEqualOidOperator());
    public static object FindByOid(this Session session, Type type, int oid) => session.FromCriteria(type, oid.IsEqualOidOperator());
    public static T FindByStringProperty<T>(this Session session, string propertyName, string value) => session.FromCriteria<T>(propertyName.StringCriteria(value));
    public static T FindByStringProperty<T>(this IObjectSpace objectSpace, string propertyName, string value) => objectSpace.FromCriteria<T>(propertyName.StringCriteria(value));
    public static T FromCriteria<T>(this Session session, CriteriaOperator criteria) => session.FindObject<T>(criteria);
    public static T FromCriteria<T>(this IObjectSpace objectSpace, CriteriaOperator criteria) => objectSpace.FindObject<T>(criteria);
    public static object FromCriteria(this IObjectSpace objectSpace, Type type, CriteriaOperator criteria) => objectSpace.FindObject(type, criteria);
    public static object FromCriteria(this Session session, Type type, CriteriaOperator criteria) => session.FindObject(type, criteria);
  }
}
namespace BosmanCommerce7.Module.Extensions {
  public static class JsonFunctions {
    public static string? Serialize(object? data) {
      if (data is string) { return $"{data ?? ""}"; }
      var settings = new JsonSerializerSettings {
        NullValueHandling = NullValueHandling.Ignore
      };
      return data != null ? JsonConvert.SerializeObject(data, settings) : null;
    }
    public static T? Deserialize<T>(string json) {
      return JsonConvert.DeserializeObject<T>(json);
    }
  }
}
namespace BosmanCommerce7.Module.Extensions {
  public static class NotificationFunctions {
    public static void ShowInfoMessage(this XafApplication application, string message) {
      ShowMessage(application, message, InformationType.Info);
    }
    public static void ShowErrorMessage(this XafApplication application, string message) {
      ShowMessage(application, message, InformationType.Error, 60000);
    }
    public static void ShowErrorMessage(this XafApplication application, Exception ex) {
      var sb = new StringBuilder();
      var e = ex;
      while (e != null) {
        sb.Append(e.Message);
        e = e.InnerException;
      }
      var message = sb.ToString();
      ShowMessage(application, message, InformationType.Error, 60000);
    }
    public static void ShowSuccessMessage(this XafApplication application) {
      application.ShowSuccessMessage("Success");
    }
    public static void ShowSuccessMessage(this XafApplication application, string message) {
      ShowMessage(application, message, InformationType.Success);
    }
    public static void ShowWarnMessage(this XafApplication application, string message) {
      ShowMessage(application, message, InformationType.Warning, 15000);
    }
    private static void ShowMessage(XafApplication application, string message, InformationType informationType, int displayInterval = 3000) {
      application.ShowViewStrategy.ShowMessage(message, informationType, displayInterval, InformationPosition.Bottom);
    }
  }
}
namespace BosmanCommerce7.Module.Extensions {
  public static class ServiceProviderFunctions {
    public static ApplicationOptions GetApplicationOptions(IServiceProvider serviceProvider) {
      var service = serviceProvider.GetService<ApplicationOptions>() ?? throw new Exception($"{nameof(ApplicationOptions)} not defined in appsettings.json.");
      return service;
    }
  }
}
namespace BosmanCommerce7.Module.Extensions {
  public static class ServiceScopeFunctions {
    private static IServiceProvider? _serviceProvider;
    public static void SetServiceProvider(IServiceProvider serviceProvider) {
      _serviceProvider = serviceProvider;
    }
    public static void WrapInScope(Action<IServiceScope> action) {
      action(serviceScope);
    }
  }
}
namespace BosmanCommerce7.Module.Extensions {
  public static class ViewFunctions {
    public static View CommitChangesIfEditStateThen(this View view, Action<Session> action) {
      view.CommitChangesIfEditState();
      action(view.GetSession());
      return view;
    }
    public static View? CommitChangesIfEditState(this View view, Action? beforeCommit = null) {
      if (view == null || view.CurrentObject == null || view.IsListView() || view.IsDetailViewInViewOnlyMode()) { return view; }
      if (view.ObjectSpace.IsModified) {
        beforeCommit?.Invoke();
        view.Commit();
      }
      return view;
    }
    public static void CommitAndCloseDetailView(this View view) {
      if (view == null || view.CurrentObject == null) { return; }
      if (view.IsListView()) { return; }
      if (!view.IsDetailViewInViewOnlyMode()) { view.Commit(); }
      view.CloseView();
      return;
    }
    public static Session GetSession(this View view) => ((XPObjectSpace)view.ObjectSpace).Session;
    public static View CommitAndRefresh(this View view) => view.Commit().RefreshView();
    public static void CloseIfDetailViewOrRefresh(this View view) {
      if (view.IsListView()) {
        view.RefreshView();
        return;
      }
      view.CloseView();
    }
    public static bool IsListView(this View view) => view is ListView;
    public static bool IsDetailViewInViewOnlyMode(this View view) => view is DetailView a && a.ViewEditMode == ViewEditMode.View;
    public static void CloseView(this View view) => view.Close();
    public static View Commit(this View view) {
      view.ObjectSpace.CommitChanges();
      return view;
    }
    public static DetailView? CreateEditDetailView(XafApplication application, Type type, string editViewId, int requestOid) {
      if (requestOid <= 0) { return null; }
      var objectSpace = application.CreateObjectSpace(type);
      var request = objectSpace.FindByOid(type, requestOid);
      if (request == null) { return null; }
      var detailView = application.CreateDetailView(objectSpace, editViewId, true, request);
      detailView.ViewEditMode = ViewEditMode.Edit;
      return detailView;
    }
    public static View Rollback(this View view) {
      view.ObjectSpace.Rollback();
      return view;
    }
    public static View RefreshView(this View view) {
      view.ObjectSpace.Refresh();
      return view;
    }
    public static void SetCaption(this View view, string propertyName, string caption) {
      if (string.IsNullOrWhiteSpace(caption)) { return; }
      if (view is DetailView v) {
        var editor = v.FindItem(propertyName);
        if (editor == null) { return; }
        editor.Caption = caption;
      }
      else if (view is ListView l) {
        var editor = (ColumnsListEditor)l.Editor;
        if (editor == null) { return; }
        var column = editor.FindColumn(propertyName);
        if (column == null) { return; }
        column.Caption = caption;
      }
    }
    public static IList<T?> GetDetailViewChildListViewSelectedObjects<T>(this View view, string listViewPropertyName) {
      if (view is DetailView detailView) {
        if (detailView.FindItem(listViewPropertyName) is ListPropertyEditor lines) {
          return lines.ListView.SelectedObjects.Cast<T?>().ToList() ?? new List<T?>();
        }
      }
      return new List<T?>();
    }
  }
}
namespace BosmanCommerce7.Module.Extensions.DataAccess {
  public static class DatabaseFunctions {
    public static T WrapInOpenConnection<T>(this string? connectionString, Func<SqlConnection, T> func) {
      if (connectionString is null) {
        throw new ArgumentNullException(nameof(connectionString));
      }
      var waitAndRetry = Policy
        .Handle<SqlException>()
        .WaitAndRetry(5, i => TimeSpan.FromSeconds(i * 5));
      return
        waitAndRetry
          .Execute(
            () => {
              try {
                connection.Open();
                return func(connection);
              }
              finally {
                connection.Close();
              }
            });
    }
    public static void WrapInTransaction(this string? connectionString, Action<SqlConnection, SqlTransaction> action) {
      if (connectionString is null) {
        throw new ArgumentNullException(nameof(connectionString));
      }
      var waitAndRetry =
        Policy
          .Handle<SqlException>()
          .WaitAndRetry(5, i => TimeSpan.FromSeconds(i * 5));
      waitAndRetry
        .Execute(
          () => {
            DbTransaction? transaction = null;
            try {
              connection.Open();
              transaction = connection.BeginTransaction();
              action(connection, (SqlTransaction)transaction);
              transaction.Commit();
            }
            catch {
              transaction?.Rollback();
              throw;
            }
            finally {
              connection.Close();
            }
          });
    }
  }
}
namespace BosmanCommerce7.Module.Extensions.EvolutionSdk {
  public static class AddressFunctions {
    public static int FindFirstEmptyLine(this Address address) =>
      address.AddressLines()
             .Where(addressLine => addressLine.addressLine.IsNullOrWhiteSpace())
             .Select(addressLine => addressLine.lineNumber)
             .FirstOrDefault();
    public static string GetLineNumberValue(this Address address, int lineNumber) =>
      address.AddressLines()
             .Where(addressLine => addressLine.lineNumber == lineNumber)
             .Select(addressLine => addressLine.addressLine)
             .FirstOrDefault() ?? "";
    private static IEnumerable<(int lineNumber, string addressLine)> AddressLines(this Address address) {
      yield return (1, address.Line1);
      yield return (2, address.Line2);
      yield return (3, address.Line3);
      yield return (4, address.Line4);
      yield return (5, address.Line5);
      //Do not process postal code. yield return (6, address.PostalCode);
    }
    private static Address SetAddressLineValue(this Address address, int lineNumber, string newValue) {
      if (lineNumber == 1) {
        address.Line1 = newValue;
      }
      else if (lineNumber == 2) {
        address.Line2 = newValue;
      }
      else if (lineNumber == 3) {
        address.Line3 = newValue;
      }
      else if (lineNumber == 4) {
        address.Line4 = newValue;
      }
      else if (lineNumber == 5) {
        address.Line5 = newValue;
      }
      return address;
    }
    private static Address CloneAddress(this Address address) =>
      new() {
        Line1 = address.Line1,
        Line2 = address.Line2,
        Line3 = address.Line3,
        Line4 = address.Line4,
        Line5 = address.Line5,
        PostalCode = address.PostalCode
      };
    public static bool HasEmptySpace(this Address address) => address.FindFirstEmptyLine() > 0;
    public static Address WriteToFirstEmptySpace(this Address address, string value) {
      var firstEmptyLine = address.FindFirstEmptyLine();
      return firstEmptyLine == 0 ? address : address.SetAddressLineValue(firstEmptyLine, value);
    }
    public static Address MoveEmptyLineToBottom(this Address address) {
      var search = true;
      while (search) {
        search = false;
        var hasEmptyLine = false;
        foreach (var addressLine in address.AddressLines()) {
          if (!hasEmptyLine && addressLine.addressLine.IsNullOrWhiteSpace()) {
            hasEmptyLine = true;
          }
          if (!hasEmptyLine || addressLine.addressLine.IsNullOrWhiteSpace()) {
            continue;
          }
          address = address.SwapLines(addressLine.lineNumber, addressLine.lineNumber - 1);
          search = true;
        }
      }
      return address;
    }
    public static Address TryAddValueToTop(this Address address, string value) {
      if (!address.HasEmptySpace()) { return address; }
      return address
        .MakeSpaceAtTop()
        .SetAddressLineValue(1, value);
    }
    public static Address MakeSpaceAtTop(this Address address) => address.Line1.IsNullOrWhiteSpace() ? address : address.MoveLinesDown(1);
    public static Address SwapLines(this Address address, int line1, int line2) {
      var hold = address.GetLineNumberValue(line1);
      return address.SetAddressLineValue(line1, address.GetLineNumberValue(line2)).SetAddressLineValue(line2, hold);
    }
    public static Address MoveLinesDown(this Address address, int fromLineNumber) {
      var firstEmptyLine = address.FindFirstEmptyLine();
      if (firstEmptyLine == 0) { return address; }
      var newAddress = address.CloneAddress();
      for (var i = firstEmptyLine; i > fromLineNumber; i--) {
        newAddress = newAddress.SetAddressLineValue(i, address.GetLineNumberValue(i - 1));
      }
      newAddress = newAddress.SetAddressLineValue(fromLineNumber, "");
      newAddress.PostalCode = address.PostalCode;
      return newAddress;
    }
  }
}
namespace BosmanCommerce7.Module.Extensions.EvolutionSdk {
  public record PostToEvolutionSalesOrderContext {
    public IObjectSpace ObjectSpace { get; init; } = default!;
  }
}
namespace BosmanCommerce7.Module.Extensions.QuartzTools {
  public static class CronExpressionFunctions {
    public static string SecondsInterval(int seconds) => $"*/{seconds} * * ? * * *";
    public static string MinutesInterval(int minutes) => $"0 */{minutes} * ? * * *";
  }
}
namespace BosmanCommerce7.Module.Extensions.QuartzTools {
  public interface ISyncQueueService {
    Task Execute();
  }
}
namespace BosmanCommerce7.Module.Extensions.QuartzTools {
  public static class JobIds {
    public const string JobServiceInstance = nameof(JobServiceInstance);
    public const string JobsGroup = nameof(JobsGroup);
    public const string SalesOrdersSyncJob = nameof(SalesOrdersSyncJob);
    public const string SalesOrdersPostJob = nameof(SalesOrdersPostJob);
  }
}
namespace BosmanCommerce7.Module.Extensions.QuartzTools {
  [DisallowConcurrentExecution]
  public class ProcessQueueTableJob : IJob {
    public async Task Execute(IJobExecutionContext context) {
      var name = context.JobDetail.Key.Name;
      try {
        if (context.JobDetail.JobDataMap.Get(JobIds.JobServiceInstance) is not ISyncQueueService syncQueueService) {
          Log.Logger.Warning($"Service {nameof(ISyncQueueService)} not found in this Job's context.");
          await Task.CompletedTask;
        }
        else {
          await syncQueueService.Execute();
        }
      }
      catch (Exception ex) {
        throw new JobExecutionException(ex);
      }
    }
  }
}
namespace BosmanCommerce7.Module.Extensions.QuartzTools {
  public static class QuartzApplicationFunctions {
    private static IScheduler? _scheduler;
    public static void StartJobs(QuartzStartJobContext context) {
      _scheduler = QuartzFunctions.CreateScheduler();
      ScheduleSalesOrdersSyncJobQueueService(context, _scheduler);
      ScheduleSalesOrdersPostJobQueueService(context, _scheduler);
      _scheduler.Start();
    }
    private static void ScheduleSalesOrdersSyncJobQueueService(QuartzStartJobContext context, IScheduler scheduler) {
      var jobOptions = context.Options.SalesOrdersSyncJobOptions as JobOptionsBase ?? throw new Exception($"{nameof(context.Options.SalesOrdersSyncJobOptions)} not defined in appsettings.json.");
      ScheduleSyncQueueService<ISalesOrdersSyncQueueService>(context, new QuartzStartJobDescriptor {
        JobId = JobIds.SalesOrdersSyncJob,
        JobOptions = jobOptions,
        Scheduler = scheduler
      });
    }
    private static void ScheduleSalesOrdersPostJobQueueService(QuartzStartJobContext context, IScheduler scheduler) {
      var jobOptions = context.Options.SalesOrdersPostJobOptions as JobOptionsBase ?? throw new Exception($"{nameof(context.Options.SalesOrdersPostJobOptions)} not defined in appsettings.json.");
      ScheduleSyncQueueService<ISalesOrdersPostQueueService>(context, new QuartzStartJobDescriptor {
        JobId = JobIds.SalesOrdersPostJob,
        JobOptions = jobOptions,
        Scheduler = scheduler
      });
    }
    private static void ScheduleSyncQueueService<T>(QuartzStartJobContext context, QuartzStartJobDescriptor jobDescriptor) {
      if (!jobDescriptor.JobOptions.Enabled) {
        context.Logger.LogWarning("{service} is disabled", jobDescriptor.JobId);
        return;
      }
      var service = context.ServiceProvider.GetService<T>();
      var job = JobBuilder.Create<ProcessQueueTableJob>()
        .WithIdentity(jobDescriptor.JobId, JobIds.JobsGroup)
        .SetJobData(new JobDataMap() { { JobIds.JobServiceInstance, service! } })
        .Build();
      var trigger = TriggerBuilder.Create()
        .WithIdentity(jobDescriptor.JobId, JobIds.JobsGroup)
        .StartAt(DateTime.Now.AddSeconds(jobDescriptor.JobOptions.StartDelaySeconds))
        .WithCronSchedule(CronExpressionFunctions.SecondsInterval(jobDescriptor.JobOptions.RepeatIntervalSeconds))
        .Build();
      jobDescriptor.Scheduler.ScheduleJob(job, trigger);
    }
    public static void StopJobs() {
      _scheduler?.Shutdown();
    }
  }
  public record QuartzStartJobDescriptor {
    public string JobId { get; init; } = default!;
    public IScheduler Scheduler { get; init; } = default!;
    public JobOptionsBase JobOptions { get; init; } = default!;
  }
  public record QuartzStartJobContext {
    public IServiceProvider ServiceProvider { get; init; } = default!;
    public ILogger Logger { get; init; } = default!;
    public ApplicationOptions Options { get; init; } = default!;
  }
}
namespace BosmanCommerce7.Module.Extensions.QuartzTools {
  public static class QuartzFunctions {
    private static IScheduler? _scheduler;
    internal static IScheduler GetIScheduler() => (IScheduler)GetScheduler();
    public static object GetScheduler() => _scheduler ??= CreateScheduler();
    public static void StartScheduler() {
      var scheduler = GetIScheduler();
      if (scheduler.IsStarted) { return; }
      scheduler.Start();
    }
    public static void ShutdownScheduler() {
      var scheduler = GetIScheduler();
      if (scheduler.IsShutdown) { return; }
      scheduler.Shutdown();
    }
    public static void StopTrigger(ITrigger trigger) {
      var scheduler = GetIScheduler();
      scheduler.UnscheduleJob(trigger.Key);
    }
    internal static IScheduler CreateScheduler() {
      var factory = new StdSchedulerFactory();
      return factory.GetScheduler().Result;
    }
  }
}
namespace BosmanCommerce7.Module.Models {
  public record ApplicationOptions {
    public string AppDataFolder { get; init; } = @"..\App_Data";
    public ApiOptions ApiOptions { get; init; } = default!;
    public ConnectionStrings ConnectionStrings { get; init; } = default!;
    public bool EnableDebugLoggingLevel { get; init; }
    public SalesOrdersSyncJobOptions SalesOrdersSyncJobOptions { get; init; } = default!;
    public SalesOrdersPostJobOptions SalesOrdersPostJobOptions { get; init; } = default!;
    public string InAppDataFolder(string path) {
      var p = Path.Combine(AppDataFolder, path);
      if (!Directory.Exists(p)) { Directory.CreateDirectory(p); }
      return p;
    }
  }
  public record ApiOptions {
    public string? Endpoint { get; set; }
    public string? TenantId { get; set; }
    public string? AppId { get; set; }
    public string? AppSecretKey { get; set; }
  }
  public abstract record JobOptionsBase {
    public bool Enabled { get; init; }
    public int RepeatIntervalSeconds { get; init; }
    public int StartDelaySeconds { get; init; }
  }
  public record SalesOrdersSyncJobOptions : JobOptionsBase {
    public string[]? ChannelsToProcess { get; init; }
  }
  public record SalesOrdersPostJobOptions : JobOptionsBase {
  }
  public record ConnectionStrings : ILocalDatabaseConnectionStringProvider, IEvolutionDatabaseConnectionStringProvider {
    public string? LocalDatabase { get; init; }
    public string? EvolutionCompany { get; init; }
    public string? EvolutionCommon { get; init; }
  }
}
namespace BosmanCommerce7.Module.Models {
  public interface IEvolutionDatabaseConnectionStringProvider {
    string? EvolutionCompany { get; }
    string? EvolutionCommon { get; }
  }
}
namespace BosmanCommerce7.Module.Models {
  public interface ILocalDatabaseConnectionStringProvider {
    string? LocalDatabase { get; }
  }
}
namespace BosmanCommerce7.Module.Models {
  public enum SalesOrderLineType {
    Inventory = 1,
    GeneralLedger = 2
  }
}
namespace BosmanCommerce7.Module.Models {
  public enum SalesOrderPostingStatus {
    New = 0,
    Retrying = 10,
    Posted = 100,
    Failed = 200
  }
}
namespace BosmanCommerce7.Module.Models.EvolutionDatabase {
  public record EvolutionInventoryDto {
    public int ItemId { get; init; }
    public string SimpleCode { get; init; } = default!;
    public string Description_1 { get; init; } = default!;
    public string? CategoryName { get; init; }
    public string? SegmentCode1 { get; init; }
    public string? SegmentCode2 { get; init; }
    public string? SegmentCode3 { get; init; }
    public string? SegmentCode4 { get; init; }
    public string? SegmentCode5 { get; init; }
    public string? SegmentCode6 { get; init; }
    public string? SegmentCode7 { get; init; }
    public double LastCost { get; init; }
  }
}
namespace BosmanCommerce7.Module.Models.EvolutionSdk {
  public record CustomerDescriptor {
    public string? EmailAddress { get; init; }
  }
}
namespace BosmanCommerce7.Module.Models.EvolutionSdk {
  public class DatabaseConnection : IDatabaseConnection {
    public SqlConnection Connection { get; }
    public SqlTransaction Transaction { get; }
    public DatabaseConnection(SqlConnection connection, SqlTransaction transaction) {
      Connection = connection;
      Transaction = transaction;
    }
  }
}
namespace BosmanCommerce7.Module.Models.EvolutionSdk {
  public class EvolutionCompanyDescriptor : IEvolutionCompanyDescriptor {
    public string BranchCode { get; }
    public string EvolutionCommonDatabaseConnectionString { get; }
    public string EvolutionCompanyDatabaseConnectionString { get; }
    public string? UserName { get; }
    public EvolutionCompanyDescriptor(string evolutionCompanyDatabaseConnectionString, string evolutionCommonDatabaseConnectionString, string? userName = null, string branchCode = "") {
      EvolutionCompanyDatabaseConnectionString = evolutionCompanyDatabaseConnectionString;
      EvolutionCommonDatabaseConnectionString = evolutionCommonDatabaseConnectionString;
      UserName = userName;
      BranchCode = branchCode;
    }
  }
}
namespace BosmanCommerce7.Module.Models.EvolutionSdk {
  public record EvolutionPriceListPrice(int UsedPriceListId, double UnitPriceInVat);
}
namespace BosmanCommerce7.Module.Models.EvolutionSdk {
  public interface IDatabaseConnection {
    SqlConnection Connection { get; }
    SqlTransaction Transaction { get; }
  }
}
namespace BosmanCommerce7.Module.Models.EvolutionSdk {
  public interface IEvolutionCompanyDescriptor {
    string BranchCode { get; }
    string EvolutionCommonDatabaseConnectionString { get; }
    string EvolutionCompanyDatabaseConnectionString { get; }
    string? UserName { get; }
  }
}
namespace BosmanCommerce7.Module.Models.LocalDatabase {
  public record FindWarehouseCodeDescriptor {
    public bool IsStoreOrder { get; init; }
    public string? PostalCode { get; init; }
    public IObjectSpace ObjectSpace { get; init; } = default!;
  }
}
namespace BosmanCommerce7.Module.Models.RestApi {
  public abstract record ApiRequestBase {
    public virtual object? Data { get; }
    protected string Resource { get; init; } = default!;
    public Method Method { get; init; } = Method.Post;
    public bool DeserializeBody { get; init; } = true;
    public bool IsPagedResponse { get; init; }
    public int CurrentPage { get; init; }
    public string GetResource() {
      if (IsPagedResponse) {
        return $"{Resource}&page={CurrentPage}";
      }
      return Resource;
    }
  }
}
namespace BosmanCommerce7.Module.Models.RestApi {
  public abstract record ApiResponseBase {
    public string? ResonseBody { get; init; }
    public byte[]? ResponseRawBytes { get; init; }
    public record Success : ApiResponseBase {
    }
    public record Failure : ApiResponseBase {
      public string? Uri { get; init; }
      public string? ErrorMessage { get; init; }
      public Exception? ErrorException { get; init; }
      public string FullErrorMessage => GetFullErrorMessage();
      public bool ProductNotFound => (FullErrorMessage?.Contains("not found", StringComparison.InvariantCultureIgnoreCase) ?? false) || (FullErrorMessage?.Contains("notfound", StringComparison.InvariantCultureIgnoreCase) ?? false);
      public bool UnprocessableEntity => (FullErrorMessage?.Contains("Unprocessable Entity", StringComparison.InvariantCultureIgnoreCase) ?? false) || (FullErrorMessage?.Contains("UnprocessableEntity", StringComparison.InvariantCultureIgnoreCase) ?? false);
      public string? StatusCode { get; init; }
      public string? StatusDescription { get; init; }
      private string GetFullErrorMessage() {
        var sb = new StringBuilder();
        void AppendLine(string caption, string value) {
          if (string.IsNullOrEmpty(value)) { return; }
          sb!.AppendLine($"{caption}: {value}");
        }
        sb!.AppendLine();
        sb!.AppendLine("Commerce7ApiResponse");
        AppendLine($"Status code:", StatusCode!);
        AppendLine($"Status description", StatusDescription!);
        AppendLine($"URI", Uri!);
        AppendLine($"Error message", ErrorMessage!);
        AppendLine($"Error message", ExceptionFunctions.GetMessages(ErrorException));
        AppendLine($"Response body", ResonseBody ?? "");
        return sb.ToString();
      }
    }
  }
}
namespace BosmanCommerce7.Module.Models.RestApi {
  public record SalesOrdersSyncApiRequest : ApiRequestBase {
    public SalesOrdersSyncApiRequest(DateTime orderSubmittedDate) {
      Resource = $"/order?orderSubmittedDate=gt:{orderSubmittedDate:yyyy-MM-dd}";
      Method = Method.Get;
      IsPagedResponse = true;
    }
  }
}
