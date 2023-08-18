/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-18
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using BosmanCommerce7.Module.ApplicationServices.DataAccess.LocalDatabaseDataAccess;
using BosmanCommerce7.Module.Models;

namespace BosmanCommerce7.UnitTests.ValueStoreTests {
  public class TestValueStore {

    readonly IValueStoreRepository _valueStoreRepository = default!;

    public TestValueStore() {
      var logger = A.Fake<ILogger<ValueStoreRepository>>();
      var connectionStringProvider = A.Fake<ILocalDatabaseConnectionStringProvider>();
      A.CallTo(() => connectionStringProvider.LocalDatabase).Returns("Data Source=.;Initial Catalog=BosmanCommerce7;Integrated Security=True");
      _valueStoreRepository = new ValueStoreRepository(logger, connectionStringProvider);
    }

    [Fact]
    public void Get_value_test_1() {
      var result = _valueStoreRepository.GetValue("unit-test-key");

      result.IsSuccess.Should().BeTrue();
      result.Value.Should().Be("the value1");
    }

    [Fact]
    public void Get_value_test_2() {
      var expectedValue = DateTime.Parse("2023-08-01 09:35:16");
      var result = _valueStoreRepository.GetDateTimeValue("unit-test-key-date-time");

      result.IsSuccess.Should().BeTrue();
      result.Value.Should().Be(expectedValue);
    }

    [Fact]
    public void Set_value_test_1() {
      var keyName = $"unit-test-key-for-set-{DateTime.Now:yyyyMMddHHmmss}";
      var keyValue = Guid.NewGuid().ToString();

      _valueStoreRepository.SetValue(keyName, keyValue);
      var result = _valueStoreRepository.GetValue(keyName);

      result.IsSuccess.Should().BeTrue();
      result.Value.Should().Be(keyValue);
    }

    [Fact]
    public void Set_value_test_2() {
      var expectedValue = DateTime.Now;
      var keyName = $"unit-test-key-for-set-datetime";
      var keyValue = expectedValue;

      _valueStoreRepository.SetDateTimeValue(keyName, keyValue);
      var result = _valueStoreRepository.GetDateTimeValue(keyName);

      result.IsSuccess.Should().BeTrue();
      result.Value.Should().Be(keyValue);
    }

    [Fact]
    public void Delete_key_test_1() {
      var keyName = $"unit-test-key-for-delete-{DateTime.Now:yyyyMMddHHmmss}";
      var keyValue = Guid.NewGuid().ToString();

      _valueStoreRepository.SetValue(keyName, keyValue);
      var result = _valueStoreRepository.GetValue(keyName);

      result.IsSuccess.Should().BeTrue();
      result.Value.Should().Be(keyValue);

      var keyExistsResult = _valueStoreRepository.KeyExists(keyName);
      keyExistsResult.IsSuccess.Should().BeTrue();
      keyExistsResult.Value.Should().BeTrue();

      var deleteResult = _valueStoreRepository.DeleteKey(keyName);
      deleteResult.IsSuccess.Should().BeTrue();

      keyExistsResult = _valueStoreRepository.KeyExists(keyName);
      keyExistsResult.IsSuccess.Should().BeTrue();
      keyExistsResult.Value.Should().BeFalse();

    }
  }
}