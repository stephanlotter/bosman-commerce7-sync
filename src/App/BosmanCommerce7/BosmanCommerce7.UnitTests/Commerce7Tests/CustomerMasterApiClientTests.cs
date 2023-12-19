/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-18
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.CustomerMasterSyncServices.Models;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.CustomerMasterSyncServices.RestApi;

namespace BosmanCommerce7.UnitTests.Commerce7Tests {
  public class CustomerMasterApiClientTests : Commerce7ClientApiTestsBase {

    private Commerce7CustomerId CustomerId { get; } = Commerce7CustomerId.Parse("d1a43fe5-2ed8-4048-bd29-99d1a0dbc9d0");

    public CustomerMasterApiClientTests() {
    }

    [Fact]
    public void Fetch_customer() {
      var sut = new CustomerMasterApiClient(A.Fake<ILogger<CustomerMasterApiClient>>(), ApiClientService);

      var result = sut.GetCustomerMasterById(CustomerId);

      result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Fetch_customer_by_email() {
      var sut = new CustomerMasterApiClient(A.Fake<ILogger<CustomerMasterApiClient>>(), ApiClientService);

      var result = sut.GetCustomerMasterByEmail("stephan@neurasoft.co.za");

      result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Fetch_customer_default_address() {
      var sut = new CustomerMasterApiClient(A.Fake<ILogger<CustomerMasterApiClient>>(), ApiClientService);

      var result = sut.GetCustomerDefaultAddress(CustomerId);

      result.IsSuccess.Should().BeTrue();

      result.Value.HasData.Should().BeTrue();
      Commerce7CustomerId.Parse($"{result.Value.DefaultAddress!.customerId}").Should().Be(CustomerId);
    }

    [Fact]
    public void Update_customer_default_address() {
      var sut = new CustomerMasterApiClient(A.Fake<ILogger<CustomerMasterApiClient>>(), ApiClientService);

      var result = sut.GetCustomerDefaultAddress(CustomerId);
      result.IsSuccess.Should().BeTrue();
      result.Value.HasData.Should().BeTrue();
      Commerce7CustomerId.Parse($"{result.Value.DefaultAddress!.customerId}").Should().Be(CustomerId);

      var customerAddress = result.Value.DefaultAddress!;

      string NextZipCode(string zipCode) {
        return $"{int.Parse(zipCode) + 1}".PadLeft(4, '0');
      }

      var newZipCode = NextZipCode($"{customerAddress.zipCode}");

      var customerAddressId = customerAddress.id;

      var data = new UpdateCustomerAddressRecord {
        Id = CustomerId,
        AddressId = Commerce7CustomerId.Parse($"{customerAddressId}"),
        ZipCode = newZipCode,
        Address = customerAddress.address,
        City = customerAddress.city,
      };

      var updateResult = sut.UpdateCustomerAddress(data);

      updateResult.IsSuccess.Should().BeTrue();

      result = sut.GetCustomerDefaultAddress(CustomerId);
      result.IsSuccess.Should().BeTrue();
      result.Value.HasData.Should().BeTrue();
      customerAddress = result.Value.DefaultAddress!;

      $"{customerAddress.zipCode}".Should().Be(newZipCode);
    }
  }
}