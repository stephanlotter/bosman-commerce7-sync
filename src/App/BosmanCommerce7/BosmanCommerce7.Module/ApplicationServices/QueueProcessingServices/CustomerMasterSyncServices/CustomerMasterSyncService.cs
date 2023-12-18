/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-11-28
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.DataAccess.LocalDatabaseDataAccess;
using BosmanCommerce7.Module.ApplicationServices.EvolutionSdk.Customers;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.CustomerMasterSyncServices.Models;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.CustomerMasterSyncServices.RestApi;
using BosmanCommerce7.Module.BusinessObjects;
using BosmanCommerce7.Module.BusinessObjects.Customers;
using BosmanCommerce7.Module.Models;
using BosmanCommerce7.Module.Models.EvolutionSdk.Customers;
using CSharpFunctionalExtensions;
using CSharpFunctionalExtensions.ValueTasks;
using Microsoft.Extensions.Logging;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.CustomerMasterSyncServices {

  public class CustomerMasterSyncService : SyncMasterDataServiceBase, ICustomerMasterSyncService {
    private readonly CustomerMasterSyncJobOptions _customerMasterSyncJobOptions;
    private readonly ICustomerMasterLocalMappingService _customerMasterLocalMappingService;
    private readonly ICustomerMasterApiClient _apiClient;
    private readonly IEvolutionCustomerRepository _evolutionCustomerRepository;

    private List<EvolutionCustomerId> _processedIds = new();

    public CustomerMasterSyncService(ILogger<CustomerMasterSyncService> logger,
      CustomerMasterSyncJobOptions customerMasterSyncJobOptions,
      ICustomerMasterLocalMappingService customerMasterLocalMappingService,
      ICustomerMasterApiClient apiClient,
      IEvolutionCustomerRepository evolutionCustomerRepository,
      ILocalObjectSpaceEvolutionSdkProvider localObjectSpaceEvolutionSdkProvider
      ) : base(logger, localObjectSpaceEvolutionSdkProvider) {
      _customerMasterSyncJobOptions = customerMasterSyncJobOptions;
      _customerMasterLocalMappingService = customerMasterLocalMappingService;
      _apiClient = apiClient;
      _evolutionCustomerRepository = evolutionCustomerRepository;
    }

    public Result<CustomerMasterSyncResult> Execute(CustomerMasterSyncContext context) {
      _errorCount = 0;
      _processedIds.Clear();

      Logger.LogDebug("Start {SyncService} records sync.", this.GetType().Name);

      return ProcessQueueItems<CustomerUpdateQueue>(context)
        .Bind(() => BuildResult())
        .Finally(result => {
          Logger.LogDebug("End {SyncService} records sync.", this.GetType().Name);
          return result;
        });

      Result<CustomerMasterSyncResult> BuildResult() {
        return _errorCount == 0 ? Result.Success(new CustomerMasterSyncResult { }) : Result.Failure<CustomerMasterSyncResult>($"Completed with {_errorCount} errors.");
      }
    }

    protected override Result ProcessQueueItem(UpdateQueueBase updateQueueItem) {
      var queueItem = (CustomerUpdateQueue)updateQueueItem;

      if (_processedIds.Contains(queueItem.CustomerId)) {
        queueItem.Status = QueueProcessingStatus.Skipped;
        return Result.Success();
      }

      try {
        var evolutionCustomerResult = _evolutionCustomerRepository.Get(new CustomerDescriptor { CustomerId = queueItem.CustomerId });

        if (evolutionCustomerResult.IsFailure) {
          return Result.Failure($"Could not load customer with ID {queueItem.CustomerId} from Evolution. ({evolutionCustomerResult.Error})");
        }

        var evolutionCustomer = evolutionCustomerResult.Value;
        var evolutionEmailAddress = $"{evolutionCustomer.GetUserField("ucARwcEmail") ?? ""}";

        if (string.IsNullOrWhiteSpace(evolutionEmailAddress)) {
          return Result.Failure($"Cannot process customer with ID {queueItem.CustomerId}. The customer does not have an email address in Wine Club E-mail field (ucARwcEmail)");
        }

        dynamic? customerMaster = null;

        var localMappingResult = _customerMasterLocalMappingService.GetLocalId(queueItem.CustomerId);

        if (localMappingResult.HasValue) { customerMaster = TryFindUsingLocalMapping(); }

        customerMaster ??= TryFindUsingEvolutionEmailAddress();

        var mustCreateCustomer = customerMaster == null;

        var customerName = $"{evolutionCustomer.Description.Trim()}";
        var customerNameFormatter = new CustomerNameFormatter(customerName);
        var customerFirstName = customerNameFormatter.FirstName;
        var customerLastName = customerNameFormatter.LastName;

        string? ValueOrNull(string? value) {
          return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        Commerce7CustomerId commerce7CustomerId(dynamic? c) => c != null ? Commerce7CustomerId.Parse($"{c!.id}") : Commerce7CustomerId.Empty;

        TelephoneNumber[]? TelephoneNumbersOrNull(string? telephone) {
          if (string.IsNullOrWhiteSpace(telephone)) { return null; }
          var phoneNumberUtil = PhoneNumbers.PhoneNumberUtil.GetInstance();
          var p = phoneNumberUtil.Parse(telephone, "ZA");
          var t = phoneNumberUtil.Format(p, PhoneNumbers.PhoneNumberFormat.NATIONAL);
          if (t.Length < 10) { return null; }
          return new TelephoneNumber[] { new TelephoneNumber { Phone = $"{t}" } };
        }

        if (mustCreateCustomer) {
          var customerMasterResult = _apiClient.CreateCustomerWithAddress(new CreateCustomerRecord {
            FirstName = $"{customerFirstName}",
            LastName = $"{customerLastName}",
            Address = ValueOrNull(evolutionCustomer.PhysicalAddress.Line1),
            Address2 = ValueOrNull(evolutionCustomer.PhysicalAddress.Line2),
            City = ValueOrNull(evolutionCustomer.PhysicalAddress.Line3),
            StateCode = ValueOrNull(evolutionCustomer.PhysicalAddress.Line4),
            ZipCode = ValueOrNull(evolutionCustomer.PhysicalAddress.PostalCode),
            Emails = new EmailAddress[] { new EmailAddress { Email = evolutionEmailAddress } },
            Phones = TelephoneNumbersOrNull(evolutionCustomer.Telephone)
          });

          if (customerMasterResult.IsFailure) {
            return Result.Failure($"Could not create customer with ID {queueItem.CustomerId} in Commerce7. ({customerMasterResult.Error})");
          }

          customerMaster = customerMasterResult.Value.Data?.First();
        }
        else {
          var customerMasterResult = _apiClient.UpdateCustomer(new UpdateCustomerRecord {
            Id = commerce7CustomerId(customerMaster),
            FirstName = $"{customerFirstName}",
            LastName = $"{customerLastName}",
            Emails = new EmailAddress[] { new EmailAddress { Email = evolutionEmailAddress } },
            Phones = TelephoneNumbersOrNull(evolutionCustomer.Telephone)
          })
          .Map(a => {
            var customerDefaultAddressResult = _apiClient.GetCustomerDefaultAddress(commerce7CustomerId(customerMaster));

            if (customerDefaultAddressResult.Value.DefaultAddress == null) {
              return Result.Failure<CustomerAddressResponse>("Could not load customer default address from Commerce7.");
            }
            var customerDefaultAddressId = customerDefaultAddressResult.Value.DefaultAddress!.id;

            var data = new UpdateCustomerAddressRecord {
              Id = commerce7CustomerId(customerMaster),
              AddressId = Commerce7CustomerId.Parse($"{customerDefaultAddressId}"),
              Address = ValueOrNull(evolutionCustomer.PhysicalAddress.Line1),
              Address2 = ValueOrNull(evolutionCustomer.PhysicalAddress.Line2),
              City = ValueOrNull(evolutionCustomer.PhysicalAddress.Line3),
              StateCode = ValueOrNull(evolutionCustomer.PhysicalAddress.Line4),
              ZipCode = ValueOrNull(evolutionCustomer.PhysicalAddress.PostalCode),
            };

            return _apiClient.UpdateCustomerAddress(data);
          })
          ;

          if (customerMasterResult.IsFailure) {
            return Result.Failure($"Could not update customer with ID {queueItem.CustomerId} in Commerce7. ({customerMasterResult.Error})");
          }
        }

        if (customerMaster == null) {
          Logger.LogWarning("Did not receive a customer response object from Commerce7 for queue ID: {queueId}", queueItem.Oid);
          return Result.Success();
        }

        _customerMasterLocalMappingService.StoreMapping(queueItem.CustomerId, commerce7CustomerId(customerMaster));

        return Result.Success();

        dynamic? TryFindUsingLocalMapping() {
          var commerce7CustomerId = localMappingResult.Value;
          var commerce7Customer = _apiClient.GetCustomerMasterById(commerce7CustomerId);

          if (commerce7Customer.IsFailure) {
            if (commerce7Customer.Error.Contains("404")) {
              _customerMasterLocalMappingService.DeleteMapping(queueItem.CustomerId);
              return null;
            }

            throw new Exception(commerce7Customer.Error);
          }

          dynamic? customer = commerce7Customer.Value.Data?.FirstOrDefault();

          if (customer == null) { _customerMasterLocalMappingService.DeleteMapping(queueItem.CustomerId); }

          return customer;
        }

        dynamic? TryFindUsingEvolutionEmailAddress() {
          if (string.IsNullOrWhiteSpace(evolutionEmailAddress)) { return null; }
          var commerce7Customer = _apiClient.GetCustomerMasterByEmail(evolutionEmailAddress);
          return commerce7Customer.IsFailure ? throw new Exception(commerce7Customer.Error) : commerce7Customer.Value.Data?.FirstOrDefault();
        }
      }
      finally {
        _processedIds.Add(queueItem.CustomerId);
      }
    }
  }
}