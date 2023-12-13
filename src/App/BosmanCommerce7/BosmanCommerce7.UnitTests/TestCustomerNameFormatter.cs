/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-13
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.CustomerMasterSyncServices;

namespace BosmanCommerce7.UnitTests {

  public class TestCustomerNameFormatter {

    [Fact]
    public void Test_1() {
      var customerName = " Stephan Lotter  ";
      var customerNameFormatter = new CustomerNameFormatter(customerName);

      customerNameFormatter.FirstName.Should().Be("Stephan");
      customerNameFormatter.LastName.Should().Be("Lotter");
    }

    [Fact]
    public void Test_2() {
      var customerName = "Stephan ";
      var customerNameFormatter = new CustomerNameFormatter(customerName);

      customerNameFormatter.FirstName.Should().Be("Stephan");
      customerNameFormatter.LastName.Should().Be(CustomerNameFormatter.NoLastName);
    }

    [Fact]
    public void Test_3() {
      var customerName = "  Stephan ";
      var customerNameFormatter = new CustomerNameFormatter(customerName);

      customerNameFormatter.FirstName.Should().Be("Stephan");
      customerNameFormatter.LastName.Should().Be(CustomerNameFormatter.NoLastName);
    }

    [Fact]
    public void Test_4() {
      var customerName = "  ";
      var customerNameFormatter = new CustomerNameFormatter(customerName);

      customerNameFormatter.FirstName.Should().Be(CustomerNameFormatter.NoFirstName);
      customerNameFormatter.LastName.Should().Be(CustomerNameFormatter.NoLastName);
    }

    [Fact]
    public void Test_5() {
      string? customerName = null;
      var customerNameFormatter = new CustomerNameFormatter(customerName);

      customerNameFormatter.FirstName.Should().Be(CustomerNameFormatter.NoFirstName);
      customerNameFormatter.LastName.Should().Be(CustomerNameFormatter.NoLastName);
    }

    [Fact]
    public void Test_6a() {
      var customerName = "  Robert & Fiona Hollins ";
      var customerNameFormatter = new CustomerNameFormatter(customerName);

      customerNameFormatter.FirstName.Should().Be("Robert & Fiona");
      customerNameFormatter.LastName.Should().Be("Hollins");
    }

    [Fact]
    public void Test_6b() {
      var customerName = "  Robert &";
      var customerNameFormatter = new CustomerNameFormatter(customerName);

      customerNameFormatter.FirstName.Should().Be("Robert");
      customerNameFormatter.LastName.Should().Be(CustomerNameFormatter.NoLastName);
    }

    [Fact]
    public void Test_6c() {
      var customerName = " & Fiona Hollins ";
      var customerNameFormatter = new CustomerNameFormatter(customerName);

      customerNameFormatter.FirstName.Should().Be("Fiona");
      customerNameFormatter.LastName.Should().Be("Hollins");
    }

    [Fact]
    public void Test_7a() {
      var customerName = "Joe C Bloggs";
      var customerNameFormatter = new CustomerNameFormatter(customerName);

      customerNameFormatter.FirstName.Should().Be("Joe");
      customerNameFormatter.LastName.Should().Be("Bloggs");
    }

    [Fact]
    public void Test_7b() {
      var customerName = "Joe C. Bloggs";
      var customerNameFormatter = new CustomerNameFormatter(customerName);

      customerNameFormatter.FirstName.Should().Be("Joe");
      customerNameFormatter.LastName.Should().Be("Bloggs");
    }

    [Theory]
    [InlineData("Bosman Adama (Pty) Ltd t/a Leeurivier", "Bosman Adama (Pty) Ltd t/a Leeurivier", CustomerNameFormatter.NoLastName)]
    [InlineData("Five Wood Enterprises T/A Talisman Hire Hermanus", "Five Wood Enterprises T/A Talisman Hire Hermanus", CustomerNameFormatter.NoLastName)]
    [InlineData("African Premium Brands (Pty) Ltd", "African Premium Brands (Pty) Ltd", CustomerNameFormatter.NoLastName)]
    [InlineData("Advendurance CC", "Advendurance CC", CustomerNameFormatter.NoLastName)]
    [InlineData("Ed Sccaunders", "Ed", "Sccaunders")]
    [InlineData("Ed SaundersCC", "Ed", "SaundersCC")]
    [InlineData("Ed Saunders C/O Knife Restaurant", "Ed Saunders C/O Knife Restaurant", CustomerNameFormatter.NoLastName)]
    [InlineData("Exclusive Escape 5 Mountains Lodg", "Exclusive Escape 5 Mountains Lodg", CustomerNameFormatter.NoLastName)]
    public void Test_8(string customerName, string firstName, string lastName) {
      var customerNameFormatter = new CustomerNameFormatter(customerName);

      customerNameFormatter.FirstName.Should().Be(firstName);
      customerNameFormatter.LastName.Should().Be(lastName);
    }
  }
}