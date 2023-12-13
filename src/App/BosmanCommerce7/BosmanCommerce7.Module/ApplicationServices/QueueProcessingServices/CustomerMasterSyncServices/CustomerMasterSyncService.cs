﻿/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-11-28
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.DataAccess.LocalDatabaseDataAccess;
using BosmanCommerce7.Module.ApplicationServices.EvolutionSdk;
using BosmanCommerce7.Module.ApplicationServices.EvolutionSdk.Customers;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.CustomerMasterSyncServices.Models;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.CustomerMasterSyncServices.RestApi;
using BosmanCommerce7.Module.BusinessObjects;
using BosmanCommerce7.Module.BusinessObjects.Customers;
using BosmanCommerce7.Module.Models;
using BosmanCommerce7.Module.Models.EvolutionSdk.Customers;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.CustomerMasterSyncServices {

  public class CustomerMasterSyncService : SyncMasterDataServiceBase, ICustomerMasterSyncService {
    private readonly CustomerMasterSyncJobOptions _customerMasterSyncJobOptions;
    private readonly ICustomerMasterLocalMappingService _customerMasterLocalMappingService;
    private readonly ICustomerMasterApiClient _apiClient;
    private readonly IEvolutionCustomerRepository _evolutionCustomerRepository;

    private List<EvolutionCustomerId> _processedCustomerIds = new();

    public CustomerMasterSyncService(ILogger<CustomerMasterSyncService> logger,
      CustomerMasterSyncJobOptions customerMasterSyncJobOptions,
      ICustomerMasterLocalMappingService customerMasterLocalMappingService,
      ICustomerMasterApiClient apiClient,
      IEvolutionCustomerRepository evolutionCustomerRepository,
      ILocalObjectSpaceProvider localObjectSpaceProvider,
      IEvolutionSdk evolutionSdk
      ) : base(logger, localObjectSpaceProvider, evolutionSdk) {
      _customerMasterSyncJobOptions = customerMasterSyncJobOptions;
      _customerMasterLocalMappingService = customerMasterLocalMappingService;
      _apiClient = apiClient;
      _evolutionCustomerRepository = evolutionCustomerRepository;
    }

    public Result<CustomerMasterSyncResult> Execute(CustomerMasterSyncContext context) {
      _errorCount = 0;
      _processedCustomerIds.Clear();
      Logger.LogDebug("Start {SyncService} master records sync.", typeof(CustomerMasterSyncService).Name);

      var result = ProcessQueueItems<CustomerUpdateQueue>(context);

      if (result.IsFailure) {
        return Result.Failure<CustomerMasterSyncResult>(result.Error);
      }

      Logger.LogDebug("End {SyncService} master records sync.", typeof(CustomerMasterSyncService).Name);

      return _errorCount == 0 ? Result.Success(BuildResult()) : Result.Failure<CustomerMasterSyncResult>($"Completed with {_errorCount} errors.");

      CustomerMasterSyncResult BuildResult() {
        return new CustomerMasterSyncResult { };
      }
    }

    protected override Result ProcessQueueItem(UpdateQueueBase updateQueueItem) {
      var queueItem = (CustomerUpdateQueue)updateQueueItem;

      if (_processedCustomerIds.Contains(queueItem.CustomerId)) { return Result.Success(); }

      var evolutionCustomerResult = _evolutionCustomerRepository.GetCustomer(new CustomerDescriptor { CustomerId = queueItem.CustomerId });

      if (evolutionCustomerResult.IsFailure) {
        return Result.Failure($"Could not load customer with ID {queueItem.CustomerId} from Evolution. ({evolutionCustomerResult.Error})");
      }

      var evolutionCustomer = evolutionCustomerResult.Value;
      var evolutionEmailAddress = $"{evolutionCustomer.GetUserField("ucARwcEmail") ?? ""}";

      dynamic? customerMaster = null;

      var localMappingResult = _customerMasterLocalMappingService.GetLocalId(queueItem.CustomerId);

      if (localMappingResult.HasValue) { customerMaster = TryFindUsingLocalMapping(); }

      customerMaster ??= TryFindUsingEvolutionEmailAddress();

      var createCustomer = customerMaster == null;

      var customerName = $"{evolutionCustomer.Description.Trim()}".Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
      var customerFirstName = customerName.First();
      var customerLastName = customerName.Length > 1 ? string.Join(" ", customerName.Skip(1)) : "No last name";

      if (createCustomer) {
        customerMaster = _apiClient.CreateCustomerWithAddress(new CreateCustomerRecord {
          FirstName = $"{customerFirstName}",
          LastName = $"{customerLastName}",
          Address = $"{evolutionCustomer.PhysicalAddress.Line1}",
          Address2 = $"{evolutionCustomer.PhysicalAddress.Line2}",
          City = $"{evolutionCustomer.PhysicalAddress.Line3}",
          StateCode = $"{evolutionCustomer.PhysicalAddress.Line4}",
          ZipCode = $"{evolutionCustomer.PhysicalAddress.PostalCode}",
          Emails = new EmailAddress[] { new EmailAddress { Email = evolutionEmailAddress } },
          Phones = new TelephoneNumber[] { new TelephoneNumber { Phone = $"{evolutionCustomer.Telephone}" } }
        });

        var customerMasterId = customerMaster.Id;
        _customerMasterLocalMappingService.StoreMapping(queueItem.CustomerId, customerMasterId);
      }
      else {
        // update the customer
      }

      _customerMasterLocalMappingService.StoreMapping(queueItem.CustomerId, customerMaster!.Id);

      _processedCustomerIds.Add(queueItem.CustomerId);

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

        dynamic? customer = commerce7Customer.Value.CustomerMasters?.FirstOrDefault();

        if (customer == null) { _customerMasterLocalMappingService.DeleteMapping(queueItem.CustomerId); }

        return customer;
      }

      dynamic? TryFindUsingEvolutionEmailAddress() {
        if (string.IsNullOrWhiteSpace(evolutionEmailAddress)) { return null; }
        var commerce7Customer = _apiClient.GetCustomerMasterByEmail(evolutionEmailAddress);
        return commerce7Customer.IsFailure ? throw new Exception(commerce7Customer.Error) : commerce7Customer.Value.CustomerMasters?.FirstOrDefault();
      }
    }
  }
}